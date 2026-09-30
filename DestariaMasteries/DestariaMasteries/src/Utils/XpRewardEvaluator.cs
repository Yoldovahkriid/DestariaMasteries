using MasteryLibrary;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Networking.Packets;
using MasteryLibrary.src.Networking.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Utils
{
    public enum BlockTypeEnum
    {
        Ore,
        UngradedOre,
        Crop,
        Mushroom,
        NoXP
    }

    public enum XpSourceCategory
    {
        Mining,
        Farming,
        Foraging,
        Woodcutting,
        Combat,
        AnimalHarvest,
        Social
    }

    public static class XpRewardEvaluator
    {
        private const string ConfigFileName = "MasteryXpConfig.json";
        public static MasteryXpConfig Config { get; private set; } = new MasteryXpConfig();
        private static NetworkServiceServer? NetworkService { get; set; } = null!;
        private static AssetLocation oreWildcard = new AssetLocation("*:ore-*-*-*");
        private static AssetLocation ungradedOreWildcard = new AssetLocation("*:ore-*-*");
        private static AssetLocation cropWildcard = new AssetLocation("*:crop-*-*");
        private static AssetLocation logWildcard = new AssetLocation("*:log-*-*-*");
        private static AssetLocation logSectionWildcard = new AssetLocation("*:logsection-*-*-*");
        private static AssetLocation mushroomWildcard = new AssetLocation("*:mushroom-*");

        public static void Initialize(ICoreServerAPI api)
        {
            try
            {
                Config = api.LoadModConfig<MasteryXpConfig>(ConfigFileName) ?? new MasteryXpConfig();
            }
            catch (Exception ex)
            {
                api.Logger.Error($"Failed to load {ConfigFileName}, falling back to default XP values: {ex}");
                Config = new MasteryXpConfig();
            }

            try
            {
                api.StoreModConfig(Config, ConfigFileName);
            }
            catch (Exception ex)
            {
                api.Logger.Error($"Failed to save {ConfigFileName}: {ex}");
            }

            oreWildcard = new AssetLocation(Config.OreWildcard);
            ungradedOreWildcard = new AssetLocation(Config.UngradedOreWildcard);
            cropWildcard = new AssetLocation(Config.CropWildcard);
            logWildcard = new AssetLocation(Config.LogWildcard);
            logSectionWildcard = new AssetLocation(Config.LogSectionWildcard);
            mushroomWildcard = new AssetLocation(Config.MushroomWildcard);

            NetworkService = api.ModLoader.GetModSystem<MasteryLibraryAPI>().NetworkService as NetworkServiceServer;
        }

        private static void GrantXpToPlayer(IServerPlayer byPlayer, float amount, EnumXpImageSourceType sourceImageType = EnumXpImageSourceType.Image, string? text = null, AssetLocation? sourceAsset = null)
        {
            if (byPlayer == null || amount <= 0f) return;

            EnumGameMode gm = byPlayer.WorldData.CurrentGameMode;
            if (gm == EnumGameMode.Creative || gm == EnumGameMode.Spectator)
            {
                return;
            }

            var data = byPlayer.Entity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            if (data == null) return; // nothing was actually granted, so don't log that it was

            data.GainExperience(amount);

            if (NetworkService != null)
            {
                NetworkService.SendXpPopUpPacket(byPlayer, amount, sourceImageType, text, sourceAsset);
            }
        }

        public static void OnBlockBroken(IServerPlayer byPlayer, BlockSelection blockSel, ref float dropQuantityMultiplier, ref EnumHandling handling)
        {
            Block block = blockSel.Block;
            if (block == null) return;

            BlockTypeEnum blockType = GetBlockType(block.Code);
            float xpToAward = GetExpForBlock(block, byPlayer, blockType);
            XpSourceCategory source = blockType == BlockTypeEnum.Crop ? XpSourceCategory.Farming : XpSourceCategory.Mining;

            string localizedBlockName = block.GetPlacedBlockName(byPlayer.Entity.World, blockSel.Position);
            if (xpToAward > 0)
            {
                byPlayer.Entity.Api.Logger.Audit($"Awarding {xpToAward} XP to player {byPlayer.PlayerName} for breaking block {block.Code} ({localizedBlockName}) of type {blockType} (source category: {source})");
            }
            GrantXpToPlayer(byPlayer, xpToAward, EnumXpImageSourceType.Block, localizedBlockName, block.Code);
        }

        private static BlockTypeEnum GetBlockType(AssetLocation block)
        {
            if (WildcardUtil.Match(oreWildcard, block))
            {
                return BlockTypeEnum.Ore;
            }
            else if (WildcardUtil.Match(ungradedOreWildcard, block))
            {
                return BlockTypeEnum.UngradedOre;
            }
            else if (WildcardUtil.Match(cropWildcard, block))
            {
                return BlockTypeEnum.Crop;
            }
            else if (WildcardUtil.Match(mushroomWildcard, block))
            {
                return BlockTypeEnum.Mushroom;
            }
            else return BlockTypeEnum.NoXP;
        }

        private static float GetExpForBlock(Block block, IServerPlayer byPlayer, BlockTypeEnum blockType)
        {
            return blockType switch
            {
                BlockTypeEnum.Ore => CalculateOreXP(block, byPlayer),
                BlockTypeEnum.UngradedOre => CalculateUngradedOreXP(block, byPlayer),
                BlockTypeEnum.Crop => CalculateCropXP(block, byPlayer),
                BlockTypeEnum.Mushroom => Config.MushroomXp,
                _ => 0f
            };
        }

        #region Ore XP Calculation
        private static Dictionary<string, string> materialToProductCache = new();
        private static Dictionary<string, bool> materialIsGemCache = new();

        private static float CalculateOreXP(Block block, IServerPlayer byPlayer)
        {
            IWorldAccessor world = byPlayer.Entity.World;

            if (!TryGetGradedOreParts(block.Code, out var quality, out var oreMaterial, out _))
            {
                return Config.DefaultOreXp;
            }

            string? product = ResolveSmeltedProduct(block, world);
            if (product != null)
            {
                float multiplier = (quality != null && Config.OreGradeMultiplier.TryGetValue(quality, out var mult)) ? mult : 1f;
                float baseXp = Config.ProductXp.TryGetValue(product, out var metalXpValue) ? metalXpValue : Config.DefaultOreXp;
                return baseXp * multiplier;
            }

            if (oreMaterial != null && IsGemMaterial(block.Code.Domain, oreMaterial, world))
            {
                float multiplier = (quality != null && Config.GemPotentialMultiplier.TryGetValue(quality, out var mult)) ? mult : 1f;
                float baseXp = Config.GemXp.TryGetValue(oreMaterial, out var gemXpValue) ? gemXpValue : Config.DefaultOreXp;
                return baseXp * multiplier;
            }

            byPlayer.Entity.Api.Logger.Warning($"Could not resolve smelted product or gem for ore block {block.Code}. Awarding default XP.");
            return Config.DefaultOreXp;
        }

        private static float CalculateUngradedOreXP(Block block, IServerPlayer byPlayer)
        {
            if (!TryGetUngradedOreParts(block.Code, out var material, out _))
                return Config.DefaultUngradedOreXp;

            return Config.UngradedOreXp.TryGetValue(material, out var xp) ? xp : Config.DefaultUngradedOreXp;
        }

        private static bool TryGetGradedOreParts(AssetLocation code, out string? quality, out string? material, out string? rock)
        {
            quality = material = rock = null;

            if (!WildcardUtil.Match(oreWildcard, code)) return false;

            string[] parts = code.Path.Split('-');
            if (parts.Length < 4) return false;

            quality = parts[1];
            material = parts[2];
            rock = parts[3];
            return true;
        }

        private static bool TryGetUngradedOreParts(AssetLocation code, out string? material, out string? rock)
        {
            material = rock = null;

            if (!WildcardUtil.Match(ungradedOreWildcard, code)) return false;

            string[] parts = code.Path.Split('-');
            if (parts.Length < 3) return false;

            material = parts[1];
            rock = parts[2];
            return true;
        }

        private static string? ResolveSmeltedProduct(Block oreBlock, IWorldAccessor world)
        {
            if (!TryGetGradedOreParts(oreBlock.Code, out _, out var oreMaterial, out _))
                return null;
            if (oreMaterial == null) return null;

            string cacheKey = $"{oreBlock.Code.Domain}:{oreMaterial}";
            if (materialToProductCache.TryGetValue(cacheKey, out var cachedProduct))
                return cachedProduct;

            string? product = ResolveViaNugget(oreBlock.Code.Domain, oreMaterial, world);
            if (product == null) return null;

            materialToProductCache[cacheKey] = product;
            return product;
        }

        private static string? ResolveViaNugget(string oreDomain, string oreMaterial, IWorldAccessor world)
        {
            string nuggetMaterial = Config.OreMaterialToNuggetMaterial.TryGetValue(oreMaterial, out var overrideMaterial)
                ? overrideMaterial
                : oreMaterial;

            Item? nuggetItem = world.GetItem(new AssetLocation(oreDomain, $"nugget-{nuggetMaterial}"));

            if (nuggetItem == null && oreDomain != "game")
            {
                nuggetItem = world.GetItem(new AssetLocation("game", $"nugget-{nuggetMaterial}"));
            }

            if (nuggetItem == null) return null;

            ItemStack nuggetStack = new ItemStack(nuggetItem);
            CombustibleProperties? combProps = nuggetItem.GetCombustibleProperties(world, nuggetStack, null);
            if (combProps?.SmeltedStack == null) return null;

            combProps.SmeltedStack.Resolve(world, "ore xp lookup");
            return combProps.SmeltedStack.ResolvedItemstack?.Collectible?.Code?.Path;
        }

        private static bool IsGemMaterial(string oreDomain, string oreMaterial, IWorldAccessor world)
        {
            string cacheKey = $"{oreDomain}:{oreMaterial}";
            if (materialIsGemCache.TryGetValue(cacheKey, out var cached)) return cached;

            Item? gemItem = world.GetItem(new AssetLocation(oreDomain, $"gem-{oreMaterial}-rough"));
            if (gemItem == null && oreDomain != "game")
            {
                gemItem = world.GetItem(new AssetLocation("game", $"gem-{oreMaterial}-rough"));
            }

            bool isGem = gemItem != null;
            materialIsGemCache[cacheKey] = isGem;
            return isGem;
        }
        #endregion

        #region Crop XP Calculation
        private static float CalculateCropXP(Block block, IServerPlayer byPlayer)
        {
            BlockCropProperties cropProps = block.CropProps;
            if (cropProps == null) return 0f;

            string[] parts = block.Code.Path.Split('-');
            if (parts.Length < 3 || !int.TryParse(parts[parts.Length - 1], out int currentStage))
            {
                return 0f;
            }

            if (currentStage < cropProps.GrowthStages)
            {
                return 0f;
            }

            float totalGrowthMonths = cropProps.GrowthStages * cropProps.TotalGrowthMonths;

            // xp = a(months)^2 + b(months) + c
            float a = Config.CropXpFormula.A;
            float b = Config.CropXpFormula.B;
            float c = Config.CropXpFormula.C;

            float xp = (a * totalGrowthMonths * totalGrowthMonths) + (b * totalGrowthMonths) + c;

            return MathF.Floor(MathF.Max(xp, 1f));
        }
        #endregion
        #region Harvest XP Calculation
        public static void OnHarvest(IServerPlayer byPlayer, float basexp, BlockPos pos, AssetLocation? sourceAsset = null)
        {
            string? blockName = null;
            if (sourceAsset != null)
            {
                Block block = byPlayer.Entity.World.BlockAccessor.GetBlock(pos);
                blockName = block != null ? block.GetPlacedBlockName(byPlayer.Entity.World, pos) : new ItemStack(byPlayer.Entity.World.GetBlock(sourceAsset)).GetName();
            }

            byPlayer.Entity.Api.Logger.Audit($"Awarding {basexp} XP to player {byPlayer.PlayerName} for harvesting at position {pos} (block: {blockName}, source asset: {sourceAsset})");
            GrantXpToPlayer(byPlayer, basexp, EnumXpImageSourceType.Block, blockName, sourceAsset);
        }
        #endregion

        #region Tree/Log XP Calculation
        public static float GetXpForTreeBlock(Block block)
        {
            if (block == null) return 0f;
            if (!WildcardUtil.Match(logWildcard, block.Code) && !WildcardUtil.Match(logSectionWildcard, block.Code)) return 0f;

            string[] parts = block.Code.Path.Split('-');
            if (parts.Length < 3) return Config.DefaultLogXp;

            string species = parts[2];
            return Config.LogXp.TryGetValue(species, out var xp) ? xp : Config.DefaultLogXp;
        }

        public static void OnTreeHarvest(IServerPlayer byPlayer, float basexp, Block block = null, BlockPos pos = null, AssetLocation? source = null)
        {
            string? blockName = null;
            if (block != null && pos != null)
            {
                blockName = block.GetPlacedBlockName(byPlayer.Entity.World, pos);
            }
            else if (source != null)
            {
                blockName = new ItemStack(byPlayer.Entity.World.GetBlock(source)).GetName();
            }

            byPlayer.Entity.Api.Logger.Audit($"Awarding {basexp} XP to player {byPlayer.PlayerName} for harvesting tree/log (block: {blockName}, source asset: {source})");
            GrantXpToPlayer(byPlayer, basexp, EnumXpImageSourceType.Block, blockName, source);
        }
        #endregion

        #region Entity Death XP Calculation
        public static void OnEntityDeath(Entity entity, DamageSource damageSource)
        {
            if (entity == null || damageSource == null) return;
            if (damageSource.GetCauseEntity() is not EntityPlayer byPlayer) return;

            IServerPlayer? player = byPlayer.Player as IServerPlayer;
            if (player == null) return;

            float? healthBasedXp = entity.GetBehavior<EntityBehaviorHealth>()?.MaxHealth * Config.XpPerHealthPoint;
            float xpToAward = healthBasedXp ?? Config.DefaultCombatXp;

            byPlayer.Api.Logger.Audit($"Awarding {xpToAward} XP to player {player.PlayerName} for killing entity {entity.Code} ({entity.GetName()})");
            GrantXpToPlayer(player, xpToAward, EnumXpImageSourceType.Entity, entity.GetName(), entity.Code);
        }
        #endregion

        #region Entity Harvest XP Calculation
        public static void OnEntityHarvest(Entity entity, IServerPlayer byPlayer)
        {
            if (entity == null || byPlayer == null) return;
            byPlayer.Entity.Api.Logger.Audit($"Awarding {Config.AnimalHarvestXp} XP to player {byPlayer.PlayerName} for harvesting entity {entity.Code} ({entity.GetName()})");
            GrantXpToPlayer(byPlayer, Config.AnimalHarvestXp, EnumXpImageSourceType.Entity, entity.GetName(), entity.Code);
        }
        #endregion

        #region Social XP Calculation
        // This method assumes that the caller already culled inactive players
        public static void GrantSocialXp(IServerPlayer player, int amount)
        {
            float xpToAward = Config.SocialXpPerPlayer * amount;
            player.Entity.Api.Logger.Audit($"Awarding {xpToAward} XP to player {player.PlayerName} for social interaction with {amount} other players");
            GrantXpToPlayer(player, xpToAward, EnumXpImageSourceType.Image, Lang.Get("destariamasteries:xpsource-social"), new AssetLocation("destariamasteries:textures/gui/xp-social.png"));
        }
        #endregion
    }
}
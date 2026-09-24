using MasteryLibrary.src.Behaviors.EntityBehaviors;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
        private static AssetLocation oreWildcard = new AssetLocation("*:ore-*-*-*");
        private static AssetLocation ungradedOreWildcard = new AssetLocation("*:ore-*-*");
        private static AssetLocation cropWildcard = new AssetLocation("*:crop-*-*");
        private static AssetLocation logWildcard = new AssetLocation("*:log-*-*-*");
        private static AssetLocation logSectionWildcard = new AssetLocation("*:logsection-*-*-*");

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
        }

        private static void GrantXpToPlayer(IServerPlayer byPlayer, float amount, XpSourceCategory source, string detail = null)
        {
            if (byPlayer == null || amount <= 0f) return;

            EnumGameMode gm = byPlayer.WorldData.CurrentGameMode;
            if (gm == EnumGameMode.Creative || gm == EnumGameMode.Spectator)
            {
                byPlayer.Entity?.Api.Logger.Event($"Not awarding XP to {byPlayer.PlayerName} for {source} because they are in {gm} mode.");
                return;
            }

            var data = byPlayer.Entity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            if (data == null) return; // nothing was actually granted, so don't log that it was

            string suffix = string.IsNullOrEmpty(detail) ? string.Empty : $" ({detail})";
            byPlayer.Entity?.Api.Logger.Event($"Awarded {amount} XP to {byPlayer.PlayerName} for {source}{suffix}");

            data.GainExperience(amount);
        }

        public static void OnBlockBroken(IServerPlayer byPlayer, BlockSelection blockSel, ref float dropQuantityMultiplier, ref EnumHandling handling)
        {
            Block block = blockSel.Block;
            if (block == null) return;

            BlockTypeEnum blockType = GetBlockType(block.Code);
            float xpToAward = GetExpForBlock(block, byPlayer, blockType);
            XpSourceCategory source = blockType == BlockTypeEnum.Crop ? XpSourceCategory.Farming : XpSourceCategory.Mining;

            GrantXpToPlayer(byPlayer, xpToAward, source, block.Code?.ToString());
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
            else return BlockTypeEnum.NoXP;
        }

        private static float GetExpForBlock(Block block, IServerPlayer byPlayer, BlockTypeEnum blockType)
        {
            return blockType switch
            {
                BlockTypeEnum.Ore => CalculateOreXP(block, byPlayer),
                BlockTypeEnum.UngradedOre => CalculateUngradedOreXP(block, byPlayer),
                BlockTypeEnum.Crop => CalculateCropXP(block, byPlayer),
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
        public static void OnHarvest(IServerPlayer byPlayer, float basexp)
        {
            GrantXpToPlayer(byPlayer, basexp, XpSourceCategory.Foraging);
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

        public static void OnTreeHarvest(IServerPlayer byPlayer, float basexp)
        {
            GrantXpToPlayer(byPlayer, basexp, XpSourceCategory.Woodcutting);
        }
        #endregion

        #region Entity Death XP Calculation
        public static void OnEntityDeath(Entity entity, DamageSource damageSource)
        {
            if (entity == null || damageSource == null) return;
            if (damageSource.SourceEntity is not IServerPlayer byPlayer) return;

            float? healthBasedXp = entity.GetBehavior<EntityBehaviorHealth>()?.MaxHealth * Config.XpPerHealthPoint;
            float xpToAward = healthBasedXp ?? Config.DefaultCombatXp;

            GrantXpToPlayer(byPlayer, xpToAward, XpSourceCategory.Combat, entity.Code?.ToString());
        }
        #endregion

        #region Entity Harvest XP Calculation
        public static void OnEntityHarvest(Entity entity, IServerPlayer byPlayer)
        {
            if (entity == null || byPlayer == null) return;
            GrantXpToPlayer(byPlayer, Config.AnimalHarvestXp, XpSourceCategory.AnimalHarvest, entity.Code?.ToString());
        }
        #endregion

        #region Social XP Calculation
        // This method assumes that the caller already culled inactive players
        public static void GrantSocialXp(IServerPlayer player, int amount)
        {
            float xpToAward = Config.SocialXpPerPlayer * amount;
            GrantXpToPlayer(player, xpToAward, XpSourceCategory.Social, $"{amount} nearby players");
        }
        #endregion
    }
}
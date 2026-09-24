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
        private static void GrantXpToPlayer(IServerPlayer byPlayer, float amount, XpSourceCategory source, string detail = null)
        {
            if (byPlayer == null || amount <= 0f) return;

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
        private static AssetLocation oreWildcard = new AssetLocation("*:ore-*-*-*");
        private static AssetLocation ungradedOreWildcard = new AssetLocation("*:ore-*-*");
        static Dictionary<string, float> productXp = new()
        {
            { "ironbloom", 25f },
            { "ingot-copper", 5f },
            { "ingot-gold", 100f },
            { "ingot-lead", 35f },
            { "ingot-tin", 35f },
            { "ingot-chromium", 75f },
            { "ingot-platinum", 75f },
            { "ingot-titanium", 75f },
            { "ingot-zinc", 35f },
            { "ingot-silver", 50f },
            { "ingot-bismuth", 35f },
            { "ingot-nickel", 50f },
        };

        private static Dictionary<string, float> gemXp = new()
        {
            { "emerald", 100f },
            { "diamond", 200f },
            { "olivine_peridot", 50f },
        };

        private static Dictionary<string, float> ungradedOreXp = new()
        {
            { "flint", 5f },
            { "quartz", 1f },
            { "alum", 20f },
            { "stibnite", 20f },
            { "lignite", 5f },
            { "bituminouscoal", 5f },
            { "anthracite", 10f },
            { "sulfur", 5f },
            { "sylvite", 50f },
            { "borax", 5f },
            { "kernite", 5f },
            { "cinnabar", 25f },
            { "corundum", 5f },
            { "lapislazuli", 50f },
            { "olivine", 1f },
            { "fluorite", 5f },
            { "phosphorite", 5f }
        };
        private static Dictionary<string, float> oreGradeMultiplier = new()
        {
            { "poor", 1f },
            { "medium", 1.5f },
            { "rich", 2f },
            { "bountiful", 3f },
        };
        private static Dictionary<string, float> gemPotentialMultiplier = new()
        {
            { "low", 1f },
            { "medium", 2f },
            { "high", 3f },
        };
        private static Dictionary<string, string> materialToProductCache = new();
        private static Dictionary<string, bool> materialIsGemCache = new();
        private static Dictionary<string, string> oreMaterialToNuggetMaterial = new()
        {
            { "quartz_nativegold", "nativegold" },
            { "galena_nativesilver", "nativesilver" },
            { "quartz_nativesilver", "nativesilver" },
        };

        private static float CalculateOreXP(Block block, IServerPlayer byPlayer)
        {
            IWorldAccessor world = byPlayer.Entity.World;

            if (!TryGetGradedOreParts(block.Code, out var quality, out var oreMaterial, out _))
            {
                return 1f;
            }

            string? product = ResolveSmeltedProduct(block, world);
            if (product != null)
            {
                float multiplier = (quality != null && oreGradeMultiplier.TryGetValue(quality, out var mult)) ? mult : 1f;
                float baseXp = productXp.TryGetValue(product, out var metalXpValue) ? metalXpValue : 1f;
                return baseXp * multiplier;
            }

            if (oreMaterial != null && IsGemMaterial(block.Code.Domain, oreMaterial, world))
            {
                float multiplier = (quality != null && gemPotentialMultiplier.TryGetValue(quality, out var mult)) ? mult : 1f;
                float baseXp = gemXp.TryGetValue(oreMaterial, out var gemXpValue) ? gemXpValue : 1f;
                return baseXp * multiplier;
            }

            byPlayer.Entity.Api.Logger.Warning($"Could not resolve smelted product or gem for ore block {block.Code}. Awarding default XP.");
            return 1f;
        }

        private static float CalculateUngradedOreXP(Block block, IServerPlayer byPlayer)
        {
            if (!TryGetUngradedOreParts(block.Code, out var material, out _))
                return 1f;

            return ungradedOreXp.TryGetValue(material, out var xp) ? xp : 1f;
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
            string nuggetMaterial = oreMaterialToNuggetMaterial.TryGetValue(oreMaterial, out var overrideMaterial)
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
        private static AssetLocation cropWildcard = new AssetLocation("*:crop-*-*");
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

            // a(months)^2+b(months)+c
            float a = 0.05f;
            float b = 0.5f;
            float c = 10.0f;

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
        private static AssetLocation logWildcard = new AssetLocation("*:log-*-*-*");
        private static AssetLocation logSectionWildcard = new AssetLocation("*:logsection-*-*-*");

        private static Dictionary<string, float> logXp = new()
        {
            { "birch", 2f },
            { "oak", 2f },
            { "maple", 2f },
            { "pine", 2f },
            { "acacia", 2f },
            { "kapok", 2f },
            { "baldcypress", 2f },
            { "larch", 2f },
            { "redwood", 2f },
            { "ebony", 8f },
            { "walnut", 2f },
            { "purpleheart", 8f }
        };

        public static float GetXpForTreeBlock(Block block)
        {
            if (block == null) return 0f;
            if (!WildcardUtil.Match(logWildcard, block.Code) && !WildcardUtil.Match(logSectionWildcard, block.Code)) return 0f;

            string[] parts = block.Code.Path.Split('-');
            if (parts.Length < 3) return 1f;

            string species = parts[2];
            return logXp.TryGetValue(species, out var xp) ? xp : 2f;
        }

        public static void OnTreeHarvest(IServerPlayer byPlayer, float basexp)
        {
            GrantXpToPlayer(byPlayer, basexp, XpSourceCategory.Woodcutting);
        }
        #endregion

        #region Entity Death XP Calculation
        private static float XpPerHealthPoint = 1.5f; // 1.5 XP per health point of the entity
        public static void OnEntityDeath(Entity entity, DamageSource damageSource)
        {
            if (entity == null || damageSource == null) return;
            if (damageSource.SourceEntity is not IServerPlayer byPlayer) return;

            float? healthBasedXp = entity.GetBehavior<EntityBehaviorHealth>()?.MaxHealth * XpPerHealthPoint;
            float xpToAward = healthBasedXp ?? 1f;

            GrantXpToPlayer(byPlayer, xpToAward, XpSourceCategory.Combat, entity.Code?.ToString());
        }
        #endregion

        #region Entity Harvest XP Calculation
        private static float HarvestXp = 20f;
        public static void OnEntityHarvest(Entity entity, IServerPlayer byPlayer)
        {
            if (entity == null || byPlayer == null) return;
            GrantXpToPlayer(byPlayer, HarvestXp, XpSourceCategory.AnimalHarvest, entity.Code?.ToString());
        }
        #endregion

        #region Social XP Calculation
        private static float SocialXpPerPlayer = 5f; // 5 XP per tick for social interaction

        // This method assumes that the caller already culled inactive players
        public static void GrantSocialXp(IServerPlayer player, int amount)
        {
            float xpToAward = SocialXpPerPlayer * amount;
            GrantXpToPlayer(player, xpToAward, XpSourceCategory.Social, $"{amount} nearby players");
        }
        #endregion
    }
}
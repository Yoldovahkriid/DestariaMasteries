using System.Collections.Generic;

namespace DestariaMasteries.src.Utils
{
    public class MasteryXpConfig
    {
        //Block-code wildcards used to classify broken blocks
        public string OreWildcard = "*:ore-*-*-*";
        public string UngradedOreWildcard = "*:ore-*-*";
        public string MeteoriteWildcard = "*:meteorite-*";
        public string CropWildcard = "*:crop-*-*";
        public string LogWildcard = "*:log-*-*-*";
        public string LogSectionWildcard = "*:logsection-*-*-*";
        public string MushroomWildcard = "*:mushroom-*";

        // Ore -> smelted product base XP
        public Dictionary<string, float> ProductXp = new()
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
            { "ingot-uranium", 45f },

        };

        // Gem base XP
        public Dictionary<string, float> GemXp = new()
        {
            { "emerald", 100f },
            { "diamond", 200f },
            { "olivine_peridot", 50f },
        };

        // Ungraded ore (flint, quartz, coal, etc.) base XP
        public Dictionary<string, float> UngradedOreXp = new()
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
            { "phosphorite", 5f },
        };

        public Dictionary<string, float> meteoriteXP = new()
        {
            { "iron", 100f }
        };

        // Multipliers keyed by ore grade / gem potential
        public Dictionary<string, float> OreGradeMultiplier = new()
        {
            { "poor", 1f },
            { "medium", 1.5f },
            { "rich", 2f },
            { "bountiful", 3f },
        };

        public Dictionary<string, float> GemPotentialMultiplier = new()
        {
            { "low", 1f },
            { "medium", 2f },
            { "high", 3f },
        };

        // Ore material name -> nugget material name overrides
        public Dictionary<string, string> OreMaterialToNuggetMaterial = new()
        {
            { "quartz_nativegold", "nativegold" },
            { "galena_nativesilver", "nativesilver" },
            { "quartz_nativesilver", "nativesilver" },
        };

        // Fallback XP when a lookup fails
        public float DefaultOreXp = 5f;
        public float DefaultUngradedOreXp = 1f;

        // Crop XP formula: xp = a*totalMonths^2 + b*totalMonths + c
        public CropXpFormula CropXpFormula = new();

        // Mushroom XP
        public float MushroomXp = 15f;

        // Tree/log XP by species
        public Dictionary<string, float> LogXp = new()
        {
            { "birch", 7.5f },
            { "oak", 7.5f },
            { "maple", 7.5f },
            { "pine", 7.5f },
            { "acacia", 7.5f },
            { "kapok", 7.5f },
            { "baldcypress", 7.5f },
            { "larch", 7.5f },
            { "redwood", 7.5f },
            { "ebony", 15f },
            { "walnut", 7.5f },
            { "purpleheart", 15f },
        };
        public float DefaultLogXp = 7.5f;

        // Combat XP: awarded XP = entity max health * this
        public float XpPerHealthPoint = 3.5f;
        public float DefaultCombatXp = 100f; // used if the entity has no health behavior

        // Flat XP for harvesting a dead animal/creature
        public float AnimalHarvestXp = 40f;

        // Social XP
        public float SocialXpPerPlayer = 5f; // per "active tick credit" awarded
        public SocialSystemConfig SocialSystem = new();

        public Dictionary<string, float> SmithingMetalXp { get; set; } = new()
        {
            { "bismuth",       10f },
            { "bismuthbronze", 15f },
            { "blackbronze",   25f },
            { "brass",         15f },
            { "chromium",      25f },
            { "copper",        10f },
            { "cupronickel",   25f },
            { "electrum",      25f },
            { "gold",          35f },
            { "iron",          20f },
            { "meteoriciron",  25f },
            { "lead",          10f },
            { "molybdochalkos",15f },
            { "platinum",      25f },
            { "nickel",        25f },
            { "silver",        20f },
            { "stainlesssteel",25f },
            { "steel",         25f },
            { "tin",           10f },
            { "tinbronze",     15f },
            { "titanium",      25f },
            { "uranium",       20f },
            { "zinc",          10f },
        };
        public Dictionary<string, string> SmithingWorkItemMetalOverrides { get; set; } = new()
        {
            { "ironbloom", "iron" },
        };
        public float DefaultSmithingXpPerIngot { get; set; } = 15f;
    }

    public class CropXpFormula
    {
        public float A = 0.05f;
        public float B = 0.5f;
        public float C = 10.0f;
    }

    public class SocialSystemConfig
    {
        public int TickIntervalMs = 30000;         // how often the social-xp tick runs
        public float CheckRadius = 12.0f;          // blocks; nearby-player radius
        public double EpsilonMovement = 0.01;       // blocks; below this counts as "not moved"
        public double EpsilonRotation = 0.02;       // degrees; below this counts as "not looked around"
        public int IdleTicksThreshold = 4;          // consecutive idle ticks before a player is inactive
        public int MinPlayersForSocialXp = 2;       // includes the player themself
        public int XpAwardedAfterTicks = 10;        // active ticks required before granting XP
    }
}
using DestariaMasteries.src.Alchemy;
using DestariaMasteries.src.Behavior.CollectibleBehaviors;
using DestariaMasteries.src.Behavior.EntityBehaviors;
using DestariaMasteries.src.Effects;
using DestariaMasteries.src.Homesteader;
using DestariaMasteries.src.Mechanist;
using DestariaMasteries.src.Mechanist.Blocks.Jonasscrew;
using DestariaMasteries.src.Utils;
using DestariaMasteries.src.Healing;
using HarmonyLib;
using MasteryLibrary;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Systems
{
    public class DestariaMasteriesLoader : ModSystem
    {
        public static ICoreClientAPI? ClientAPI;
        MasteryLibraryAPI? MasteryLibrary;
        private Harmony? Harmony;
        private ICoreServerAPI? sapi;
        private SocialXpSystem? socialXpSystem;
        public override void Start(ICoreAPI api)
        {
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                Harmony = new Harmony(Mod.Info.ModID);
                Harmony.PatchAllUncategorized();
            }
            //Masteries
            MasteryLibrary = api.ModLoader.GetModSystem<MasteryLibraryAPI>();
            MasteryLibrary.MasteryDefinitions.AddMastery(new AlchemyMastery());
            MasteryLibrary.MasteryDefinitions.AddMastery(new HealingMastery());

            MasteryLibrary.MasteryDefinitions.AddMastery(new MechanistMastery());
            MasteryLibrary.MasteryDefinitions.AddMastery(new HomesteaderMastery());
            //Alchemy Skills
            MasteryLibrary.AbilityRegistry.RegisterAbility(new PrimedToxin());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new DazzleBlast());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new CostlyMistake());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new CatalyticSurge());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new PoisonCloud());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new SoulRecovery());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new ShockingFingers());

            //Mechanist Skills
            MasteryLibrary.AbilityRegistry.RegisterAbility(new ScrapMechanic());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new TemporalAdjustment());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new StormChaser());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new TemporalDevastation());
            //Homesteader Skills
            MasteryLibrary.AbilityRegistry.RegisterAbility(new GroveTending());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new GentleTouch());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new ToTheBone());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new BrewMaster());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new GreenerThanGreen());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new MiracleOfLife());
            //Effects
            MasteryLibrary.EffectRegistry.RegisterEffect(new PoisonDOT());
            MasteryLibrary.EffectRegistry.RegisterEffect(new Blindness());
            MasteryLibrary.EffectRegistry.RegisterEffect(new Deafness());
            MasteryLibrary.EffectRegistry.RegisterEffect(new StabilityDrain());

            api.RegisterBlockClass("BlockJonasScrew", typeof(BlockJonasScrew));

            api.RegisterCollectibleBehaviorClass("SmallItemDamageReduction", typeof(SmallItemDamageReduction));
            api.RegisterCollectibleBehaviorClass("DamageScalar", typeof(DamageScalarBehavior));

            api.RegisterBlockEntityBehaviorClass("MPJonasScrew", typeof(BEBehaviorMPJonasScrew));

            api.RegisterEntityBehaviorClass("DamageResistances", typeof(DamageResistances));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            api.ChatCommands.Create("skilldebug")
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("gettraits")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith((TextCommandCallingArgs args) =>
                {
                    EntityPlayer? player = args.Caller.Entity as EntityPlayer;
                    foreach (var kvp in player.Stats)
                    {
                        string stat = $"{kvp.Key}: ";
                        foreach (var item in kvp.Value.ValuesByKey)
                        {
                            stat += $"{item.Key} = {item.Value.Value}, ";
                        }
                        api.SendMessage(args.Caller.Player, GlobalConstants.AllChatGroups, stat, EnumChatType.Notification);
                    }
                    return TextCommandResult.Success();
                })
                .EndSubCommand();

            XpRewardEvaluator.Initialize(api);

            api.Event.BreakBlock += XpRewardEvaluator.OnBlockBroken;
            api.Event.OnEntityDeath += XpRewardEvaluator.OnEntityDeath;

            socialXpSystem = new SocialXpSystem(api);

            MasteryLibrary?.Events.OnPlayerLeveledUp += (IServerPlayer player, int newLevel, int previousLevel) =>
            {
                sapi.SendMessage(player, GlobalConstants.AllChatGroups, $"You have leveled up and are now level {newLevel}!", EnumChatType.Notification);
            };
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            ClientAPI = api;
        }

        public override void Dispose()
        {
            Harmony?.UnpatchAll($"{Mod.Info.ModID}");
            socialXpSystem?.Dispose();
        }
    }
}

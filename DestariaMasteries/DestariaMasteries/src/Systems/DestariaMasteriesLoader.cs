using DestariaMasteries.src.Alchemy;
using DestariaMasteries.src.Effects;
using DestariaMasteries.src.Healing;
using HarmonyLib;
using MasteryLibrary;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Client;

namespace DestariaMasteries.src.Systems
{
    public class DestariaMasteriesLoader : ModSystem
    {
        public static ICoreClientAPI? ClientAPI;
        MasteryLibraryAPI MasteryLibrary;
        private Harmony Harmony;
        public override void Start(ICoreAPI api)
        {
            Harmony = new Harmony("DestariaSkills");
            Harmony.PatchAll();

            MasteryLibrary = api.ModLoader.GetModSystem<MasteryLibraryAPI>();
            MasteryLibrary.MasteryDefinitions.AddMastery(new AlchemyMastery());
            MasteryLibrary.MasteryDefinitions.AddMastery(new HealingMastery());

            MasteryLibrary.AbilityRegistry.RegisterAbility(new PrimedToxin());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new DazzleBlast());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new CostlyMistake());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new CatalyticSurge());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new PoisonCloud());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new SoulRecovery());
            MasteryLibrary.AbilityRegistry.RegisterAbility(new ShockingFingers());

            MasteryLibrary.EffectRegistry.RegisterEffect(new PoisonDOT());
            MasteryLibrary.EffectRegistry.RegisterEffect(new Blindness());
            MasteryLibrary.EffectRegistry.RegisterEffect(new Deafness());
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("destariamasteries:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            ClientAPI = api;
        }

    }
}

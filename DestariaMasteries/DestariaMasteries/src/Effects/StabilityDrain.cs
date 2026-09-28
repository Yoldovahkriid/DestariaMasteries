using Cairo;
using MasteryLibrary.src.Core.Effects;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Effects
{
    internal class StabilityDrain : Effect
    {
        public override string Code => "StabilityDrain";

        public override string IconPath => string.Empty;

        public override bool CanStack => true;
        public override StackingBehavior StackingBehavior => StackingBehavior.ExtendDuration;

        private const string LAST_TICK_KEY = "LastTickTime";
        private const string TICK_INTERVAL_KEY = "TickInterval";
        private const string DRAIN_PER_TICK_KEY = "DrainPerTick";

        public override void OnApply(EffectInstance effectInstance, EntityAgent entityAgent)
        {
            effectInstance.CustomData.Add(LAST_TICK_KEY, 0L);
            effectInstance.CustomData.Add(TICK_INTERVAL_KEY, 250);
        }

        public override void OnTick(EffectInstance effectInstance, EntityAgent entityAgent, float deltaTime)
        {
            if (entityAgent is not EntityPlayer player) return;
            long lastTick = effectInstance.GetValueSafe<long>(LAST_TICK_KEY);
            int tickInterval = effectInstance.GetValueSafe<int>(TICK_INTERVAL_KEY);
            long currentTime = player.World.ElapsedMilliseconds;

            if (currentTime - lastTick >= tickInterval)
            {
                EntityBehaviorTemporalStabilityAffected? tsa = player.GetBehavior<EntityBehaviorTemporalStabilityAffected>();
                if (tsa == null) return;
                tsa.OwnStability -= effectInstance.GetValueSafe<float>(DRAIN_PER_TICK_KEY);
            }
        }
    }
}

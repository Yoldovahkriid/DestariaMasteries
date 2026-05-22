using MasteryLibrary.src.Core.Effects;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Effects
{
    public class Blindness : Effect
    {
        public override string Code => "Blindness";

        public override string IconPath => String.Empty;

        public override void OnApply(EffectInstance effectInstance, EntityAgent entityAgent)
        {
            if(entityAgent is EntityPlayer player)
            {
                player.WatchedAttributes.SetBool("IsBlinded", true);
                player.WatchedAttributes.MarkPathDirty("IsBlinded");
            }
        }

        public override void OnTick(EffectInstance effectInstance, EntityAgent entityAgent, float deltaTime)
        {
            if (!(entityAgent is EntityPlayer))
            {
                entityAgent.GetBehavior<EntityBehaviorTaskAI>()?.TaskManager.ExecuteTask<AiTaskIdle>();
            }
        }

        public override void OnRemove(EffectInstance effectInstance, EntityAgent entityAgent, RemovalReason reason)
        {
            if (entityAgent is EntityPlayer player)
            {
                player.WatchedAttributes.SetBool("IsBlinded", false);
                player.WatchedAttributes.MarkPathDirty("IsBlinded");
            }
        }
    }
}

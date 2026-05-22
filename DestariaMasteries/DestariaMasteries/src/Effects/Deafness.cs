using MasteryLibrary.src.Core.Effects;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace DestariaMasteries.src.Effects
{
    public class Deafness : Effect
    {
        public override string Code => "Deafness";
        public override string IconPath => String.Empty;
        public override EffectType Type => EffectType.Debuff;

        public override void OnApply(EffectInstance effectInstance, EntityAgent entityAgent)
        {
            if (entityAgent is EntityPlayer player)
            {
                player.WatchedAttributes.SetBool("IsDeafened", true);
                player.WatchedAttributes.MarkPathDirty("IsDeafened");
            }
        }

        public override void OnRemove(EffectInstance effectInstance, EntityAgent entityAgent, RemovalReason reason)
        {
            if (entityAgent is EntityPlayer player)
            {
                player.WatchedAttributes.SetBool("IsDeafened", false);
                player.WatchedAttributes.MarkPathDirty("IsDeafened");
            }
        }
    }
}

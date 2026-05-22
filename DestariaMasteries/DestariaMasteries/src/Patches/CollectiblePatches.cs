using HarmonyLib;
using MasteryLibrary;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Core.Masteries.Instances;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(CollectibleObject), "tryEatStop")]
    public class CollectiblePatchesTryEatStop
    {
        static bool Prefix(CollectibleObject __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity)
        {
            FoodNutritionProperties nutriProps = __instance.GetNutritionProperties(byEntity.World, slot.Itemstack, byEntity);
            bool IsMushroom = false;
            if (__instance.Code.Path.Contains("mushroom"))
            {
                IsMushroom |= true;
            }
            if (byEntity.World is IServerWorldAccessor && nutriProps != null && secondsUsed >= 0.95f)
            {
                TransitionState state = __instance.UpdateAndGetTransitionState(byEntity.World, slot, EnumTransitionType.Perish);
                float spoilState = state != null ? state.TransitionLevel : 0;

                float satLossMul = GlobalConstants.FoodSpoilageSatLossMul(spoilState, slot.Itemstack, byEntity);
                float healthLossMul = GlobalConstants.FoodSpoilageHealthLossMul(spoilState, slot.Itemstack, byEntity);

                float bonusNutrition = 0;
                if (IsMushroom && byEntity.HasBehavior<EntityBehaviorEffects>() && byEntity.HasBehavior<EntityBehaviorPlayerMasteries>())
                {
                    bonusNutrition = byEntity.Stats.GetBlended("mushroomsSaturation"); //Subtracting one because of BaseValue
                    EntityBehaviorPlayerMasteries? masteries = byEntity.GetBehavior<EntityBehaviorPlayerMasteries>();
                    SkillInstance? skill = masteries?.PlayerMasteryData.GetSkillInstance("FungalNourishment");

                    EntityBehaviorEffects? agentEffects = byEntity.GetBehavior<EntityBehaviorEffects>();
                    EffectManager? effectManager = agentEffects?.EffectManager;
                    if (skill != null)
                    {
                        MasteryLibraryAPI mapi = byEntity.Api.ModLoader.GetModSystem<MasteryLibraryAPI>();
                        EffectInstance? effect = mapi.EffectRegistry.CreateEffectInstance("StatBoost", byEntity.GetName(), (long)skill.Skill.Duration * 1000, skill.Level);
                        var activeStats = skill.Skill.GetValueSafe<Dictionary<string, StatConfiguration>>("ActiveStats");
                        if (activeStats != null)
                        {
                            effect?.CustomData.Add("ActiveStats", activeStats);
                        }
                        effectManager?.AddEffect(effect);
                    }
                }
                byEntity.ReceiveSaturation((nutriProps.Satiety + bonusNutrition) * satLossMul, nutriProps.FoodCategory, nutriProps.SaturationLossDelay);

                foreach (var category in byEntity.Stats)
                {
                    byEntity.Api.Logger.Debug(category.Key);
                }

                IPlayer player = null;
                if (byEntity is EntityPlayer) player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

                slot.TakeOut(1);

                if (nutriProps.EatenStack != null)
                {
                    if (slot.Empty)
                    {
                        slot.Itemstack = nutriProps.EatenStack.ResolvedItemstack?.Clone();
                    }
                    else
                    {
                        if (player == null || !player.InventoryManager.TryGiveItemstack(nutriProps.EatenStack.ResolvedItemstack?.Clone(), true))
                        {
                            byEntity.World.SpawnItemEntity(nutriProps.EatenStack.ResolvedItemstack?.Clone(), byEntity.Pos.XYZ);
                        }
                    }
                }

                float healthChange = nutriProps.Health * healthLossMul;
                if (healthChange < 0 && IsMushroom)
                {
                    healthChange *= Math.Max(player.Entity.Stats.GetBlended("mushroomDamageMult"), 0);
                }

                float intox = byEntity.WatchedAttributes.GetFloat("intoxication");
                byEntity.WatchedAttributes.SetFloat("intoxication", Math.Min(1.1f, intox + nutriProps.Intoxication));

                float psyche = byEntity.WatchedAttributes.GetFloat("psychedelic");
                byEntity.WatchedAttributes.SetFloat("psychedelic", Math.Min(2.0f, psyche + nutriProps.Psychedelic));

                if (healthChange != 0)
                {
                    float durationSec = slot.Itemstack?.Collectible?.Attributes?["eatHealthEffectDurationSec"].AsFloat(0) ?? 0;
                    int ticks = slot.Itemstack?.Collectible?.Attributes?["eatHealthEffectTicks"].AsInt(1) ?? 1;

                    byEntity.ReceiveDamage(new DamageSource()
                    {
                        Source = EnumDamageSource.Internal,
                        Type = healthChange > 0 ? EnumDamageType.Heal : EnumDamageType.Poison,
                        Duration = TimeSpan.FromSeconds(durationSec),
                        TicksPerDuration = ticks,
                        DamageOverTimeTypeEnum = healthChange > 0 ? EnumDamageOverTimeEffectType.Unknown : EnumDamageOverTimeEffectType.Poison
                    }, Math.Abs(healthChange));
                }

                slot.MarkDirty();
                player?.InventoryManager.BroadcastHotbarSlot();
            }
            return false;
        }
    }
}

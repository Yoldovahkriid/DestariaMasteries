using DestariaMasteries.src.Systems;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch]
    public class LoadedSoundNativePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoadedSoundNative), "GlobalVolume", MethodType.Getter)]
        public static void Patch_LoadedSoundNative_GlobalVolume_Getter_Postfix(ILoadedSound __instance, ref float __result)
        {
            ICoreClientAPI? capi = DestariaMasteriesLoader.ClientAPI;
            if (capi == null) return;

            EntityPlayer? player = capi.World.Player?.Entity;
            if (player == null) return;

            if (player.WatchedAttributes.GetBool("IsDeafened"))
            {
                __result = 0f;
            }
        }
    }
}

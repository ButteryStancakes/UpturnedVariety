﻿using System.IO;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Linq;

namespace UpturnedVariety
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.upturnedvariety", PLUGIN_NAME = "Upturned Variety", PLUGIN_VERSION = "1.1.0";
        internal static new ManualLogSource Logger;
        internal static AudioClip boombox;
        internal static Texture giftBoxTex2;
        internal static Material lollipop2;

        void Awake()
        {
            Logger = base.Logger;

            try
            {
                AssetBundle upturnedBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "upturnedvariety"));
                boombox = upturnedBundle.LoadAsset<AudioClip>("Boombox");
                giftBoxTex2 = upturnedBundle.LoadAsset<Texture>("GiftBoxTex2");
                lollipop2 = upturnedBundle.LoadAsset<Material>("LollyPop");
                upturnedBundle.Unload(false);
            }
            catch
            {
                Logger.LogError("Encountered some error loading asset bundle. Did you install the plugin correctly?");
                return;
            }

            new Harmony(PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");
        }
    }

    [HarmonyPatch]
    class UpturnedVarietyPatches
    {
        static StartMatchLever startMatchLever;

        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        static void StartOfRoundPostAwake(StartOfRound __instance)
        {
            if (Plugin.boombox != null)
            {
                Item boombox = __instance.allItemsList.itemsList.FirstOrDefault(item => item.name == "Boombox");
                if (boombox != null)
                {
                    BoomboxItem boomboxItem = boombox.spawnPrefab.GetComponent<BoomboxItem>();
                    if (System.Array.IndexOf(boomboxItem.musicAudios, Plugin.boombox) < 0)
                    {
                        boomboxItem.musicAudios = new List<AudioClip>(boomboxItem.musicAudios)
                        {
                            Plugin.boombox
                        }.ToArray();
                        Plugin.Logger.LogDebug($"Loaded Upturned track into Boombox");
                    }
                }
            }

            if (startMatchLever == null)
                startMatchLever = Object.FindObjectOfType<StartMatchLever>();
        }

        [HarmonyPatch(typeof(GiftBoxItem), nameof(GiftBoxItem.Start))]
        [HarmonyPostfix]
        static void GiftBoxItemPostStart(GiftBoxItem __instance)
        {
            if (startMatchLever != null && Plugin.giftBoxTex2 != null && startMatchLever.leverHasBeenPulled && new System.Random((int)__instance.targetFloorPosition.x + (int)__instance.targetFloorPosition.y).NextDouble() >= 0.5)
            {
                __instance.GetComponent<Renderer>().material.mainTexture = Plugin.giftBoxTex2;
                Plugin.Logger.LogDebug($"Gift #{__instance.GetInstanceID()} using alternate texture");
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
        [HarmonyPostfix]
        static void GrabbableObjectPostStart(GrabbableObject __instance)
        {
            if (__instance.itemProperties.name == "Candy" && __instance.mainObjectRenderer.sharedMaterials != null && __instance.mainObjectRenderer.sharedMaterials?.Length == 2 && startMatchLever != null && Plugin.lollipop2 != null && startMatchLever.leverHasBeenPulled && new System.Random((int)__instance.targetFloorPosition.x + (int)__instance.targetFloorPosition.y).NextDouble() >= 0.5)
            {
                __instance.GetComponent<Renderer>().materials =
                [
                    Plugin.lollipop2,
                        __instance.mainObjectRenderer.sharedMaterials[1]
                ];
                Plugin.Logger.LogDebug($"Candy #{__instance.GetInstanceID()} using alternate texture");
            }
        }
    }
}
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(GUID_LOBBY_COMPATIBILITY, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string PLUGIN_GUID = "butterystancakes.lethalcompany.upturnedvariety", PLUGIN_NAME = "Upturned Variety", PLUGIN_VERSION = "2.1.0";
        internal static new ManualLogSource Logger;

        const string GUID_LOBBY_COMPATIBILITY = "BMX.LobbyCompatibility";

        internal static ConfigEntry<bool> configBoombox;

        internal static AudioClip boombox;

        void Awake()
        {
            Logger = base.Logger;

            if (Chainloader.PluginInfos.ContainsKey(GUID_LOBBY_COMPATIBILITY))
            {
                Logger.LogInfo("CROSS-COMPATIBILITY - Lobby Compatibility detected");
                LobbyCompatibility.Init();
            }

            configBoombox = Config.Bind(
                "Items",
                "Boombox",
                true,
                "Enables alternate models for the \"Boombox\" item.");

            SkinManager.LoadAllConfigs(Config);

            try
            {
                AssetBundle upturnedBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "upturnedvariety"));
                boombox = upturnedBundle.LoadAsset<AudioClip>("Boombox");
                SkinManager.LoadAllSkins(upturnedBundle);
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
        static readonly Color SHINY_BOOMBOX = new(0.5799929f, 0.5799929f, 0.5799929f);

        static StartMatchLever _startMatchLever;
        static StartMatchLever StartMatchLever
        {
            get
            {
                if (_startMatchLever == null)
                    _startMatchLever = Object.FindAnyObjectByType<StartMatchLever>();

                return _startMatchLever;
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        [HarmonyPostfix]
        static void StartOfRound_Post_Awake(StartOfRound __instance)
        {
            if (Plugin.boombox != null)
            {
                Item boombox = __instance.allItemsList.itemsList.FirstOrDefault(item => item.name == "Boombox" && item.itemId == 9);
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

            SkinManager.Reset();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SyncScrapValuesClientRpc))]
        [HarmonyPostfix]
        static void RoundManager_Post_SyncScrapValuesClientRpc(NetworkObjectReference[] spawnedScrap)
        {
            SkinManager.ApplySkinsToItems(spawnedScrap);
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
        [HarmonyPostfix]
        static void GrabbableObject_Post_Start(GrabbableObject __instance)
        {
            if (__instance.itemProperties.name == "FishTestProp" && __instance.mainObjectRenderer != null)
                SkinManager.fish.Add(__instance.mainObjectRenderer);
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetControlTipsForItem))]
        [HarmonyPostfix]
        static void GrabbableObject_Post_SetControlTipsForItem(GrabbableObject __instance)
        {
            if (__instance.TryGetComponent(out SubstituteItemName subItemName))
                HUDManager.Instance.controlTipLines[0].SetText(HUDManager.Instance.controlTipLines[0].text.Replace(__instance.itemProperties.itemName, subItemName.subName));
        }

        [HarmonyPatch(typeof(BoomboxItem), nameof(BoomboxItem.Start))]
        [HarmonyPostfix]
        static void BoomboxItem_Post_Start(BoomboxItem __instance)
        {
            if (!Plugin.configBoombox.Value || __instance.mainObjectRenderer == null || StartMatchLever == null || !StartMatchLever.leverHasBeenPulled || !__instance.TryGetComponent(out NetworkObject netObj))
                return;

            // shiny gray boombox
            if (new System.Random(StartOfRound.Instance.randomMapSeed * (int)netObj.NetworkObjectId).NextDouble() >= 0.5)
            {
                Material[] mats = __instance.mainObjectRenderer.materials;
                mats[3].SetColor("_Color", SHINY_BOOMBOX);
                mats[3].SetColor("_BaseColor", SHINY_BOOMBOX);
                mats[3].SetFloat("_Metallic", 0.6833333f);
                mats[3].SetFloat("_Smoothness", 0.275f);
                __instance.mainObjectRenderer.materials = mats;
            }
        }
    }
}
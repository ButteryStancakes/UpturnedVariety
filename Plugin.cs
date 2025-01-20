using System.IO;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

namespace UpturnedVariety
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.upturnedvariety", PLUGIN_NAME = "Upturned Variety", PLUGIN_VERSION = "1.2.0";
        internal static new ManualLogSource Logger;
        internal static AudioClip boombox;
        internal static Texture giftBoxTex2;
        internal static Material lollipop2;
        internal static Color candyYellow = new(0.9063317f, 0.8953158f, 0.4669504f);

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
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SyncScrapValuesClientRpc))]
        [HarmonyPostfix]
        static void PostSyncScrapValuesClientRpc(RoundManager __instance, NetworkObjectReference[] spawnedScrap)
        {
            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (spawnedScrap[i].TryGet(out NetworkObject networkObject) && networkObject.TryGetComponent(out GrabbableObject grabbableObject))
                {
                    if (grabbableObject is GiftBoxItem)
                    {
                        if (Plugin.giftBoxTex2 != null && ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition) == 1)
                        {
                            grabbableObject.GetComponent<Renderer>().material.mainTexture = Plugin.giftBoxTex2;
                            Plugin.Logger.LogDebug($"Gift #{networkObject.NetworkObjectId} using alternate texture");
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "Candy" && grabbableObject.mainObjectRenderer.sharedMaterials != null && grabbableObject.mainObjectRenderer.sharedMaterials.Length == 2 && grabbableObject.mainObjectRenderer.sharedMaterials[0].name.StartsWith("LollyPop"))
                    {
                        switch (ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition, Plugin.lollipop2 != null ? 3 : 2))
                        {
                            case 1:
                                Material[] mats = grabbableObject.mainObjectRenderer.materials;
                                mats[0].SetColor("_Color", Plugin.candyYellow);
                                mats[0].SetColor("_BaseColor", Plugin.candyYellow);
                                grabbableObject.mainObjectRenderer.materials = mats;
                                Plugin.Logger.LogDebug($"Candy #{networkObject.NetworkObjectId} using alternate texture");
                                break;
                            case 2:
                                grabbableObject.mainObjectRenderer.materials =
                                [
                                    Plugin.lollipop2,
                                    grabbableObject.mainObjectRenderer.sharedMaterials[1]
                                ];
                                Plugin.Logger.LogDebug($"Candy #{networkObject.NetworkObjectId} using alternate texture 2");
                                break;
                        }
                    }
                }
            }
        }
    }

    internal class ItemVariety
    {
        internal static int GetSkinIndex(Vector3 pos, int count = 2)
        {
            return new System.Random(StartOfRound.Instance.randomMapSeed + (int)pos.x + (int)pos.z).Next(count);
        }
    }
}
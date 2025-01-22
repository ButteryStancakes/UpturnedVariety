using System.IO;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using BepInEx.Configuration;

namespace UpturnedVariety
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "butterystancakes.lethalcompany.upturnedvariety", PLUGIN_NAME = "Upturned Variety", PLUGIN_VERSION = "1.3.0";
        internal static new ManualLogSource Logger;

        internal static ConfigEntry<bool> configGift, configCandy, configPerfume, configPerfumeMeshes, configPills, configMug, configControlPad, configFish;

        void Awake()
        {
            Logger = base.Logger;

            configGift = Config.Bind(
                "Items",
                "Gift",
                true,
                "Enables alternate palettes for the \"Gift box\" item."
            );

            configCandy = Config.Bind(
                "Items",
                "Candy",
                true,
                "Enables alternate palettes for the \"Candy\" item."
            );

            configPerfume = Config.Bind(
                "Items",
                "Perfume",
                true,
                "Enables alternate palettes for the \"Perfume bottle\" item."
            );

            configPerfumeMeshes = Config.Bind(
                "Items",
                "PerfumeModels",
                true,
                "Enables alternate models for the \"Perfume bottle\" item."
            );

            configPills = Config.Bind(
                "Items",
                "Pills",
                true,
                "Enables alternate palettes for the \"Pill bottle\" item."
            );

            configMug = Config.Bind(
                "Items",
                "Mug",
                true,
                "Enables alternate palettes for the \"Mug\" item."
            );

            configControlPad = Config.Bind(
                "Items",
                "ControlPad",
                true,
                "Enables alternate palettes for the \"Control pad\" item."
            );

            configFish = Config.Bind(
                "Items",
                "Fish",
                true,
                "Enables alternate models for the \"Plastic fish\" item."
            );

            try
            {
                AssetBundle upturnedBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "upturnedvariety"));
                ItemVariety.boombox = upturnedBundle.LoadAsset<AudioClip>("Boombox");
                if (configGift.Value)
                    ItemVariety.giftBoxTex2 = upturnedBundle.LoadAsset<Texture>("GiftBoxTex2");
                if (configCandy.Value)
                {
                    ItemVariety.lollipop2 = upturnedBundle.LoadAsset<Material>("LollyPop");
                    ItemVariety.lollyStick = upturnedBundle.LoadAsset<Material>("LightWood");
                    ItemVariety.lollyMesh = upturnedBundle.LoadAsset<Mesh>("Cylinder.001");
                }
                if (configPerfumeMeshes.Value)
                {
                    ItemVariety.perfumeMeshes = [
                        upturnedBundle.LoadAsset<Mesh>("PerfumeBottle_001"),
                        upturnedBundle.LoadAsset<Mesh>("PerfumeBottle_002"),
                        upturnedBundle.LoadAsset<Mesh>("PerfumeBottle_003"),
                        upturnedBundle.LoadAsset<Mesh>("PerfumeBottle_004"),
                        upturnedBundle.LoadAsset<Mesh>("PerfumeBottle_005"),
                    ];
                }
                if (configPills.Value)
                    ItemVariety.pillBottle2 = upturnedBundle.LoadAsset<Texture>("PillBottleTextureB2");
                if (configMug.Value)
                    ItemVariety.coffeeMug6 = upturnedBundle.LoadAsset<Material>("CoffeeMug6");
                if (configControlPad.Value)
                    ItemVariety.controlPad2 = upturnedBundle.LoadAsset<Texture>("ArcadeControlPanel2");
                if (configFish.Value)
                {
                    ItemVariety.fish2 = upturnedBundle.LoadAsset<Mesh>("Fish2");
                    ItemVariety.sardine = upturnedBundle.LoadAsset<Mesh>("Sardine");
                    ItemVariety.sardineBanana = upturnedBundle.LoadAsset<Mesh>("Sardine.001");
                }
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
            if (ItemVariety.boombox != null)
            {
                Item boombox = __instance.allItemsList.itemsList.FirstOrDefault(item => item.name == "Boombox");
                if (boombox != null)
                {
                    BoomboxItem boomboxItem = boombox.spawnPrefab.GetComponent<BoomboxItem>();
                    if (System.Array.IndexOf(boomboxItem.musicAudios, ItemVariety.boombox) < 0)
                    {
                        boomboxItem.musicAudios = new List<AudioClip>(boomboxItem.musicAudios)
                        {
                            ItemVariety.boombox
                        }.ToArray();
                        Plugin.Logger.LogDebug($"Loaded Upturned track into Boombox");
                    }
                }
            }

            ItemVariety.cache.Clear();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SyncScrapValuesClientRpc))]
        [HarmonyPostfix]
        static void PostSyncScrapValuesClientRpc(RoundManager __instance, NetworkObjectReference[] spawnedScrap)
        {
            ItemVariety.offset = -1;
            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (spawnedScrap[i].TryGet(out NetworkObject networkObject) && ItemVariety.cache.Add(networkObject.NetworkObjectId) && networkObject.TryGetComponent(out GrabbableObject grabbableObject))
                {
                    if (Plugin.configGift.Value && grabbableObject is GiftBoxItem)
                    {
                        if (ItemVariety.giftBoxTex2 != null && ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition) == 1)
                        {
                            grabbableObject.GetComponent<Renderer>().material.mainTexture = ItemVariety.giftBoxTex2;
                            Plugin.Logger.LogDebug($"Gift #{networkObject.NetworkObjectId} using alternate texture");
                        }
                    }
                    else if (Plugin.configCandy.Value && grabbableObject.itemProperties.name == "Candy" && ItemVariety.lollipop2 != null && ItemVariety.lollyStick != null && grabbableObject.mainObjectRenderer?.sharedMaterials != null && grabbableObject.mainObjectRenderer.sharedMaterials.Length == 2 && grabbableObject.mainObjectRenderer.sharedMaterials[0].name.StartsWith("LollyPop") && ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition) == 1)
                    {
                        grabbableObject.mainObjectRenderer.materials =
                        [
                            ItemVariety.lollipop2,
                            ItemVariety.lollyStick
                        ];
                        if (ItemVariety.lollyMesh != null)
                            grabbableObject.mainObjectRenderer.GetComponent<MeshFilter>().mesh = ItemVariety.lollyMesh;
                        Plugin.Logger.LogDebug($"Candy #{networkObject.NetworkObjectId} using alternate texture");
                    }
                    else if (grabbableObject.itemProperties.name == "PerfumeBottle" && grabbableObject.mainObjectRenderer?.sharedMaterials != null && grabbableObject.mainObjectRenderer.sharedMaterials.Length == 2 && grabbableObject.mainObjectRenderer.sharedMaterials[0].name.StartsWith("Material.004"))
                    {
                        if (Plugin.configPerfume.Value)
                        {
                            int mat = ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition, 4); // 5
                            if (mat != 0)
                            {
                                Color color = default, transColor = default;
                                switch (mat)
                                {
                                    // red
                                    case 1:
                                        color = ItemVariety.perfumeRed;
                                        transColor = ItemVariety.perfumeRedTrans;
                                        break;
                                    // blue
                                    case 2:
                                        color = ItemVariety.perfumeBlue;
                                        transColor = ItemVariety.perfumeBlueTrans;
                                        break;
                                    // black
                                    case 3:
                                        color = ItemVariety.perfumeBlack;
                                        transColor = ItemVariety.perfumeBlackTrans;
                                        break;
                                        // pink
                                        /*case 4:
                                            color = ItemVariety.perfumePink;
                                            transColor = ItemVariety.perfumePinkTrans;
                                            break;*/
                                }
                                Material perfumeBottle = grabbableObject.mainObjectRenderer.materials[0];
                                perfumeBottle.SetColor("_Color", color);
                                perfumeBottle.SetColor("_BaseColor", color);
                                perfumeBottle.SetColor("_TransmittanceColor", transColor);
                                grabbableObject.mainObjectRenderer.materials =
                                [
                                    perfumeBottle,
                                    grabbableObject.mainObjectRenderer.sharedMaterials[1]
                                ];
                                Plugin.Logger.LogDebug($"Perfume #{networkObject.NetworkObjectId} using alternate texture");
                            }
                        }

                        if (Plugin.configPerfumeMeshes.Value && ItemVariety.perfumeMeshes != null && ItemVariety.perfumeMeshes.Length > 0)
                        {
                            int model = ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition, ItemVariety.perfumeMeshes.Length + 1);
                            if (model != 0)
                            {
                                grabbableObject.mainObjectRenderer.GetComponent<MeshFilter>().mesh = ItemVariety.perfumeMeshes[model - 1];
                                Plugin.Logger.LogDebug($"Perfume #{networkObject.NetworkObjectId} using alternate model");
                            }
                        }
                    }
                    else if (Plugin.configPills.Value && grabbableObject.itemProperties.name == "PillBottle" && ItemVariety.pillBottle2 != null && grabbableObject.mainObjectRenderer?.sharedMaterial != null && grabbableObject.mainObjectRenderer.sharedMaterial.name.StartsWith("Material.002") && ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition) == 1)
                    {
                        grabbableObject.mainObjectRenderer.material.mainTexture = ItemVariety.pillBottle2;
                        Plugin.Logger.LogDebug($"Pills #{networkObject.NetworkObjectId} using alternate texture");
                    }
                    else if (Plugin.configMug.Value && grabbableObject.itemProperties.name == "Mug" && ItemVariety.coffeeMug6 != null && grabbableObject.mainObjectRenderer?.sharedMaterial != null && grabbableObject.mainObjectRenderer.sharedMaterial.name.StartsWith("CoffeeMug") && ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition, 6) == 5)
                    {
                        grabbableObject.mainObjectRenderer.material = ItemVariety.coffeeMug6;
                        Plugin.Logger.LogDebug($"Mug #{networkObject.NetworkObjectId} using alternate texture");
                    }
                    else if (Plugin.configControlPad.Value && grabbableObject.itemProperties.name == "ControlPad" && ItemVariety.controlPad2 != null && grabbableObject.TryGetComponent(out Renderer rend) && rend.sharedMaterial != null && rend.sharedMaterial.name.StartsWith("ArcadeControlPanel") && ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition) == 1)
                    {
                        rend.material.mainTexture = ItemVariety.controlPad2;
                        Plugin.Logger.LogDebug($"Controller #{networkObject.NetworkObjectId} using alternate texture");
                    }
                    else if (Plugin.configFish.Value && grabbableObject.itemProperties.name == "FishTestProp" && grabbableObject.mainObjectRenderer.TryGetComponent(out MeshFilter mesh))
                    {
                        if (ItemVariety.sardineBanana != null && ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition, 9) == 8)
                        {
                            mesh.mesh = ItemVariety.sardineBanana;
                            Material yellowRubber = grabbableObject.mainObjectRenderer.materials[1];
                            Material material001 = Object.Instantiate(yellowRubber);
                            yellowRubber.SetColor("_Color", ItemVariety.banana);
                            yellowRubber.SetColor("_BaseColor", ItemVariety.banana);
                            material001.SetColor("_Color", ItemVariety.stem);
                            material001.SetColor("_BaseColor", ItemVariety.stem);
                            grabbableObject.mainObjectRenderer.materials = [
                                grabbableObject.mainObjectRenderer.sharedMaterials[0],
                                yellowRubber,
                                material001
                            ];
                        }
                        else if (ItemVariety.fish2 != null && ItemVariety.sardine != null)
                        {
                            switch (ItemVariety.GetSkinIndex(grabbableObject.targetFloorPosition, 3))
                            {
                                // green
                                default:
                                    continue;
                                // yellow
                                case 1:
                                    mesh.mesh = ItemVariety.fish2;
                                    Material yellowFish = grabbableObject.mainObjectRenderer.materials[1];
                                    yellowFish.SetColor("_Color", ItemVariety.fishYellow);
                                    yellowFish.SetColor("_BaseColor", ItemVariety.fishYellow);
                                    grabbableObject.mainObjectRenderer.materials = [
                                        yellowFish,
                                        grabbableObject.mainObjectRenderer.sharedMaterials[0]
                                    ];
                                    break;
                                // red
                                case 2:
                                    mesh.mesh = ItemVariety.sardine;
                                    Material silverFish = grabbableObject.mainObjectRenderer.materials[1];
                                    silverFish.SetColor("_Color", ItemVariety.fishRed);
                                    silverFish.SetColor("_BaseColor", ItemVariety.fishRed);
                                    grabbableObject.mainObjectRenderer.materials = [
                                        grabbableObject.mainObjectRenderer.sharedMaterials[0],
                                        silverFish
                                    ];
                                    break;
                            }
                        }
                        Plugin.Logger.LogDebug($"Fish #{networkObject.NetworkObjectId} using alternate model");
                    }
                }
            }
        }
    }

    internal class ItemVariety
    {
        internal static AudioClip boombox;
        internal static Texture giftBoxTex2, pillBottle2, controlPad2;
        internal static Material lollipop2, lollyStick, coffeeMug6;
        internal static Color perfumeRed = new(0.9f, 0.3922666f, 0.324f, 0.6313726f),
                              perfumeRedTrans = new(1f, 0.4f, 0.4f),
                              perfumeBlue = new(0.4509091f, 0.416f, 0.8f, 0.6313726f), // 0.4076735f, 0.368f, 0.8f
                              perfumeBlueTrans = new(0.59f, 0.6241666f, 1f),
                              perfumeBlack = new(0.01f, 0.01f, 0.01f, 0.6313726f),
                              perfumeBlackTrans = new(0.48f, 0.48f, 0.48f),
                              /*perfumePink = new(0.9f, 0.297f, 0.5985f),
                              perfumePinkTrans = new(1f, 0.52f, 0.7276597f)*/
                              fishYellow = new(0.773024f, 0.8392157f, 0.3692549f),
                              fishRed = new(0.5019608f, 0.1780566f, 0.1556078f),
                              banana = new(0.44f, 0.4296874f, 0.2028124f),
                              stem = new(0.3799999f, 0.243271f, 0.1527103f);
        internal static Mesh lollyMesh, fish2, sardine, sardineBanana;
        internal static Mesh[] perfumeMeshes;

        internal static HashSet<ulong> cache = [];
        internal static int offset;

        internal static int GetSkinIndex(Vector3 pos, int count = 2)
        {
            offset++;
            return new System.Random(StartOfRound.Instance.randomMapSeed + (int)pos.x + (int)pos.z + offset).Next(count);
        }
    }
}
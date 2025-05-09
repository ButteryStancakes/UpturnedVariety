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
        internal const string PLUGIN_GUID = "butterystancakes.lethalcompany.upturnedvariety", PLUGIN_NAME = "Upturned Variety", PLUGIN_VERSION = "1.3.3";
        internal static new ManualLogSource Logger;

        const string GUID_LOBBY_COMPATIBILITY = "BMX.LobbyCompatibility";

        internal static ConfigEntry<bool> configGift, configCandy, configPerfume, configPerfumeMeshes, configPills, configMug, configControlPad, configFish, configCandyMeshes, configSteeringWheel, configPickles;

        void Awake()
        {
            Logger = base.Logger;

            if (Chainloader.PluginInfos.ContainsKey(GUID_LOBBY_COMPATIBILITY))
            {
                Logger.LogInfo("CROSS-COMPATIBILITY - Lobby Compatibility detected");
                LobbyCompatibility.Init();
            }

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

            configCandyMeshes = Config.Bind(
                "Items",
                "CandyModels",
                true,
                "Enables alternate models for the \"Candy\" item."
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

            configSteeringWheel = Config.Bind(
                "Items",
                "SteeringWheel",
                true,
                "Enables alternate palettes for the \"Steering wheel\" item."
            );

            configPickles = Config.Bind(
                "Items",
                "Pickles",
                true,
                "Enables alternate models for the \"Jar of pickles\" item."
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
                    if (configCandyMeshes.Value)
                    {
                        ItemVariety.sucker = upturnedBundle.LoadAsset<Mesh>("Sucker");
                        ItemVariety.candyPink = upturnedBundle.LoadAsset<Material>("CandyPink");
                    }
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
                if (configSteeringWheel.Value)
                    ItemVariety.darkPlastic = upturnedBundle.LoadAsset<Material>("DarkPlastic");
                if (configPickles.Value)
                {
                    ItemVariety.glassJar = upturnedBundle.LoadAsset<Mesh>("GlassJar");
                    ItemVariety.jarGlass = upturnedBundle.LoadAsset<Material>("GlassCase");
                    ItemVariety.jarLid = upturnedBundle.LoadAsset<Material>("DisplayCasePlastic");
                    ItemVariety.candyGlob = upturnedBundle.LoadAsset<GameObject>("CandyGlob");
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
            ItemVariety.offsets.Clear();

            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (spawnedScrap[i].TryGet(out NetworkObject networkObject) && ItemVariety.cache.Add(networkObject.NetworkObjectId) && networkObject.TryGetComponent(out GrabbableObject grabbableObject))
                {
                    if (grabbableObject is GiftBoxItem)
                    {
                        ItemVariety.GetSkinIndices(grabbableObject);

                        if (Plugin.configGift.Value && ItemVariety.giftBoxTex2 != null && ItemVariety.tex == 1)
                        {
                            grabbableObject.GetComponent<Renderer>().material.mainTexture = ItemVariety.giftBoxTex2;
                            Plugin.Logger.LogDebug($"Gift #{networkObject.NetworkObjectId} using alternate texture");
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "Candy" && grabbableObject.mainObjectRenderer?.sharedMaterials != null && grabbableObject.mainObjectRenderer.sharedMaterials.Length == 2 && grabbableObject.mainObjectRenderer.sharedMaterials[0].name.StartsWith("LollyPop"))
                    {
                        ItemVariety.GetSkinIndices(grabbableObject, 2, 2);

                        if (Plugin.configCandy.Value && ItemVariety.lollipop2 != null && ItemVariety.lollyStick != null)
                        {
                            if (ItemVariety.tex == 1)
                            {
                                grabbableObject.mainObjectRenderer.materials =
                                [
                                    ItemVariety.lollipop2,
                                    ItemVariety.lollyStick
                                ];
                                Plugin.Logger.LogDebug($"Candy #{networkObject.NetworkObjectId} using alternate texture");
                            }

                            if (Plugin.configCandyMeshes.Value && ItemVariety.mesh == 1 && (ItemVariety.tex != 0 || ItemVariety.candyPink != null))
                            {
                                grabbableObject.mainObjectRenderer.GetComponent<MeshFilter>().mesh = ItemVariety.sucker;
                                Plugin.Logger.LogDebug($"Candy #{networkObject.NetworkObjectId} using alternate model");

                                if (ItemVariety.tex == 0)
                                {
                                    grabbableObject.mainObjectRenderer.materials =
                                    [
                                        ItemVariety.candyPink,
                                        grabbableObject.mainObjectRenderer.sharedMaterials[1]
                                    ];
                                }
                            }
                            else if (ItemVariety.tex == 1 && ItemVariety.lollyMesh != null)
                                grabbableObject.mainObjectRenderer.GetComponent<MeshFilter>().mesh = ItemVariety.lollyMesh;
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "PerfumeBottle" && grabbableObject.mainObjectRenderer?.sharedMaterials != null && grabbableObject.mainObjectRenderer.sharedMaterials.Length == 2 && grabbableObject.mainObjectRenderer.sharedMaterials[0].name.StartsWith("Material.004"))
                    {
                        ItemVariety.GetSkinIndices(grabbableObject, 4, 6);

                        if (Plugin.configPerfume.Value && ItemVariety.tex != 0)
                        {
                            Color color = default, transColor = default;
                            switch (ItemVariety.tex)
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

                        if (Plugin.configPerfumeMeshes.Value && ItemVariety.perfumeMeshes != null && ItemVariety.perfumeMeshes.Length >= 5 && ItemVariety.mesh != 0)
                        {
                            grabbableObject.mainObjectRenderer.GetComponent<MeshFilter>().mesh = ItemVariety.perfumeMeshes[ItemVariety.mesh - 1];
                            Plugin.Logger.LogDebug($"Perfume #{networkObject.NetworkObjectId} using alternate model");
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "PillBottle" && grabbableObject.mainObjectRenderer?.sharedMaterial != null && grabbableObject.mainObjectRenderer.sharedMaterial.name.StartsWith("Material.002"))
                    {
                        ItemVariety.GetSkinIndices(grabbableObject);

                        if (Plugin.configPills.Value && ItemVariety.pillBottle2 != null && ItemVariety.tex == 1)
                        {
                            grabbableObject.mainObjectRenderer.material.mainTexture = ItemVariety.pillBottle2;
                            Plugin.Logger.LogDebug($"Pills #{networkObject.NetworkObjectId} using alternate texture");
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "Mug" && grabbableObject.mainObjectRenderer?.sharedMaterial != null && grabbableObject.mainObjectRenderer.sharedMaterial.name.StartsWith("CoffeeMug"))
                    {
                        ItemVariety.GetSkinIndices(grabbableObject, 6);

                        if (Plugin.configMug.Value && ItemVariety.coffeeMug6 != null && ItemVariety.tex == 5)
                        {
                            grabbableObject.mainObjectRenderer.material = ItemVariety.coffeeMug6;
                            Plugin.Logger.LogDebug($"Mug #{networkObject.NetworkObjectId} using alternate texture");
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "ControlPad" && grabbableObject.TryGetComponent(out Renderer rend) && rend.sharedMaterial != null && rend.sharedMaterial.name.StartsWith("ArcadeControlPanel"))
                    {
                        ItemVariety.GetSkinIndices(grabbableObject);

                        if (Plugin.configControlPad.Value && ItemVariety.controlPad2 != null && ItemVariety.tex == 1)
                        {
                            rend.material.mainTexture = ItemVariety.controlPad2;
                            Plugin.Logger.LogDebug($"Controller #{networkObject.NetworkObjectId} using alternate texture");
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "FishTestProp" && grabbableObject.mainObjectRenderer.TryGetComponent(out MeshFilter mesh))
                    {
                        // use tex for banana chance, mesh for others
                        ItemVariety.GetSkinIndices(grabbableObject, 10, 3);

                        if (Plugin.configFish.Value)
                        {
                            if (ItemVariety.tex == 8)
                            {
                                if (ItemVariety.sardineBanana == null)
                                    continue;

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
                                switch (ItemVariety.mesh)
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
                    else if (grabbableObject.itemProperties.name == "SteeringWheel" && grabbableObject.mainObjectRenderer != null && grabbableObject.mainObjectRenderer.sharedMaterial != null && grabbableObject.mainObjectRenderer.sharedMaterial.name.StartsWith("DirtySmoothSteel"))
                    {
                        ItemVariety.GetSkinIndices(grabbableObject);

                        if (Plugin.configSteeringWheel.Value && ItemVariety.darkPlastic != null && ItemVariety.tex == 1)
                        {
                            grabbableObject.mainObjectRenderer.material = ItemVariety.darkPlastic;
                            Plugin.Logger.LogDebug($"Wheel #{networkObject.NetworkObjectId} using alternate texture");
                        }
                    }
                    else if (grabbableObject.itemProperties.name == "PickleJar" && grabbableObject.TryGetComponent(out rend) && grabbableObject.TryGetComponent(out mesh) && rend.sharedMaterials != null && rend.sharedMaterials.Length == 2 && rend.sharedMaterials[0].name.StartsWith("JarGlass"))
                    {
                        ItemVariety.GetSkinIndices(grabbableObject);

                        Transform pickles = grabbableObject.transform.Find("Pickles.001");
                        if (Plugin.configPickles.Value && pickles != null && ItemVariety.glassJar != null && ItemVariety.jarGlass != null && ItemVariety.jarLid != null && ItemVariety.candyGlob != null && ItemVariety.tex == 1)
                        {
                            pickles.gameObject.SetActive(false);

                            mesh.mesh = ItemVariety.glassJar;
                            rend.materials =
                            [
                                ItemVariety.jarGlass,
                                ItemVariety.jarLid
                            ];

                            Object.Instantiate(ItemVariety.candyGlob, grabbableObject.transform);

                            ScanNodeProperties scanNodeProperties = grabbableObject.GetComponentInChildren<ScanNodeProperties>();
                            if (scanNodeProperties != null)
                                scanNodeProperties.headerText = scanNodeProperties.headerText.Replace("Jar of pickles", "Candy filled jar"); // replace() in case of localization

                            grabbableObject.gameObject.AddComponent<CandyFilledJar>();

                            Plugin.Logger.LogDebug($"Pickles #{networkObject.NetworkObjectId} is alternate");
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.ChangeControlTipMultiple))]
        [HarmonyPostfix]
        static void HUDManagerPostChangeControlTipMultiple(HUDManager __instance, bool holdingItem, Item itemProperties)
        {
            if (!holdingItem || itemProperties.name != "PickleJar")
                return;

            if (GameNetworkManager.Instance?.localPlayerController?.currentlyHeldObjectServer?.GetComponent<CandyFilledJar>() != null)
                __instance.controlTipLines[0].text = __instance.controlTipLines[0].text.Replace("Jar of pickles", "Candy jar");
        }
    }

    internal class ItemVariety
    {
        internal static AudioClip boombox;
        internal static Texture giftBoxTex2, pillBottle2, controlPad2;
        internal static Material lollipop2, lollyStick, coffeeMug6, darkPlastic, candyPink, jarGlass, jarLid;
        internal static Color perfumeRed = new(0.9f, 0.3922666f, 0.324f, 0.6313726f),
                              perfumeRedTrans = new(1f, 0.4f, 0.4f),
                              perfumeBlue = new(0.4509091f, 0.416f, 0.8f, 0.6313726f),
                              perfumeBlueTrans = new(0.59f, 0.6241666f, 1f),
                              perfumeBlack = new(0.01f, 0.01f, 0.01f, 0.6313726f),
                              perfumeBlackTrans = new(0.48f, 0.48f, 0.48f),
                              /*perfumePink = new(0.9f, 0.297f, 0.5985f),
                              perfumePinkTrans = new(1f, 0.52f, 0.7276597f)*/
                              fishYellow = new(0.773024f, 0.8392157f, 0.3692549f),
                              fishRed = new(0.5019608f, 0.1780566f, 0.1556078f),
                              banana = new(0.44f, 0.4296874f, 0.2028124f),
                              stem = new(0.3799999f, 0.243271f, 0.1527103f);
        internal static Mesh lollyMesh, sucker, fish2, sardine, sardineBanana, glassJar;
        internal static Mesh[] perfumeMeshes;
        internal static GameObject candyGlob;

        internal static HashSet<ulong> cache = [];
        internal static Dictionary<string, int> offsets = [];

        internal static int tex = -1, mesh = -1;

        internal static void GetSkinIndices(GrabbableObject obj, int texCount = 2, int meshCount = -1)
        {
            if (offsets.ContainsKey(obj.itemProperties.name))
                offsets[obj.itemProperties.name]++;
            else
                offsets.Add(obj.itemProperties.name, 0);

            System.Random rngSeed = new(StartOfRound.Instance.randomMapSeed + (int)obj.targetFloorPosition.x + (int)obj.targetFloorPosition.z + offsets[obj.itemProperties.name]);

            if (texCount >= 0)
                tex = rngSeed.Next(texCount);
            if (meshCount >= 0)
                mesh = rngSeed.Next(meshCount);
        }
    }
}
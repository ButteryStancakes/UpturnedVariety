using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedLamp : IUpturnedItem
    {
        static readonly Color LAMP_STAND_UPTURNED = new(0.2395526f, 0.2213185f, 0.1565525f),
                              RUBBER_HANDLE_UPTURNED = new(0.01842319f, 0.01562534f, 0.01562534f),
                              DARK_STEEL_UPTURNED = new(0.2547169f, 0.2547169f, 0.2547169f),
                              LAMPSHADE_LETHAL = new(0.513327f, 0.4134762f, 0.3871232f),
                              LAMP_STAND_LETHAL = new(0.5266604f, 0.5077859f, 0.4321852f),
                              DARK_STEEL_LETHAL = new(0.1733264f, 0.1733264f, 0.1733264f);

        static ConfigEntry<bool> enableMeshes;

        static Material lampshade;
        static Mesh cylinder;

        public string GetItemName()
        {
            return "FancyLamp";
        }

        public int[] GetRandomData()
        {
            return [2, 2]; // 2 palettes, 2 models
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableMeshes = cfg.Bind(
                "Items",
                "Lamp",
                true,
                "Enables alternate models for the \"Fancy lamp\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            lampshade = bundle.LoadAsset<Material>("Lampshade");

            cylinder = bundle.LoadAsset<Mesh>("Cylinder.000");

            if (lampshade == null || cylinder == null)
                throw new System.Exception("Failed to load assets for \"Fancy lamp\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.mainObjectRenderer?.sharedMaterial != null && item.mainObjectRenderer.sharedMaterial.name.StartsWith("FancyLampTex");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            // [1] == 1 is alternate model
            if (enableMeshes.Value && rand[1] == 1)
            {
                item.mainObjectRenderer.GetComponent<MeshFilter>().mesh = cylinder;
                Plugin.Logger.LogDebug($"Lamp #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate model");

                Material baseMaterial = Object.Instantiate(item.mainObjectRenderer.sharedMaterial);
                baseMaterial.SetTexture("_MainTex", null);
                baseMaterial.SetTexture("_BaseColorMap", null);

                Material[] mats =
                [
                    null,
                    baseMaterial,
                    Object.Instantiate(baseMaterial),
                    Object.Instantiate(baseMaterial)
                ];

                Light light = item.mainObjectRenderer.GetComponentInChildren<Light>();
                light.transform.localPosition = new(light.transform.localPosition.x, light.transform.localPosition.y, 1.626763f);

                // upturned
                if (rand[0] == 0)
                {
                    // Lampshade
                    mats[0] = lampshade;
                    // LampStand
                    mats[1].SetColor("_Color", LAMP_STAND_UPTURNED);
                    mats[1].SetColor("_BaseColor", LAMP_STAND_UPTURNED);
                    mats[1].SetFloat("_Metallic", 0.7f);
                    mats[1].SetFloat("_Smoothness", 0.5f);
                    // RubberHandle
                    mats[2].SetColor("_Color", RUBBER_HANDLE_UPTURNED);
                    mats[2].SetColor("_BaseColor", RUBBER_HANDLE_UPTURNED);
                    mats[2].SetFloat("_Metallic", 0f);
                    mats[2].SetFloat("_Smoothness", 0.5f);
                    // DarkSteel
                    mats[3].SetColor("_Color", DARK_STEEL_UPTURNED);
                    mats[3].SetColor("_BaseColor", DARK_STEEL_UPTURNED);
                    mats[3].SetFloat("_Metallic", 0.724f);
                    mats[3].SetFloat("_Smoothness", 0.5f);

                    light.colorTemperature = 4846f;
                }
                // v50 intro
                else
                {
                    // Lampshade
                    mats[0] = Object.Instantiate(baseMaterial);
                    mats[0].SetColor("_Color", LAMPSHADE_LETHAL);
                    mats[0].SetColor("_BaseColor", LAMPSHADE_LETHAL);
                    mats[0].SetFloat("_Metallic", 0f);
                    mats[0].SetFloat("_Smoothness", 0.5f);
                    // LampStand
                    mats[1].SetColor("_Color", LAMP_STAND_LETHAL);
                    mats[1].SetColor("_BaseColor", LAMP_STAND_LETHAL);
                    mats[1].SetFloat("_Metallic", 0.4583333f);
                    mats[1].SetFloat("_Smoothness", 0.5f);
                    // BlackRubber
                    mats[2].SetColor("_Color", Color.black);
                    mats[2].SetColor("_BaseColor", Color.black);
                    mats[2].SetFloat("_Metallic", 0f);
                    mats[2].SetFloat("_Smoothness", 0.0916667f);
                    // DarkSteel
                    mats[3].SetColor("_Color", DARK_STEEL_LETHAL);
                    mats[3].SetColor("_BaseColor", DARK_STEEL_LETHAL);
                    mats[3].SetFloat("_Metallic", 0.3416667f);
                    mats[3].SetFloat("_Smoothness", 0.55f);

                    light.colorTemperature = 4412f;
                }
                item.mainObjectRenderer.sharedMaterials = mats;
            }
        }
    }
}
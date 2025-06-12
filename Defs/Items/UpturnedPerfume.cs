using BepInEx.Configuration;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedPerfume : IUpturnedItem
    {
        static ConfigEntry<bool> enableTextures, enableMeshes;

        static Color red = new(0.9f, 0.3922666f, 0.324f, 0.6313726f),
                     redTrans = new(1f, 0.4f, 0.4f),
                     blue = new(0.4509091f, 0.416f, 0.8f, 0.6313726f),
                     blueTrans = new(0.59f, 0.6241666f, 1f),
                     black = new(0.01f, 0.01f, 0.01f, 0.6313726f),
                     blackTrans = new(0.48f, 0.48f, 0.48f);
        static Mesh[] meshes;

        public string GetItemName()
        {
            return "PerfumeBottle";
        }

        public int[] GetRandomData()
        {
            return [4, 6]; // 4 colors, 6 models
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableTextures = cfg.Bind(
                "Items",
                "Perfume",
                true,
                "Enables alternate palettes for the \"Perfume bottle\" item.");

            enableMeshes = cfg.Bind(
                "Items",
                "PerfumeModels",
                true,
                "Enables alternate models for the \"Perfume bottle\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            meshes = [
                bundle.LoadAsset<Mesh>("PerfumeBottle_001"),
                bundle.LoadAsset<Mesh>("PerfumeBottle_002"),
                bundle.LoadAsset<Mesh>("PerfumeBottle_003"),
                bundle.LoadAsset<Mesh>("PerfumeBottle_004"),
                bundle.LoadAsset<Mesh>("PerfumeBottle_005"),
            ];

            if (meshes.Any(mesh => mesh is null))
                throw new System.Exception("Failed to load assets for \"Perfume bottle\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.mainObjectRenderer?.sharedMaterials != null && item.mainObjectRenderer.sharedMaterials.Length == 2 && item.mainObjectRenderer.sharedMaterials[0].name.StartsWith("Material.004");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            // [0] != 0 is alternate color
            if (enableTextures.Value && rand[0] > 0)
            {
                Color color = default, transColor = default;
                switch (rand[0])
                {
                    case 1:
                        color = red;
                        transColor = redTrans;
                        break;
                    case 2:
                        color = blue;
                        transColor = blueTrans;
                        break;
                    case 3:
                        color = black;
                        transColor = blackTrans;
                        break;
                }
                Material glass = item.mainObjectRenderer.materials[0];
                glass.SetColor("_Color", color);
                glass.SetColor("_BaseColor", color);
                glass.SetColor("_TransmittanceColor", transColor);
                item.mainObjectRenderer.materials =
                [
                    glass,
                    item.mainObjectRenderer.sharedMaterials[1]
                ];
                Plugin.Logger.LogDebug($"Perfume #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate color");
            }

            // [1] != 0 is alternate model
            if (enableMeshes.Value && rand[1] > 0 && rand[1] <= meshes.Length)
            {
                item.mainObjectRenderer.GetComponent<MeshFilter>().mesh = meshes[rand[1] - 1];
                Plugin.Logger.LogDebug($"Perfume #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate model");
            }
        }
    }
}
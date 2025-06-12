using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedCandy : IUpturnedItem
    {
        static ConfigEntry<bool> enableTextures, enableMeshes;

        static Material lollyPop, lightWood, candyPink;
        static Mesh cylinder, sucker;

        public string GetItemName()
        {
            return "Candy";
        }

        public int[] GetRandomData()
        {
            return [2, 2]; // 2 textures, 2 models
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableTextures = cfg.Bind(
                "Items",
                "Perfume",
                true,
                "Enables alternate palettes for the \"Candy\" item.");

            enableMeshes = cfg.Bind(
                "Items",
                "PerfumeModels",
                true,
                "Enables alternate models for the \"Candy\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            lollyPop = bundle.LoadAsset<Material>("LollyPop");
            lightWood = bundle.LoadAsset<Material>("LightWood");
            candyPink = bundle.LoadAsset<Material>("CandyPink");

            cylinder = bundle.LoadAsset<Mesh>("Cylinder.001");
            sucker = bundle.LoadAsset<Mesh>("Sucker");

            if (lollyPop == null || lightWood == null || candyPink == null || cylinder == null || sucker == null)
                throw new System.Exception("Failed to load assets for \"Candy\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.mainObjectRenderer?.sharedMaterials != null && item.mainObjectRenderer.sharedMaterials.Length == 2 && item.mainObjectRenderer.sharedMaterials[0].name.StartsWith("LollyPop");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            // [0] == 1 is alternate texture
            if (enableTextures.Value && rand[0] == 1)
            {
                item.mainObjectRenderer.materials =
                [
                    lollyPop,
                    lightWood
                ];
                Plugin.Logger.LogDebug($"Candy #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate texture");

                // not using alt model?
                if (rand[1] == 0)
                {
                    // use higher poly version of original model (it looks better with the texture)
                    item.mainObjectRenderer.GetComponent<MeshFilter>().mesh = cylinder;
                }
            }

            // [1] == 1 is alternate model
            if (enableMeshes.Value && rand[1] == 1)
            {
                item.mainObjectRenderer.GetComponent<MeshFilter>().mesh = sucker;
                Plugin.Logger.LogDebug($"Candy #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate model");

                // not using alt tex?
                if (rand[0] == 0)
                {
                    // use pink color, like original v9 WIP candy
                    item.mainObjectRenderer.materials =
                    [
                        candyPink,
                        item.mainObjectRenderer.sharedMaterials[1]
                    ];
                }
            }
        }
    }
}
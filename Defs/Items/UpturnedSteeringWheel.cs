using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedSteeringWheel : IUpturnedItem
    {
        static ConfigEntry<bool> enableTextures;

        static Material darkPlastic;

        public string GetItemName()
        {
            return "SteeringWheel";
        }

        public int[] GetRandomData()
        {
            return [2];
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableTextures = cfg.Bind(
                "Items",
                "SteeringWheel",
                true,
                "Enables alternate palettes for the \"Steering wheel\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            darkPlastic = bundle.LoadAsset<Material>("DarkPlastic");

            if (darkPlastic == null)
                throw new System.Exception("Failed to load assets for \"Steering wheel\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.mainObjectRenderer != null && item.mainObjectRenderer.sharedMaterial != null && item.mainObjectRenderer.sharedMaterial.name.StartsWith("DirtySmoothSteel");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            if (enableTextures.Value && rand[0] == 1)
            {
                item.mainObjectRenderer.material = darkPlastic;
                Plugin.Logger.LogDebug($"Wheel #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate texture");
            }
        }
    }
}
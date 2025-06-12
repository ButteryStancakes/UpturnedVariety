using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedMug : IUpturnedItem
    {
        static ConfigEntry<bool> enableTextures;

        static Material coffeeMug6;

        public string GetItemName()
        {
            return "Mug";
        }

        public int[] GetRandomData()
        {
            return [6]; // 5 vanilla variants + Upturned
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableTextures = cfg.Bind(
                "Items",
                "Mug",
                true,
                "Enables alternate palettes for the \"Mug\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            coffeeMug6 = bundle.LoadAsset<Material>("CoffeeMug6");

            if (coffeeMug6 == null)
                throw new System.Exception("Failed to load assets for \"Mug\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.mainObjectRenderer?.sharedMaterial != null && item.mainObjectRenderer.sharedMaterial.name.StartsWith("CoffeeMug");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            if (enableTextures.Value && rand[0] == 5)
            {
                item.mainObjectRenderer.material = coffeeMug6;
                Plugin.Logger.LogDebug($"Mug #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate texture");
            }
        }
    }
}
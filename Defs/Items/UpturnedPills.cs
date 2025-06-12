using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedPills : IUpturnedItem
    {
        static ConfigEntry<bool> enableTextures;

        static Texture pillBottleTextureB2;

        public string GetItemName()
        {
            return "PillBottle";
        }

        public int[] GetRandomData()
        {
            return [2];
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableTextures = cfg.Bind(
                "Items",
                "Pills",
                true,
                "Enables alternate palettes for the \"Pill bottle\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            pillBottleTextureB2 = bundle.LoadAsset<Texture>("PillBottleTextureB2");

            if (pillBottleTextureB2 == null)
                throw new System.Exception("Failed to load assets for \"Pill bottle\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.mainObjectRenderer?.sharedMaterial != null && item.mainObjectRenderer.sharedMaterial.name.StartsWith("Material.002");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            if (enableTextures.Value && rand[0] == 1)
            {
                item.mainObjectRenderer.material.mainTexture = pillBottleTextureB2;
                Plugin.Logger.LogDebug($"Pills #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate texture");
            }
        }
    }
}
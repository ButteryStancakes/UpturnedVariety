using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedControlPad : IUpturnedItem
    {
        static ConfigEntry<bool> enableTextures;

        static Texture arcadeControlPanel2;

        public string GetItemName()
        {
            return "ControlPad";
        }

        public int[] GetRandomData()
        {
            return [2];
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableTextures = cfg.Bind(
                "Items",
                "ControlPad",
                true,
                "Enables alternate palettes for the \"Control pad\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            arcadeControlPanel2 = bundle.LoadAsset<Texture>("ArcadeControlPanel2");

            if (arcadeControlPanel2 == null)
                throw new System.Exception("Failed to load assets for \"Control pad\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.TryGetComponent(out Renderer rend) && rend.sharedMaterial != null && rend.sharedMaterial.name.StartsWith("ArcadeControlPanel");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            if (enableTextures.Value && rand[0] == 1)
            {
                item.GetComponent<Renderer>().material.mainTexture = arcadeControlPanel2;
                Plugin.Logger.LogDebug($"Controller #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate texture");
            }
        }
    }
}
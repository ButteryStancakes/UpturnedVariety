using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedGiftBox : IUpturnedItem
    {
        static ConfigEntry<bool> enableTextures;

        static Texture giftBoxTex2;

        public string GetItemName()
        {
            return "GiftBox";
        }

        public int[] GetRandomData()
        {
            return [2];
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableTextures = cfg.Bind(
                "Items",
                "Gift",
                true,
                "Enables alternate palettes for the \"Gift box\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            giftBoxTex2 = bundle.LoadAsset<Texture>("GiftBoxTex2");

            if (giftBoxTex2 == null)
                throw new System.Exception("Failed to load assets for \"Gift box\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item is GiftBoxItem && item.itemProperties.itemId == 152767;
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            if (enableTextures.Value && rand[0] == 1)
            {
                item.GetComponent<Renderer>().material.mainTexture = giftBoxTex2;
                Plugin.Logger.LogDebug($"Gift #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate texture");
            }
        }
    }
}
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UpturnedVariety.Defs.Items;

namespace UpturnedVariety
{
    internal class SkinManager
    {
        static readonly List<IUpturnedItem> allSkinnedItems =
        [
            new UpturnedGiftBox(),
            new UpturnedCandy(),
            new UpturnedPerfume(),
            new UpturnedPills(),
            new UpturnedMug(),
            new UpturnedControlPad(),
            new UpturnedFish(),
            new UpturnedSteeringWheel(),
            new UpturnedPickles(),
            new UpturnedLamp()
        ];

        static HashSet<ulong> cache = [];
        static System.Random random;

        internal static List<Renderer> fish = [];

        internal static void LoadAllConfigs(ConfigFile cfg)
        {
            foreach (IUpturnedItem skinnedItem in allSkinnedItems)
                skinnedItem.LoadConfigs(cfg);
        }

        internal static void LoadAllSkins(AssetBundle bundle)
        {
            foreach (IUpturnedItem skinnedItem in allSkinnedItems)
                skinnedItem.LoadSkins(bundle);
        }

        internal static void Reset()
        {
            cache.Clear();
            fish.Clear();
        }

        internal static void ApplySkinsToItems(NetworkObjectReference[] netRefs)
        {
            random = new(StartOfRound.Instance.randomMapSeed + 32322);

            foreach (NetworkObjectReference netRef in netRefs)
            {
                // validate the item first
                if (!netRef.TryGet(out NetworkObject netObj) || !cache.Add(netObj.NetworkObjectId) || !netObj.TryGetComponent(out GrabbableObject item))
                    continue;

                IUpturnedItem skinnedItem = allSkinnedItems.FirstOrDefault(skin => skin.GetItemName() == item.itemProperties.name);
                if (skinnedItem != null)
                {
                    // always generate random values, to prevent different configs having desynced seeding
                    int[] randTemp = skinnedItem.GetRandomData();
                    int[] randValues = new int[randTemp.Length];
                    for (int i = 0; i < randTemp.Length; i++)
                        randValues[i] = random.Next(randTemp[i]);

                    // apply changes based on random values
                    if (skinnedItem.MatchToVanillaItem(item))
                        skinnedItem.ApplySkin(item, randValues);
                }
            }
        }

        internal static bool IsThereABanana()
        {
            bool missing = false;
            bool banana = false;

            for (int i = 0; i < fish.Count; i++)
            {
                if (fish[i] == null)
                {
                    missing = true;
                    continue;
                }

                if (fish[i].sharedMaterials != null && fish[i].sharedMaterials.Length == 3)
                    banana = true;
            }

            if (missing)
                fish.RemoveAll(x => x is null);

            return banana;
        }
    }
}

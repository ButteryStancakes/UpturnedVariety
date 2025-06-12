using BepInEx.Configuration;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal interface IUpturnedItem
    {
        internal string GetItemName();

        // array should be amount of numbers to generate, with each index the value range
        // i.e. [2,3] will generate 2 random numbers, with the first 0 or 1 and the second 0, 1, or 2
        internal int[] GetRandomData();

        internal void LoadConfigs(ConfigFile cfg);

        internal void LoadSkins(AssetBundle bundle);

        // in case additional material, mesh, etc. checks are necessary before applying skin
        internal bool MatchToVanillaItem(GrabbableObject item);

        internal void ApplySkin(GrabbableObject item, int[] rand);
    }
}

using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedPickles : IUpturnedItem
    {
        static ConfigEntry<bool> enableMeshes;

        static Material glassCase, displayCasePlastic;
        static Mesh glassJar;
        static GameObject candyGlob;

        public string GetItemName()
        {
            return "PickleJar";
        }

        public int[] GetRandomData()
        {
            return [2];
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableMeshes = cfg.Bind(
                "Items",
                "Pickles",
                true,
                "Enables alternate models for the \"Jar of pickles\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            glassJar = bundle.LoadAsset<Mesh>("GlassJar");
            glassCase = bundle.LoadAsset<Material>("GlassCase");
            displayCasePlastic = bundle.LoadAsset<Material>("DisplayCasePlastic");
            candyGlob = bundle.LoadAsset<GameObject>("CandyGlob");

            if (glassJar == null || glassCase == null || displayCasePlastic == null || candyGlob == null)
                throw new System.Exception("Failed to load assets for \"Jar of pickles\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.TryGetComponent(out Renderer rend) && rend.sharedMaterials != null && rend.sharedMaterials.Length == 2 && rend.sharedMaterials[0].name.StartsWith("JarGlass") && item.transform.Find("Pickles.001") != null;
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {
            if (enableMeshes.Value && rand[0] == 1)
            {
                item.transform.Find("Pickles.001").gameObject.SetActive(false);

                item.GetComponent<MeshFilter>().mesh = glassJar;
                item.GetComponent<Renderer>().materials =
                [
                    glassCase,
                    displayCasePlastic
                ];

                Object.Instantiate(candyGlob, item.transform);

                ScanNodeProperties scanNodeProperties = item.GetComponentInChildren<ScanNodeProperties>();
                if (scanNodeProperties != null)
                    scanNodeProperties.headerText = "Candy filled jar";

                item.gameObject.AddComponent<SubstituteItemName>().subName = "Candy jar";

                Plugin.Logger.LogDebug($"Pickles #{item.GetComponent<NetworkObject>().NetworkObjectId} is alternate");
            }
        }
    }
}
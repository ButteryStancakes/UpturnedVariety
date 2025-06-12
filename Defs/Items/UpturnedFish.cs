using BepInEx.Configuration;
using Unity.Netcode;
using UnityEngine;

namespace UpturnedVariety.Defs.Items
{
    internal class UpturnedFish : IUpturnedItem
    {
        static ConfigEntry<bool> enableMeshes;

        static Color fishYellow = new(0.773024f, 0.8392157f, 0.3692549f),
                     fishRed = new(0.5019608f, 0.1780566f, 0.1556078f),
                     banana = new(0.44f, 0.4296874f, 0.2028124f),
                     stem = new(0.3799999f, 0.243271f, 0.1527103f);
        static Mesh fish2, sardine, sardine001;

        public string GetItemName()
        {
            return "FishTestProp";
        }

        public int[] GetRandomData()
        {
            return [4, 3]; // 4 variants, but backup in case there's already a banana
        }

        public void LoadConfigs(ConfigFile cfg)
        {
            enableMeshes = cfg.Bind(
                "Items",
                "Fish",
                true,
                "Enables alternate models for the \"Plastic fish\" item.");
        }

        public void LoadSkins(AssetBundle bundle)
        {
            fish2 = bundle.LoadAsset<Mesh>("Fish2");
            sardine = bundle.LoadAsset<Mesh>("Sardine");
            sardine001 = bundle.LoadAsset<Mesh>("Sardine.001");

            if (fish2 == null || sardine == null || sardine001 == null)
                throw new System.Exception("Failed to load assets for \"Plastic fish\" item");
        }

        public bool MatchToVanillaItem(GrabbableObject item)
        {
            return item.mainObjectRenderer?.sharedMaterials != null && item.mainObjectRenderer.sharedMaterials.Length == 2 && item.mainObjectRenderer.sharedMaterials[0].name.StartsWith("Eye");
        }

        public void ApplySkin(GrabbableObject item, int[] rand)
        {

            if (enableMeshes.Value)
            {
                int skin = rand[0];
                // the banana fish is critically endangered; the last of its kind
                if (skin == 3 && SkinManager.IsThereABanana())
                    skin = rand[1];

                if (skin > 0)
                {
                    MeshFilter mesh = item.mainObjectRenderer.GetComponent<MeshFilter>();
                    switch (skin)
                    {
                        // yellow
                        case 1:
                            mesh.mesh = fish2;
                            Material yellowFish = item.mainObjectRenderer.materials[1];
                            yellowFish.SetColor("_Color", fishYellow);
                            yellowFish.SetColor("_BaseColor", fishYellow);
                            item.mainObjectRenderer.materials = [
                                yellowFish,
                                item.mainObjectRenderer.sharedMaterials[0]
                            ];
                            break;
                        // red
                        case 2:
                            mesh.mesh = sardine;
                            Material silverFish = item.mainObjectRenderer.materials[1];
                            silverFish.SetColor("_Color", fishRed);
                            silverFish.SetColor("_BaseColor", fishRed);
                            item.mainObjectRenderer.materials = [
                                item.mainObjectRenderer.sharedMaterials[0],
                                silverFish
                            ];
                            break;
                        // banana
                        case 3:
                            mesh.mesh = sardine001;
                            Material yellowRubber = item.mainObjectRenderer.materials[1];
                            Material material001 = Object.Instantiate(yellowRubber);
                            yellowRubber.SetColor("_Color", banana);
                            yellowRubber.SetColor("_BaseColor", banana);
                            material001.SetColor("_Color", stem);
                            material001.SetColor("_BaseColor", stem);
                            item.mainObjectRenderer.materials = [
                                item.mainObjectRenderer.sharedMaterials[0],
                                yellowRubber,
                                material001
                            ];
                            break;
                    }
                    Plugin.Logger.LogDebug($"Fish #{item.GetComponent<NetworkObject>().NetworkObjectId} using alternate model");
                }
            }
        }
    }
}
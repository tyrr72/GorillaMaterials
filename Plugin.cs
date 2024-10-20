using BepInEx;
using GorillaNetworking;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Utilla;

namespace Matrix
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private Material newMat;
        private int matIndex = 1;
        private const int totalMats = 8;
        private string cosmeticPath;
        private GameObject cosmeticsParent;

        public void Start()
        {
            cosmeticsParent = new GameObject("CustomCosmeticsTy");
            cosmeticsParent.transform.position = Vector3.zero;

            cosmeticPath = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "Cosmetics");

            GorillaTagger.OnPlayerSpawned(Init);
        }

        private AssetBundle LoadAssetBundle(string path)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (stream == null)
            {
                Logger.LogError($"Failed to load stream for asset bundle at {path}");
                return null;
            }

            return AssetBundle.LoadFromStream(stream);
        }

        public async void Init()
        {
            var bundle = LoadAssetBundle("Matrix.materials");
            if (bundle != null)
            {
                var asset = bundle.LoadAsset<GameObject>("MaterialMod");
                if (asset != null)
                {
                    Instantiate(asset, cosmeticsParent.transform);
                }
                else
                {
                    Logger.LogError("MaterialMod asset not found in the bundle.");
                }
            }
            else
            {
                Logger.LogError("Failed to load asset bundle.");
            }
        }

        public void OnGUI()
        {
            GUILayout.Label($"Index: {matIndex}");
            // GUILayout.Label($"Current Mat Index: {GorillaTagger.Instance.offlineVRRig.setMatIndex}");

            if (GUILayout.Button("-"))
            {
                matIndex = Mathf.Max(1, matIndex - 1);
            }
            if (GUILayout.Button("+"))
            {
                matIndex = Mathf.Min(totalMats, matIndex + 1);
            }
        }

        public void Update()
        {
            GameObject matObject = GameObject.Find("Mat" + matIndex);
            if (matObject != null)
            {
                newMat = matObject.GetComponent<Renderer>().material;

                VRRig rig = GorillaTagger.Instance.offlineVRRig;
                if (rig.materialsToChangeTo[0] != newMat && rig.setMatIndex == 0)
                {
                    rig.materialsToChangeTo[0] = newMat;

                    Material[] sharedMaterials = rig.mainSkin.sharedMaterials;
                    sharedMaterials[0] = rig.materialsToChangeTo[0];
                    sharedMaterials[1] = rig.defaultSkin.chestMaterial;

                    rig.mainSkin.sharedMaterials = sharedMaterials;

                    if (newMat.name.Contains("1"))
                    {
                        newMat.color = rig.playerColor;
                    }
                }
            }
            else
            {
                Logger.LogWarning($"Material object 'Mat{matIndex}' not found.");
            }
        }
    }
}

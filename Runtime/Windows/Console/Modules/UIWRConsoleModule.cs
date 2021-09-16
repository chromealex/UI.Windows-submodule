using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    [Help("UI.Windows Resources Console Module")]
    [Alias("uiwr")]
    public class UIWRConsoleModule : ConsoleModule {

        [Help("Prints all currently used resources")]
        public void Stat() {

            var res = WindowSystem.GetResources();
            Debug.Log("Allocated Objects Count: " + res.GetAllocatedCount());

            Debug.Log("Bundles:");
            var allBundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (var bundle in allBundles) {

                Debug.Log(bundle.name);

            }
            
            Debug.Log("Loaded Assets:");
            foreach (var asset in res.GetLoadedAssets()) {

                Debug.Log(asset);

            }

        }

        [Help("Prints all assets in bundle")]
        public void Get(string bundleName) {

            var allBundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (var bundle in allBundles) {

                if (System.String.Equals(bundle.name, bundleName, System.StringComparison.CurrentCultureIgnoreCase) == true) {

                    foreach (var asset in bundle.GetAllAssetNames()) {
                        
                        Debug.Log(asset);
                        
                    }
                    break;
                    
                }

            }

        }

    }

}
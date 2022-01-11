using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    [UnityEngine.Scripting.PreserveAttribute]
    [Help("UI.Windows Resources Console Module")]
    [Alias("uiwr")]
    public class UIWRConsoleModule : ConsoleModule {

        [UnityEngine.Scripting.PreserveAttribute]
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
            foreach (var asset in res.GetAllObjects()) {

                Debug.Log(asset.Value.loaded);

            }

        }

        [UnityEngine.Scripting.PreserveAttribute]
        [Help("Prints all assets in bundle")]
        public void Bundle(string bundleName) {

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

        [UnityEngine.Scripting.PreserveAttribute]
        [Help("Prints all static non-null references")]
        public void Static() {

            Debug.Log("Static Fields:");
            var asms = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms) {

                if (asm.FullName.StartsWith("mscorlib") == true) continue;
                if (asm.FullName.StartsWith("Unity") == true) continue;
                if (asm.FullName.StartsWith("System") == true) continue;
                if (asm.FullName.StartsWith("Mono") == true) continue;
                if (asm.FullName.StartsWith("Assembly-CSharp-Editor") == true) continue;
                
                var types = asm.GetTypes();
                foreach (var type in types) {

                    var fields = type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    foreach (var field in fields) {

                        try {

                            if (field.FieldType.IsEnum == true) continue;
                            if (field.FieldType.IsPointer == true) continue;
                            if (field.FieldType.IsPrimitive == true) continue;
                            if (field.FieldType == typeof(string)) continue;
                            
                            var val = field.GetValue(null);
                            if (val != null) {

                                var prev = UnityEngine.Application.GetStackTraceLogType(LogType.Log);
                                UnityEngine.Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
                                Debug.Log(asm.FullName);
                                Debug.Log(type.FullName);
                                Debug.Log("  " + field.Name + ": " + val);
                                UnityEngine.Application.SetStackTraceLogType(UnityEngine.LogType.Log, prev);

                            }
                            
                        } catch (System.Exception) { }

                    }

                }
                
            }

        }

    }

}
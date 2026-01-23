namespace UnityEditor.UI.Windows.Essentials.AssetPostProcessor.Editor.Processors {
    
    using UnityEngine.UI.Windows.Essentials.AssetPostProcessor.Runtime;

    #if UIWS_ASSET_POSTPROCESSOR
    public class CustomAssetPostProcessor : UnityEditor.AssetPostprocessor {

        private const string CONFIG_PATH = "Assets/EditorResources/UI.Windows/AssetPostprocessorConfig.asset";
        public static AssetPostProcessorConfig ValidateConfig() {
            var config = AssetDatabase.LoadAssetAtPath<AssetPostProcessorConfig>(CONFIG_PATH);
            if (config == null) {
                config = UnityEngine.ScriptableObject.CreateInstance<AssetPostProcessorConfig>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
                config = AssetDatabase.LoadAssetAtPath<AssetPostProcessorConfig>(CONFIG_PATH);
            }
            return config;
        }

        private void OnPreprocessTexture() {
            var config = ValidateConfig();
            var path = this.assetPath;
            if (string.IsNullOrEmpty(path) == true) return;
            var importer = this.assetImporter as TextureImporter;
            if (importer == null) return;
            var labels = new System.Collections.Generic.List<string>();
            labels.AddRange(AssetDatabase.GetLabels(this.context.mainObject));
            var iter = 0;
            do {
                var dir = System.IO.Path.GetDirectoryName(path);
                if (dir == null) break;
                path = dir;
                labels.AddRange(AssetDatabase.GetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)));
                ++iter;
                if (iter == 100) {
                    UnityEngine.Debug.LogError($"max iter: {path}");
                    break; 
                }
            } while (path != "Assets");

            var item = config.GetItemByLabels(labels);
            if (item.IsValid == true) {
                item.preset?.ApplyTo(importer);
            }
        }

        /*private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

            var config = ValidateConfig();
            
            foreach (var path in importedAssets) {
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets) {
                    if (asset is UnityEngine.Sprite sprite) {
                        OnPostprocessSprite(config, path, sprite.texture, sprite);
                    }
                }
            }
            
        }

        private static void OnPostprocessSprite(AssetPostProcessorConfig config, string path, UnityEngine.Texture texture, UnityEngine.Sprite sprite) {

            var srcPath = path;
            var labels = new System.Collections.Generic.List<string>();
            labels.AddRange(AssetDatabase.GetLabels(texture));
            var iter = 0;
            do {
                var dir = System.IO.Path.GetDirectoryName(path);
                if (dir == null) break;
                path = dir;
                labels.AddRange(AssetDatabase.GetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)));
                ++iter;
                if (iter == 100) {
                    UnityEngine.Debug.LogError($"max iter: {path}");
                    break; 
                }
            } while (path != "Assets");

            foreach (var item in config.items) {
                foreach (var label in labels) {
                    if (item.label == label) {
                        ApplyFormat(srcPath, texture, item.format);
                    }
                }
            }
            
        }

        private static void ApplyFormat(string path, UnityEngine.Texture texture, Format format) {
            var apply = false;
            if (texture.width > format.maxSize || texture.height > format.maxSize) {
                apply = true;
            }

            var textureFormat = UnityEngine.Experimental.Rendering.GraphicsFormatUtility.GetTextureFormat(texture.graphicsFormat);
            if (textureFormat != format.format) {
                apply = true;
            }

            if (apply == true) {
                const string platform = "Default";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var platformSettings = importer.GetPlatformTextureSettings(platform);
                platformSettings.maxTextureSize = format.maxSize;
                platformSettings.format = (TextureImporterFormat)format.format;
                importer.SetPlatformTextureSettings(platformSettings);
                importer.SaveAndReimport();
            }
        }*/

    }
    #endif

}
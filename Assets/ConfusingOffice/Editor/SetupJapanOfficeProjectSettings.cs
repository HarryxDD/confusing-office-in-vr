using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace UnityJapanOffice
{
    public class SetupJapanOfficeProjectSettings
    {
        static readonly string hdrpSettingPath = "Assets/UnityJapanOffice/Settings/UTJOfficeHDRenderPipelineAsset.asset";
        [InitializeOnLoadMethod]
        public static void Init()
        {
            if (IsAlreadySetup()) {
                return;
            }
            bool flag= EditorUtility.DisplayDialog("Unity Japan Office",
                "Do you want to change Project settings for UnityJapanOffice Asset?\nGraphics Settings and Build target scenes will be changed.", "OK", "Cancel");
            if(flag)
            {
                Setup();
                EditorUtility.DisplayDialog("Unity Japan Office", "Changed Graphics Settings and Build target scenes.","OK");
            }
        }

        
        private static bool IsAlreadySetup()
        {
            var asset = GraphicsSettings.defaultRenderPipeline;
            if(asset == null) { return false; }
            var path = AssetDatabase.GetAssetPath(asset);
            return (path == hdrpSettingPath);
        }



        [MenuItem("Tools/UTJOffice/InitJapanOfficeSetup")]
        public static void Setup()
        {
            SetGraphicsSettings();
            RemoveGraphicsAssetFromQualitySettings();
            AddScenes();
        }
        private static void SetGraphicsSettings()
        {

            GraphicsSettings.defaultRenderPipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(hdrpSettingPath);
        }

        private static void RemoveGraphicsAssetFromQualitySettings()
        {
            var hdrpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(hdrpSettingPath);
            int cnt = QualitySettings.names.Length;
            for (int i = 0; i < cnt; ++i)
            {
                QualitySettings.SetQualityLevel(i);
                QualitySettings.renderPipeline = hdrpAsset;
            }
        }


        private static void AddScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var baseScene = new EditorBuildSettingsScene("Assets/UnityJapanOffice/Scenes/Base.unity", true);
            if (!ContainsScene(scenes, baseScene.path))
            {
                scenes.Insert(0, baseScene);
            }
            string[] addFiles = new string[]
            {
            "Assets/UnityJapanOffice/Scenes/EveningA.unity",
            "Assets/UnityJapanOffice/Scenes/EveningB.unity",
            "Assets/UnityJapanOffice/Scenes/NoonA.unity",
            "Assets/UnityJapanOffice/Scenes/NoonB.unity",
            "Assets/UnityJapanOffice/Scenes/NightA.unity",
            "Assets/UnityJapanOffice/Scenes/NightB.unity",
            };
            foreach (var file in addFiles)
            {
                if (!ContainsScene(scenes, file))
                {
                    var addScene = new EditorBuildSettingsScene(file, true);
                    scenes.Add(addScene);
                }
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        private static bool ContainsScene(List<EditorBuildSettingsScene> scenes, string path)
        {
            foreach (var scene in scenes)
            {
                if (scene.path == path)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

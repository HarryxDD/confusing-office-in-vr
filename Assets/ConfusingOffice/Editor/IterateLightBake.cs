using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement ;

namespace UnityJapanOffice
{
    public class IterateLightBake
    {
        private List<string> targetScenes;
        private string lightMapAssetPath;
        private int index = 0;
        private int sceneProgressId = 0;
        private LightingSettings lightSettings;


        public IterateLightBake(string lightMapData, List<string> scenes)
        {
            targetScenes = scenes;
            lightMapAssetPath = lightMapData;
        }

        public void Start()
        {
            this.lightSettings = AssetDatabase.LoadAssetAtPath<LightingSettings>(lightMapAssetPath); 
            if (lightSettings == null)
            {
                Debug.LogError("Cannnot find LightingDataAsset at " + lightMapAssetPath);
                return;
            }

            sceneProgressId = Progress.Start("LightBaker Scenes", null, Progress.Options.Managed);

            Progress.RegisterCancelCallback(sceneProgressId, OnCancel);
            index = 0;
            Lightmapping.bakeCompleted += OnBakeComplete;
            OpenAndSetupScene(targetScenes[index]);
            Progress.Report(sceneProgressId, (float)0.2f / (float)targetScenes.Count, targetScenes[index]);
            Lightmapping.BakeAsync();
        }


        bool OnCancel()
        {
            Lightmapping.Cancel();
            Lightmapping.bakeCompleted -= OnBakeComplete;
            return true;
        }

        private void OpenAndSetupScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Lightmapping.SetLightingSettingsForScene( scene, lightSettings);
        }

        void OnBakeComplete()
        {
            ++index;
            if (targetScenes.Count <= index)
            {
                Progress.Remove(sceneProgressId);
                Lightmapping.bakeCompleted -= OnBakeComplete;
                return;
            }
            Progress.Report(sceneProgressId, (float)index / (float)targetScenes.Count, targetScenes[index]);
            EditorSceneManager.SaveOpenScenes();

            OpenAndSetupScene(targetScenes[index]);
            Lightmapping.BakeAsync();
        }

    }
}
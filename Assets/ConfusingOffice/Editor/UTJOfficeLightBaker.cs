using System.Collections.Generic;
using UnityEditor;

namespace UnityJapanOffice
{
    public class UTJOfficeLightBaker
    {

        [MenuItem("Tools/UTJOffice/BakeScenes")]
        public static void BakeScenesFromUI()
        {
            var res = EditorUtility.DisplayDialog("Bake scenes", "Baking Light takes so much time? Do you want to bake 6 scenes?", "ok", "cancel");
            if (!res) { return; }
            BakeScenes();
        }

        public static void BakeScenes()
        {
            var basePath = "Assets/UnityJapanOffice/Scenes/";
            var list = new List<string>();
            list.Add(basePath + "/NoonA.unity");
            list.Add(basePath + "/NoonB.unity");
            list.Add(basePath + "/EveningA.unity");
            list.Add(basePath + "/EveningB.unity");
            list.Add(basePath + "/NightA.unity");
            list.Add(basePath + "/NightB.unity");

            var baker = new IterateLightBake("Assets/UnityJapanOffice/Settings/LightBakeSettings.lighting", list);
            baker.Start();
        }
    }
}
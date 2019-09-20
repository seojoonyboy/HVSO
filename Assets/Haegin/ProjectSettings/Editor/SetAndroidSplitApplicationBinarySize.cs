using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using UnityEditor.Callbacks;

public class SetAndroidSplitApplicationBinarySize : IPostGenerateGradleAndroidProject
{
    public int callbackOrder { get { return 0; } }
    public void OnPostGenerateGradleAndroidProject(string path)
    {
#if UNITY_ANDROID
        if (PlayerSettings.Android.useAPKExpansionFiles)
        {
            string size = "";
            var files = Directory.GetFiles(path, "*.main.obb");
            foreach (string name in files)
            {
                Debug.Log("OBB File : [" + name + "]");
                size = size + new FileInfo(name).Length;
            }
            string androidmanifest = File.ReadAllText(path + "/src/main/AndroidManifest.xml");
            androidmanifest = androidmanifest.Replace("HAEGIN_UNITY_SPLIT_APP_BIN_SIZE", size);
            File.WriteAllText(path + "/src/main/AndroidManifest.xml", androidmanifest);
        }
#endif
    }
}

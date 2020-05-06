using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public class EmojiFilter
{
    [PostProcessBuild]
    public static void SetEmojiFilter(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
#if UNITY_IOS
            string pbxprojPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject proj= new PBXProject();
            proj.ReadFromString(File.ReadAllText(pbxprojPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            proj.SetBuildProperty(target, "GCC_PREPROCESSOR_DEFINITIONS", "FILTER_EMOJIS_IOS_KEYBOARD=1");
            File.WriteAllText(pbxprojPath, proj.WriteToString());
#endif
        }
    }
}
using UnityEditor;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif
using System.IO;
using UnityEditor.Callbacks;
using Haegin;

public class AddAppsFlyerSettings : MonoBehaviour 
{
#if UNITY_IOS
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (ProjectSettings.useAppsFlyer && target == BuildTarget.iOS)
        {
            // Read.
            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string targetName = PBXProject.GetUnityTargetName(); // note, not "project." ...
            string targetGUID = project.TargetGuidByName(targetName);

            project.AddFrameworkToProject(targetGUID, "Security.framework", true);
            project.AddFrameworkToProject(targetGUID, "AdSupport.framework", true);
            project.AddFrameworkToProject(targetGUID, "iAd.framework", true);

            // Add `-ObjC` to "Other Linker Flags".
            project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");

            // Write.
            File.WriteAllText(projectPath, project.WriteToString());
        }
    }
#endif
}

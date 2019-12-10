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
    [PostProcessBuild(103)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {

        Debug.Log("[pathToBuiltProject] " + pathToBuiltProject);

        if (target == BuildTarget.iOS)
        {
            if(ProjectSettings.useAppsFlyer)
            {
                // Read.
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                Debug.Log("[projectPath] " + projectPath);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));
                string targetName = PBXProject.GetUnityTargetName(); // note, not "project." ...
                string targetGUID = project.TargetGuidByName(targetName);

                project.AddFrameworkToProject(targetGUID, "Security.framework", true);
                project.AddFrameworkToProject(targetGUID, "AdSupport.framework", true);
                project.AddFrameworkToProject(targetGUID, "iAd.framework", true);

                // Add `-ObjC` to "Other Linker Flags".
                project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");

                // Add `Enable Objective-C Exceptions` 
                project.SetBuildProperty(targetGUID, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

                // Write.
                File.WriteAllText(projectPath, project.WriteToString());
            }
            else
            {
                // Read.
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                Debug.Log("[projectPath] " + projectPath);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));
                string targetName = PBXProject.GetUnityTargetName(); // note, not "project." ...
                string targetGUID = project.TargetGuidByName(targetName);

                project.AddFrameworkToProject(targetGUID, "Security.framework", true);
                project.AddFrameworkToProject(targetGUID, "AdSupport.framework", true);

                // Add `-ObjC` to "Other Linker Flags".
                project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");

                // Add `Enable Objective-C Exceptions` 
                project.SetBuildProperty(targetGUID, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

                // Write.
                File.WriteAllText(projectPath, project.WriteToString());
            }
        }
    }
#endif
}

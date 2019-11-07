using UnityEditor;
using UnityEngine;
#if UNITY_IOS || UNITY_2018_3_OR_NEWER
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif
using System.IO;
using UnityEditor.Callbacks;

public class AddExtension : MonoBehaviour
{
#if UNITY_IOS || UNITY_2018_3_OR_NEWER
    [PostProcessBuild(101)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            var entitlementsFileName = PlayerSettings.iOS.applicationDisplayName + ".entitlements";
            var pathToNotificationService = "Assets/Haegin/Notification/Editor/NotificationExt";
            var notificationServicePlistPath = pathToBuiltProject + "/NotificationExt/Info.plist";

            if (Directory.Exists(pathToBuiltProject + "/NotificationExt") == false)
            {
                Directory.CreateDirectory(pathToBuiltProject + "/NotificationExt");

                var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                PBXProject proj = new PBXProject();
                proj.ReadFromFile(projPath);
                string targetGUID = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

                var notificationServiceTarget = PBXProjectExtensions.AddAppExtension(proj, targetGUID, "NotificationExt", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) + ".NotificationExt", notificationServicePlistPath);
                proj.AddFileToBuild(notificationServiceTarget, proj.AddFile("NotificationExt/NotificationService.h", "NotificationExt/NotificationService.h"));
                proj.AddFileToBuild(notificationServiceTarget, proj.AddFile("NotificationExt/NotificationService.m", "NotificationExt/NotificationService.m"));
                proj.AddFileToBuild(notificationServiceTarget, proj.AddFile("NotificationExt/Info.plist", "NotificationExt/Info.plist"));
                proj.AddFrameworkToProject(notificationServiceTarget, "NotificationCenter.framework", true);
                proj.AddFrameworkToProject(notificationServiceTarget, "UserNotifications.framework", true);
                proj.SetBuildProperty(notificationServiceTarget, "ARCHS", "$(ARCHS_STANDARD)");

                if(PlayerSettings.iOS.appleEnableAutomaticSigning)
                    proj.SetBuildProperty(notificationServiceTarget, "CODE_SIGN_STYLE", "Automatic");
                else
                    proj.SetBuildProperty(notificationServiceTarget, "CODE_SIGN_STYLE", "Manual");

                proj.SetBuildProperty(notificationServiceTarget, "DEVELOPMENT_TEAM", PlayerSettings.iOS.appleDeveloperTeamID);
                proj.SetBuildProperty(notificationServiceTarget, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");
                proj.WriteToFile(projPath);

                if (File.Exists(pathToBuiltProject + "/NotificationExt/NotificationService.h"))
                {
                    File.Delete(pathToBuiltProject + "/NotificationExt/NotificationService.h");
                }
                File.Copy(pathToNotificationService + "/NotificationService.h", pathToBuiltProject + "/NotificationExt/NotificationService.h");

                if (File.Exists(pathToBuiltProject + "/NotificationExt/NotificationService.m"))
                {
                    File.Delete(pathToBuiltProject + "/NotificationExt/NotificationService.m");
                }
                File.Copy(pathToNotificationService + "/NotificationService.m", pathToBuiltProject + "/NotificationExt/NotificationService.m");

                if (File.Exists(pathToBuiltProject + "/NotificationExt/Info.plist"))
                {
                    File.Delete(pathToBuiltProject + "/NotificationExt/Info.plist");
                }
                File.Copy(pathToNotificationService + "/Info.plist", pathToBuiltProject + "/NotificationExt/Info.plist");

                ProjectCapabilityManager manager = new ProjectCapabilityManager(projPath, entitlementsFileName, PBXProject.GetUnityTargetName());
                manager.AddPushNotifications(Debug.isDebugBuild);
                manager.AddInAppPurchase();
                manager.AddGameCenter();
                if(!string.IsNullOrEmpty(Haegin.ProjectSettings.firebaseDynamicLink)) {
                    manager.AddAssociatedDomains(new string[] { "applinks:" + Haegin.ProjectSettings.firebaseDynamicLink });
                }
                manager.WriteToFile();
            }
            else
            {
                var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                ProjectCapabilityManager manager = new ProjectCapabilityManager(projPath, entitlementsFileName, PBXProject.GetUnityTargetName());
                manager.AddPushNotifications(Debug.isDebugBuild);
                manager.AddInAppPurchase();
                manager.AddGameCenter();
                if(!string.IsNullOrEmpty(Haegin.ProjectSettings.firebaseDynamicLink)) {
                    manager.AddAssociatedDomains(new string[] { "applinks:" + Haegin.ProjectSettings.firebaseDynamicLink });
                }
                manager.WriteToFile();
            }


            // NotificationExt Info.plist
            // Read plist
            var plistPath = Path.Combine(pathToBuiltProject, "NotificationExt/Info.plist");

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Update value
            PlistElementDict rootDict = plist.root;
            rootDict.SetString("CFBundleShortVersionString", "" + PlayerSettings.bundleVersion);
            rootDict.SetString("CFBundleVersion", "" + PlayerSettings.iOS.buildNumber);

            // Write plist
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
#endif
}

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using UnityEngine.AssetGraph;

class ModuleBuildScript
{
    private static EditorBuildSettingsScene[] SCENES = null;
    private static string TARGET_DIR = "Build";
    private static DateTime time = DateTime.Now;

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log("arg:" + i + "=" + args[i]);
            if (args[i].Equals(name) && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    static void EnableOneStore()
	{
		ProjectSettingsWindow.SetOneStoreSettings(true);
	}

	static void DisableOneStore()
	{
		ProjectSettingsWindow.SetOneStoreSettings(false);
	}

    static void BuildAndroid(int versionCode)
    {
        PlayerSettings.Android.useAPKExpansionFiles = false;
        SCENES = FindBuildScenes();

        PlayerSettings.bundleVersion = string.Format("0.2.{0}", time.ToString("MMddHHmm"));//string.Format("{0}.{1}.{2}.{3}", GameConfig.clientVersion[0], GameConfig.clientVersion[1], GameConfig.clientVersion[2], GameConfig.clientVersion[3]);
        PlayerSettings.Android.bundleVersionCode = versionCode;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
        PlayerSettings.Android.keystoreName = "/Volumes/Data/fbl_haegin/hvso.keystore";
        PlayerSettings.Android.keystorePass = "Fbl1324$";
        PlayerSettings.Android.keyaliasName = "hvso";
        PlayerSettings.Android.keyaliasPass = "Fbl1324$";
        Debug.Log(GetArg("-exportPath"));

        GenericBuild(SCENES, GetArg("-exportPath"), BuildTarget.Android, BuildOptions.CompressWithLz4HC);
    }

    static void BuildiOS(int versionCode)
    {
        PlayerSettings.Android.useAPKExpansionFiles = false;
        SCENES = FindBuildScenes();

        PlayerSettings.iOS.buildNumber = versionCode.ToString();
        PlayerSettings.iOS.hideHomeButton = false;
        PlayerSettings.bundleVersion = string.Format("0.2.{0}", time.ToString("MMddHHmm"));

        GenericBuild(SCENES, TARGET_DIR + "/XCode", BuildTarget.iOS, BuildOptions.CompressWithLz4HC);
    }

    static void PerformOneStoreAndroidBuild()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "ODIN_INSPECTOR;ENABLE_LOG;MDEBUG;STORE_KIT_API_ENABLED;APP_DELEGATE_ENABLED;GAME_KIT_API_ENABLED;USER_NOTIFICATIONS_API_ENABLED;USE_MAINTENANCESERVER_V2;AN_FIREBASE_ANALYTICS;AN_FIREBASE_MESSAGING;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.hvso.onestore";
        ProjectSettingsWindow.SetOneStoreSettings(true);
        BuildAndroid(1);
    }

    static void PerformAndroidBuild()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "ODIN_INSPECTOR;ENABLE_LOG;MDEBUG;STORE_KIT_API_ENABLED;APP_DELEGATE_ENABLED;GAME_KIT_API_ENABLED;USER_NOTIFICATIONS_API_ENABLED;USE_MAINTENANCESERVER_V2;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.hvso";
        ProjectSettingsWindow.SetOneStoreSettings(false);
        BuildAndroid(1);
    }

    static void PerformiOSBuild()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "ODIN_INSPECTOR;ENABLE_LOG;MDEBUG;STORE_KIT_API_ENABLED;APP_DELEGATE_ENABLED;GAME_KIT_API_ENABLED;USER_NOTIFICATIONS_API_ENABLED;USE_MAINTENANCESERVER_V2;AN_FIREBASE_ANALYTICS;AN_FIREBASE_MESSAGING;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.hvso";
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        ProjectSettingsWindow.SetOneStoreSettings(false);
        BuildiOS(1);
    }

    static void PerformiOSSimulatorBuild()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "ODIN_INSPECTOR;ENABLE_LOG;MDEBUG;STORE_KIT_API_ENABLED;APP_DELEGATE_ENABLED;GAME_KIT_API_ENABLED;USER_NOTIFICATIONS_API_ENABLED;USE_MAINTENANCESERVER_V2;AN_FIREBASE_ANALYTICS;AN_FIREBASE_MESSAGING");
        PlayerSettings.applicationIdentifier = "com.haegin.hvso";
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        ProjectSettingsWindow.SetOneStoreSettings(false);
        BuildiOS(1);
    }

    private static EditorBuildSettingsScene[] FindBuildAllScenes()
    {
        return EditorBuildSettings.scenes;
    }

    private static EditorBuildSettingsScene[] FindBuildScenes()
    {
        EditorBuildSettingsScene[] sceneList = new EditorBuildSettingsScene[7];
        sceneList[0] = new EditorBuildSettingsScene("Assets/Scenes/Login.unity", true);
        sceneList[1] = new EditorBuildSettingsScene("Assets/Scenes/MenuScene.unity", true);
        sceneList[2] = new EditorBuildSettingsScene("Assets/Scenes/LoadingScene.unity", true);
        sceneList[3] = new EditorBuildSettingsScene("Assets/Scenes/BatttleConnectScene.unity", true);
        sceneList[4] = new EditorBuildSettingsScene("Assets/Scenes/IngameScene.unity", true);
        sceneList[5] = new EditorBuildSettingsScene("Assets/Scenes/TutorialScene.unity", true);
        sceneList[6] = new EditorBuildSettingsScene("Assets/Scenes/DictionaryScene.unity", true);
        return sceneList;
    }

    static void GenericBuild(EditorBuildSettingsScene[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
    {

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, build_target);

        UnityEditor.Build.Reporting.BuildReport res = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);


        if (res.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new Exception("BuildPlayer failure: " + res.summary.result.ToString());
        }
    }
}
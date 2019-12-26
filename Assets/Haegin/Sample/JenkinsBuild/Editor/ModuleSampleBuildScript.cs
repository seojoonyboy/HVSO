using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using UnityEngine.AssetGraph;

class ModuleSampleBuildScript
{
    private static EditorBuildSettingsScene[] SCENES = null;
    private static string TARGET_DIR = "Build";

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
        PlayerSettings.Android.useAPKExpansionFiles = true;
        SCENES = FindBuildScenes();

        PlayerSettings.bundleVersion = "1.0";//string.Format("{0}.{1}.{2}.{3}", GameConfig.clientVersion[0], GameConfig.clientVersion[1], GameConfig.clientVersion[2], GameConfig.clientVersion[3]);
        PlayerSettings.Android.bundleVersionCode = versionCode;
        PlayerSettings.Android.keystoreName = "user.keystore";
        PlayerSettings.Android.keystorePass = "haegin";
        PlayerSettings.Android.keyaliasName = "user";
        PlayerSettings.Android.keyaliasPass = "haegin";
        Debug.Log(GetArg("-exportPath"));

        GenericBuild(SCENES, GetArg("-exportPath"), BuildTarget.Android, BuildOptions.None);
    }

    static void BuildiOS(int versionCode)
    {
        PlayerSettings.Android.useAPKExpansionFiles = false;
        SCENES = FindBuildScenes();

        PlayerSettings.iOS.buildNumber = versionCode.ToString();
        PlayerSettings.iOS.hideHomeButton = false;
        PlayerSettings.bundleVersion = "1.0";

        GenericBuild(SCENES, TARGET_DIR + "/XCode", BuildTarget.iOS, BuildOptions.None);
    }

    static void PerformOneStoreAndroidBuild()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "CUSTOM_NGUI;CROSS_PLATFORM_INPUT;MOBILE_INPUT;MDEBUG;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.modulesample.onestore";
        ProjectSettingsWindow.SetOneStoreSettings(true);
        BuildAndroid(1);
    }

    static void PerformAndroidAppBundleBuild()
    {
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "CUSTOM_NGUI;CROSS_PLATFORM_INPUT;MOBILE_INPUT;MDEBUG;QA;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.modulesample";
        ProjectSettingsWindow.SetOneStoreSettings(false);
        BuildAndroid(1);
    }

    static void PerformAndroidBuild()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "CUSTOM_NGUI;CROSS_PLATFORM_INPUT;MOBILE_INPUT;MDEBUG;QA;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.modulesample";
        ProjectSettingsWindow.SetOneStoreSettings(false);
        BuildAndroid(1);
    }

    static void PerformiOSBuild()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "CUSTOM_NGUI;CROSS_PLATFORM_INPUT;MOBILE_INPUT;MDEBUG;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.modulesample";
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        ProjectSettingsWindow.SetOneStoreSettings(false);
        BuildiOS(1);
    }

    static void PerformiOSSimulatorBuild()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "CUSTOM_NGUI;CROSS_PLATFORM_INPUT;MOBILE_INPUT;MDEBUG;DO_NOT_USE_GPRESTO;USE_SAMPLE_SCENE");
        PlayerSettings.applicationIdentifier = "com.haegin.modulesample";
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
        EditorBuildSettingsScene[] sceneList = new EditorBuildSettingsScene[5];
        sceneList[0] = new EditorBuildSettingsScene("Assets/Haegin/Sample/Scenes/SceneOBBCheck.unity", true);
        sceneList[1] = new EditorBuildSettingsScene("Assets/Haegin/Sample/Scenes/SceneStart.unity", true);
        sceneList[2] = new EditorBuildSettingsScene("Assets/Haegin/Sample/Scenes/SceneLogin.unity", true);
        sceneList[3] = new EditorBuildSettingsScene("Assets/Haegin/Sample/Scenes/SceneGameService.unity", true);
        sceneList[4] = new EditorBuildSettingsScene("Assets/Haegin/Sample/Scenes/SceneStore.unity", true);
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
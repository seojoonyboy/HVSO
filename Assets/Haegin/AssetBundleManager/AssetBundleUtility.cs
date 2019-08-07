using UnityEngine;
#if UNITY_EDITOR	
using UnityEditor;
#endif

namespace Haegin
{
    public class AssetBundleUtility
    {
        public const string AssetBundlesOutputPath = "AssetBundles";

        public static string GetPlatformName()
        {
#if UNITY_IOS
            return "iOS";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_STANDALONE
            switch(Application.platform) 
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";
                default:
                    return "General";
            }
#else
            return "General";
#endif

            //#if UNITY_EDITOR
            //            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
            //#else
            //            return GetPlatformForAssetBundles(Application.platform);
            //#endif
        }

        public static string GetAssetBundlesPath()
        {            
#if UNITY_IOS
            return Application.temporaryCachePath + "/AssetBundles/iOS/";
#elif UNITY_ANDROID
            return Application.persistentDataPath + "/AssetBundles/Android/";
#elif UNITY_STANDALONE
            return Application.dataPath + "/AssetBundles/"+ GetPlatformName() +"/";
#else
            return Application.dataPath + "/AssetBundles/";
#endif
        }

#if UNITY_EDITOR
		private static string GetPlatformForAssetBundles(BuildTarget target)
		{
			switch(target)
			{
			case BuildTarget.Android:
				return "Android";
			case BuildTarget.iOS:
				return "iOS";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return "Windows";
			case BuildTarget.StandaloneOSX:
                return "OSX";
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneLinux64:
                return "Linux";
			default:
				return null;
			}
		}
#endif
	
		private static string GetPlatformForAssetBundles(RuntimePlatform platform)
		{
			switch(platform)
			{
			    case RuntimePlatform.Android:
				    return "Android";
                case RuntimePlatform.IPhonePlayer:
				    return "iOS";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";
                default:
                    return null;
			}
		}
	}
}
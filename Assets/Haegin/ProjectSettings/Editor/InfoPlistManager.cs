using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using System.Collections.Generic;

public class InfoPlistManager : MonoBehaviour
{
#if UNITY_IOS
    [PostProcessBuild(102)]
    static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        // Read plist
        var plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // Update value
        PlistElementDict rootDict = plist.root;
        rootDict.SetString("NSPhotoLibraryUsageDescription", "To allow users to attach the related images when they create crash feedbacks.");

        rootDict.SetString("NSCameraUsageDescription", "The camera on this device may be used for chatting.");
        rootDict.SetString("NSMicrophoneUsageDescription", "The microphone of this device may be used for chatting.");
        rootDict.SetString("NSPhotoLibraryAddUsageDescription", "Access to the Photo Library of this device may be required in order to save screenshots.");

        rootDict.SetString("NSLocationAlwaysUsageDescription", "We may access your location information for improved ad experience.");
        rootDict.SetString("NSLocationWhenInUseUsageDescription", "We may access your location information for improved ad experience.");

        // Google Ads 
        // This step is required as of Google Mobile Ads SDK version 7.42.0. Failure to add add this Info.plist entry results in a crash with the message: "The Google Mobile Ads SDK was initialized incorrectly."
        if(Haegin.ProjectSettings.UseIOSGoogleMobileAds7_24_0_OR_HIGHER)
            rootDict.SetBoolean("GADIsAdManagerApp", true);

        // UIApplicationExitsOnSuspend 제거
        if(rootDict.values.ContainsKey("UIApplicationExitsOnSuspend"))
        {
            rootDict.values.Remove("UIApplicationExitsOnSuspend");
        }

        // NSAllowsArbitraryLoadsInWebContent 제거
        if (rootDict.values.ContainsKey("NSAppTransportSecurity"))
        {
            Debug.Log("Delete NSAppTransportSecurity");
            rootDict.values.Remove("NSAppTransportSecurity");
        }
        Debug.Log("Create NSAppTransportSecurity");
        PlistElementDict ATSDict = rootDict.CreateDict("NSAppTransportSecurity");
        ATSDict.SetBoolean("NSAllowsArbitraryLoads", true);

        // Write plist
        File.WriteAllText(plistPath, plist.WriteToString());
    }
#endif
}
#define USER_NOTIFICATIONS_API_ENABLED

////////////////////////////////////////////////////////////////////////////////
//
// @module IOS Native Plugin
// @author Osipov Stanislav (Stan's Assets)
// @support support@stansassets.com
// @website https://stansassets.com
//
////////////////////////////////////////////////////////////////////////////////


#import <Foundation/Foundation.h>

#import "UnityAppController.h"   //our link to the base class.
#import "ISN_Foundation.h"
#import "ISN_UICommunication.h"

#ifdef USER_NOTIFICATIONS_API_ENABLED
#import "ISN_UNUserNotificationCenter.h"
#endif

#import <FBSDKCoreKit/FBSDKCoreKit.h>

#import "Firebase.h"

@interface ISN_UIApplicationDelegate : UnityAppController  //extend from UnityAppController.

@end


static NSString* urlscheme = nullptr;

@implementation ISN_UIApplicationDelegate(urlscheme)

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [FIRApp configure];

#if !TARGET_OS_TV
#ifdef USER_NOTIFICATIONS_API_ENABLED
    [[ISN_UNUserNotificationCenter sharedInstance] addDelegate];
#endif
#endif

    return [super application:application didFinishLaunchingWithOptions:launchOptions];
}


- (BOOL) application:(UIApplication *)application handleOpenURL:(NSURL *)url
{
    urlscheme = url.absoluteString;
//    UnitySendMessage("HaeginURLSchemeListener", "NativeOpenURL", [url.absoluteString UTF8String]);
    return true;
}

- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
    urlscheme = url.absoluteString;
//    UnitySendMessage("HaeginURLSchemeListener", "NativeOpenURL", [url.absoluteString UTF8String]);
    return [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
}

#if !TARGET_OS_TV
- (BOOL)application:(UIApplication*)application openURL:(nonnull NSURL *)url options:(nonnull NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
    urlscheme = url.absoluteString;
    return [[FBSDKApplicationDelegate sharedInstance] application:application
                                                          openURL:url
                                                sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey]
                                                       annotation:options[UIApplicationOpenURLOptionsAnnotationKey]
            ];
//    return true;
}
#endif

@end

extern "C" {
    void NativeClearURLScheme() {
        urlscheme = nullptr;
    }
    bool NativeCheckURLScheme()  {
        if(urlscheme != nullptr) {
            UnitySendMessage("HaeginURLSchemeListener", "NativeOpenURL", [urlscheme UTF8String]);
            return true;
        }
        return false;
    }
}

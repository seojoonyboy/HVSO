//
//  BGWebClient_Controller.h
//  Unity-iPhone
//
//  Created by Hoyun Shin on 28/06/2018.
//

#ifndef BGWebClient_Controller_h
#define BGWebClient_Controller_h

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

@interface ISN_UIApplicationDelegate : UnityAppController  //extend from UnityAppController.

@end

@interface ISN_UIApplicationDelegate(BGWebClient)

//method declarations here

@end
#endif /* BGWebClient_Controller_h */

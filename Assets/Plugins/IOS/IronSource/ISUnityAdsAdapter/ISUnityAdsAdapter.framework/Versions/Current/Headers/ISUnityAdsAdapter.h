//
//  Copyright (c) 2015 IronSource. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "IronSource/ISBaseAdapter+Internal.h"

static NSString * const UnityAdsAdapterVersion     = @"4.1.6";
static NSString *  GitHash = @"61363f02a";

//System Frameworks For UnityAds Adapter

@import AdSupport;
@import StoreKit;
@import CoreTelephony;

@interface ISUnityAdsAdapter : ISBaseAdapter

@end

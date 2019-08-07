//
//  BGWebClient.h
//  BGWebClientTest
//
//  Created by Hoyun Shin on 29/11/2017.
//  Copyright © 2017 HAEGIN Co.,Ltd. All rights reserved.
//

#ifndef BGWebClient_h
#define BGWebClient_h
#import <Foundation/Foundation.h>

@interface BGWebClient : NSObject
+(BGWebClient*)instance;
-(BOOL)hasDownloadTask;
-(void)startDownload:(NSString*)requestURL path:(NSString*)path;
-(void)stopDownload:(NSString*)requestURL;
@property (copy) void (^backgroundSessionCompletionHandler)();
@end

#endif /* BGWebClient_h */

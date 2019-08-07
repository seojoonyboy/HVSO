//
//  BGWebClient.m
//  BGWebClientTest
//
//  Created by Hoyun Shin on 29/11/2017.
//  Copyright © 2017 HAEGIN Co.,Ltd. All rights reserved.
//
#import "BGWebClient.h"

NSString* ToString(const char* c_string)
{
    return c_string == NULL ? [NSString stringWithUTF8String:""] : [NSString stringWithUTF8String:c_string];
}

char* MakeStringCopy(const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

extern "C"
{
    void StartDownload(const char* url, const char* dest) {
        
        [[BGWebClient instance] startDownload:ToString(url) path:ToString(dest)];
    }   
    
    void StopDownload(const char* url) {
        [[BGWebClient instance] stopDownload:ToString(url)];
    } 
}

@interface BGWebClient () <NSURLSessionDelegate, NSURLSessionTaskDelegate, NSURLSessionDownloadDelegate> {
}
@property (nonatomic, readwrite) NSURLSession *session;
@end

@implementation BGWebClient

+ (BGWebClient*)instance
{
    static BGWebClient* instance = nil;
    if (!instance)
        instance = [[BGWebClient alloc] init];

    return instance;
}

- (id)init {
    if((self = [super init])) {
        self.session = nil;
        [self getURLSession];
    }
    return self;
}

-(void)startDownload:(NSString*)requestURL path:(NSString*)path {
    // requestURL을 다운로드 작업을 큐에 넣는다.
    [[self getURLSession] getTasksWithCompletionHandler:^(NSArray<NSURLSessionDataTask *> * _Nonnull dataTasks, NSArray<NSURLSessionUploadTask *> * _Nonnull uploadTasks, NSArray<NSURLSessionDownloadTask *> * _Nonnull downloadTasks) {
        for (NSURLSessionDownloadTask *task in downloadTasks) {
            if([[task.originalRequest.URL absoluteString] isEqualToString:requestURL]) {
                if(task.taskDescription != nil && [task.taskDescription isEqualToString:path]) {
                    [task resume];
                    return;
                }
                else {
                    task.taskDescription = nil;
                    [task cancel];
                }
            }
        }

        NSURL *url = [NSURL URLWithString:requestURL];
        NSURLRequest *req = [NSURLRequest requestWithURL:url];
        NSURLSessionDownloadTask *downloadTask = [[self getURLSession] downloadTaskWithRequest:req];
        downloadTask.taskDescription = path;
        [downloadTask resume];
    }];
}

-(void)stopDownload:(NSString*)requestURL {
    // requestURL의 다운로드 작업을 중지.
    [[self getURLSession] getTasksWithCompletionHandler:^(NSArray<NSURLSessionDataTask *> * _Nonnull dataTasks, NSArray<NSURLSessionUploadTask *> * _Nonnull uploadTasks, NSArray<NSURLSessionDownloadTask *> * _Nonnull downloadTasks) {
        for (NSURLSessionDownloadTask *task in downloadTasks) {
            if([[task.originalRequest.URL absoluteString] isEqualToString:requestURL]) {
                task.taskDescription = nil;
                [task cancel];
            }
        }
    }];
}

// 세션 관리
-(NSURLSession*)getURLSession {
    if(!self.session) {
        NSString *identifier = [@"_BGWebClient_" stringByAppendingString:[[NSBundle mainBundle] bundleIdentifier]];
        static dispatch_once_t onceToken;
        dispatch_once(&onceToken, ^{
            NSURLSessionConfiguration *config = [NSURLSessionConfiguration backgroundSessionConfigurationWithIdentifier:identifier];
            self.session = [NSURLSession sessionWithConfiguration:config delegate:self delegateQueue:[NSOperationQueue mainQueue]];
        });
    }
    return self.session;
}

#pragma mark -- NSURLSessionDelegate --
- (void)URLSessionDidFinishEventsForBackgroundURLSession:(NSURLSession *)session {
    if (_backgroundSessionCompletionHandler) {
        NSLog(@"--------------------------------");
        NSLog(@"-----------URLSessionDidFinishEventsForBackgroundURLSession--------------------");
        NSLog(@"--------------------------------");
        void (^completionHandler)() = _backgroundSessionCompletionHandler;
        _backgroundSessionCompletionHandler = nil;
        completionHandler();
    }
}

#pragma mark -- NSURLSessionTaskDelegate --
- (void)URLSession:(NSURLSession *)session task:(NSURLSessionTask *)task didCompleteWithError:(NSError *)error {
//    NSLog(@"%@, error.code=%d", error, (int)(error.code));
    if (session == self.session && error != nil) {
        if(error.code == NSURLErrorCancelled) {
            NSString* param = [NSString stringWithFormat:@"%@,1", [task.originalRequest.URL absoluteString]];
            UnitySendMessage("BGWebClient", "NativeAsyncCompleted", MakeStringCopy([param UTF8String]));
        }
        else {
            NSString* param = [NSString stringWithFormat:@"%@,2", [task.originalRequest.URL absoluteString]];
            UnitySendMessage("BGWebClient", "NativeAsyncCompleted", MakeStringCopy([param UTF8String]));
        }
    }
}

#pragma mark -- NSURLSessionDownloadDelegate --

- (void)URLSession:(NSURLSession *)session downloadTask:(NSURLSessionDownloadTask *)downloadTask didFinishDownloadingToURL:(NSURL *)location {
    NSString *requestURL = [downloadTask.originalRequest.URL absoluteString];
    NSString *destPath = downloadTask.taskDescription;
    NSData* data = [NSData dataWithContentsOfURL:location];
    
    NSLog(requestURL);
    
    if (!destPath) {
        //  destPath가 없으면, Cancel 된 녀석으로 볼 수 있음. 무시하자.
        return;
    }
    
    NSURL *destURL = [NSURL fileURLWithPath:destPath];
    NSFileManager *fileManager = [NSFileManager defaultManager];
    NSError *errorCopy;
    [fileManager removeItemAtURL:destURL error:nil];
    
    if([fileManager copyItemAtURL:location toURL:destURL error:&errorCopy]) {
        NSString* param = [NSString stringWithFormat:@"%@,0", [downloadTask.originalRequest.URL absoluteString]];
        UnitySendMessage("BGWebClient", "NativeAsyncCompleted", MakeStringCopy([param UTF8String]));
    }
    else {
        NSString* param = [NSString stringWithFormat:@"%@,2", [downloadTask.originalRequest.URL absoluteString]];
        UnitySendMessage("BGWebClient", "NativeAsyncCompleted", MakeStringCopy([param UTF8String]));
        return;
    }
}

- (void)URLSession:(NSURLSession *)session downloadTask:(NSURLSessionDownloadTask *)downloadTask didWriteData:(int64_t)bytesWritten totalBytesWritten:(int64_t)totalBytesWritten totalBytesExpectedToWrite:(int64_t)totalBytesExpectedToWrite {
    NSString* param = [NSString stringWithFormat:@"%@,%d,%d", [downloadTask.originalRequest.URL absoluteString], totalBytesWritten, totalBytesExpectedToWrite];
    UnitySendMessage("BGWebClient", "NativeDownloadProgressChanged", MakeStringCopy([param UTF8String]));
}

- (void)URLSession:(NSURLSession *)session downloadTask:(NSURLSessionDownloadTask *)downloadTask didResumeAtOffset:(int64_t)fileOffset expectedTotalBytes:(int64_t)expectedTotalBytes {
}

@end

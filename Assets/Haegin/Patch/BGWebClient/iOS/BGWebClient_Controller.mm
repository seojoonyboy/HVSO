#import "BGWebClient_Controller.h"
#import "BGWebClient.h"

@implementation ISN_UIApplicationDelegate(BGWebClient)

- (void)application:(UIApplication *)application handleEventsForBackgroundURLSession:(NSString *)identifier
  completionHandler:(void (^)())completionHandler
{
    /*
     Store the completion handler. The completion handler is invoked by the view controller's checkForAllDownloadsHavingCompleted method (if all the download tasks have been completed).
     */
    printf("                         handleEventsForBackgroundURLSession\n");
    [BGWebClient instance].backgroundSessionCompletionHandler = completionHandler;
}

@end

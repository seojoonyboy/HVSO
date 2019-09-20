//
//  NotificationService.m
//  NotificationExt
//
//  Created by Hoyun Shin on 22/02/2018.
//

#import "NotificationService.h"

@interface NotificationService ()

@property (nonatomic, strong) void (^contentHandler)(UNNotificationContent *contentToDeliver);
@property (nonatomic, strong) UNMutableNotificationContent *bestAttemptContent;

@end

@implementation NotificationService

- (void)didReceiveNotificationRequest:(UNNotificationRequest *)request withContentHandler:(void (^)(UNNotificationContent * _Nonnull))contentHandler {
    self.contentHandler = contentHandler;
    self.bestAttemptContent = [request.content mutableCopy];

    [[UNUserNotificationCenter currentNotificationCenter] getDeliveredNotificationsWithCompletionHandler: ^(NSArray<UNNotification *> * _Nonnull notifications) {
        self.bestAttemptContent.badge = [NSNumber numberWithInteger:notifications.count + 1];
        self.contentHandler(self.bestAttemptContent);
    }];
}

- (void)serviceExtensionTimeWillExpire {
    // Called just before the extension will be terminated by the system.
    // Use this as an opportunity to deliver your "best attempt" at modified content, otherwise the original push payload will be used.
    self.contentHandler(self.bestAttemptContent);
}

@end

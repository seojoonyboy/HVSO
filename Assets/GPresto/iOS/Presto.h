//
//  Presto.h
//  Presto
//
//  Created by Mocker on 2014. 12. 2..
//  Copyright (c) 2014년 Largosoft. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <SystemConfiguration/SystemConfiguration.h>

#import "PrestoADView.h"

#include <CommonCrypto/CommonCrypto.h>

@interface Presto : NSObject
<UIAlertViewDelegate> {
    NSInputStream *iStream;
    NSOutputStream *oStream;
}
	
@property (nonatomic, retain) NSInputStream *iStream;
@property (nonatomic, retain) NSOutputStream *oStream;

- (BOOL)Presto_init;
- (BOOL)Presto_init:(UIView *)parentsView;
- (NSString *)Presto_getsDataNS;

@end

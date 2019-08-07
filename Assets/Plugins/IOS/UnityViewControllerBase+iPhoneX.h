#pragma once

#import "UnityViewControllerBase.h"

@interface UnityViewControllerBase (iPhoneX)
- (BOOL)prefersHomeIndicatorAutoHidden; // add this
- (UIRectEdge)preferredScreenEdgesDeferringSystemGestures;

@end

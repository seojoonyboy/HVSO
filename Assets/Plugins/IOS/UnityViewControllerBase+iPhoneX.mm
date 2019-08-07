#import "UnityViewControllerBase+iPhoneX.h"

@implementation UnityViewControllerBase(iPhoneX)

// add this
- (BOOL)prefersHomeIndicatorAutoHidden
{
    return NO;
}

-(UIRectEdge)preferredScreenEdgesDeferringSystemGestures
{
    return UIRectEdgeAll;
}

@end

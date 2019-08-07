#import<CoreTelephony/CTCallCenter.h>    
#import<CoreTelephony/CTCall.h>   
#import<CoreTelephony/CTCarrier.h>    
#import<CoreTelephony/CTTelephonyNetworkInfo.h>

extern char* MakeStringCopy(const char* string);

extern "C"
{
    const char* GetNetworkOperatorName() {
        CTTelephonyNetworkInfo *myNetworkInfo = [[CTTelephonyNetworkInfo alloc] init];
        CTCarrier *myCarrier = [myNetworkInfo subscriberCellularProvider];
        if(myCarrier == nil || [myCarrier carrierName] == nil)
            return MakeStringCopy("");
        else
            return MakeStringCopy([[myCarrier carrierName] UTF8String]);
    }
}

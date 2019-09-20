#import "ISN_SKCommunication.h"
#import "ISN_Logger.h"
#import "ISN_SKTransactionObserver.h"
#import "ISN_SKProductsRequestDelegate.h"
#import <StoreKit/StoreKit.h>



@interface SA_PluginSettingsWindowStylesAppManager : NSObject
@property (nonatomic, strong) ISN_SKTransactionObserver* transactionObserver;
@property (nonatomic, strong) ISN_SKProductsRequestDelegate* productsRequestDelegate;
@end


@interface SA_PluginSettingsWindowStylesAppManager(Haegin)
@end


@implementation SA_PluginSettingsWindowStylesAppManager(Haegin)


-(void) addPayment:(NSString*) productsIdentifier {
    if([productsIdentifier length] == 0)
    {
        for (SKPaymentTransaction *transaction in [SKPaymentQueue defaultQueue].transactions) {
            if(transaction.transactionIdentifier != NULL) {
                [self.transactionObserver.transactions setObject:transaction forKey:transaction.transactionIdentifier];
            }
        }
        int count = 0;
        for (SKPaymentTransaction *transaction in [SKPaymentQueue defaultQueue].transactions) {
            if(transaction.transactionState == SKPaymentTransactionStatePurchased && [[self.productsRequestDelegate products] objectForKey:transaction.payment.productIdentifier] != NULL) {
                ISN_SKPaymentTransaction *transactionResult =  [[ISN_SKPaymentTransaction alloc] initWithSKPaymentTransaction:transaction];
                [self.transactionObserver reportTransaction:transactionResult];
                count++;
            }
        }
        if(count == 0) {
            ISN_SKPaymentTransaction *transactionResult =  [[ISN_SKPaymentTransaction alloc] init];
            transactionResult.m_error = NULL;
            transactionResult.m_originalTransaction = NULL;
            transactionResult.m_unitxDate = 0;
            transactionResult.m_state = SKPaymentTransactionStateFailed;
            transactionResult.m_transactionIdentifier = @"";
            transactionResult.m_productIdentifier = productsIdentifier;
            [self.transactionObserver reportTransaction:transactionResult];
        }
    }
    else
    {
        SKProduct* selectedProduct =  [[self.productsRequestDelegate products] objectForKey:productsIdentifier];
        if(selectedProduct != NULL) {
            SKPayment *payment = [SKPayment paymentWithProduct:selectedProduct];
            [[SKPaymentQueue defaultQueue] addPayment:payment];
        } else {
            SA_Error* error = [[SA_Error alloc] initWithCode:1 message:@"product not found"];
            ISN_SKPaymentTransaction * transaction = [[ISN_SKPaymentTransaction alloc] initWithError:error];
            [self.transactionObserver reportTransaction:transaction];
        }
    }
}
@end

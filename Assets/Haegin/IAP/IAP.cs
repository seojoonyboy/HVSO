using HaeginGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_STANDALONE && USE_STEAM
using Steamworks;
#elif UNITY_ANDROID
using SA.Foundation.Templates;
using SA.Android;
using SA.Android.Utilities;
using SA.Android.Vending.Billing;
using SA.Android.Vending.Licensing;
#elif UNITY_IOS
using SA.iOS.StoreKit;
using SA.Foundation.Templates;
#endif

namespace Haegin
{
#if UNITY_IOS
    public class ISN_PurchaseResult
    {
        public ISN_SKPaymentTransactionState State;
    }

    public class ISN_PaymentManager : ISN_iSKPaymentTransactionObserver
    {
        public delegate void OnTransactionComplete(ISN_SKPaymentTransactionState state, TransactionErrorCode transactionErrorCode);
        private OnTransactionComplete _transactionComplete;
        private List<ISN_SKPaymentTransaction> transactions = new List<ISN_SKPaymentTransaction>();

        private static bool IsInitialized = false;
        public void init(OnTransactionComplete callback)
        {
            _transactionComplete = callback;

            // just make sure we init only ince
            if (!IsInitialized)
            {
                ISN_SKPaymentQueue.AddTransactionObserver(this);
            }
        }

        //--------------------------------------
        //  ISN_TransactionObserver implementation
        //--------------------------------------
        public void OnTransactionUpdated(ISN_SKPaymentTransaction transaction)
        {
#if MDEBUG
            Debug.Log("transaction JSON: " + JsonUtility.ToJson(transaction));
            Debug.Log("OnTransactionComplete: " + transaction.ProductIdentifier);
            Debug.Log("OnTransactionComplete: state: " + transaction.State);
#endif
            switch (transaction.State)
            {

                case ISN_SKPaymentTransactionState.Purchasing:
                    break;
                case ISN_SKPaymentTransactionState.Purchased:
                case ISN_SKPaymentTransactionState.Restored:
                case ISN_SKPaymentTransactionState.Deferred:
                    transactions.Add(transaction);
                    _transactionComplete(transaction.State, 0);
                    break;
                case ISN_SKPaymentTransactionState.Failed:
#if MDEBUG
                    Debug.Log("Transaction failed with error, code: " + transaction.Error.Code);
                    Debug.Log("Transaction failed with error, description: " + transaction.Error.Message);
#endif
                    ISN_SKPaymentQueue.FinishTransaction(transaction);
                    _transactionComplete(transaction.State, (TransactionErrorCode)transaction.Error.Code);
                    break;
            }
        }

        public void OnTransactionRemoved(ISN_SKPaymentTransaction result)
        {
            //Your application does not typically need to anything on this event,  
            //but it may be used to update user interface to reflect that a transaction has been completed.
        }

        public bool OnShouldAddStorePayment(ISN_SKProduct result)
        {
            /// Return true to continue the transaction in your app.
            /// Return false to defer or cancel the transaction.
            /// If you return false, you can continue the transaction later using requetsed <see cref="ISN_SKProduct"/>
            /// 
            /// we are okay, to continue trsansaction, so let's return true
            return true;
        }


        public void OnRestoreTransactionsComplete(SA_Result result)
        {

            /// Tells the observer that the payment queue has finished sending restored transactions.
            /// 
            /// This method is called after all restorable transactions have been processed by the payment queue. 
            /// Your application is not required to do anything in this method.

            if (result.IsSucceeded)
            {
                Debug.Log("Restore Compleated");
            }
            else
            {
                Debug.Log("Error: " + result.Error.Code + " message: " + result.Error.Message);
            }
        }

        public void FinishTransactionById(long transactionId)
        {
            ISN_SKPaymentTransaction targetTransaction = null;
            foreach (ISN_SKPaymentTransaction t in transactions)
            {
                if (t.TransactionIdentifier.Equals(transactionId))
                {
                    targetTransaction = t;
                    break;
                }
            }
            if (targetTransaction != null)
            {
                ISN_SKPaymentQueue.FinishTransaction(targetTransaction);
                transactions.Remove(targetTransaction);
            }
            else
            {
                ISN_SKPaymentQueue.FinishTransactionById("" + transactionId);
            }
        }
    }
#endif

    public static class IAP
    {
        public enum PurchaseResultCode
        {
            PURCHASE_SUCCESS,
            PURCHASE_FAIL,
            PURCHASE_USER_CANCEL,
            PURCHASE_INVALID_RECEIPT,
            PURCHASE_DEFERRED
        };

        public delegate void OnInitialized(bool result, bool bProcessIAP, byte[] purchasedData);
        public delegate void OnPurchaseCompletion(PurchaseResultCode code, byte[] purchasedData, string err);
        public delegate void OnPurchaseRestoration(string err);

        private static OnPurchaseCompletion _purchaseCompletionAction;
        private static OnInitialized _initializedAction;
        private static OnPurchaseRestoration _purchaseRestorationAction;
#if UNITY_ANDROID
        private static bool _isInitialized = false;
        private static byte[] _purchasedData;
#endif

#if UNITY_STANDALONE && USE_STEAM
        private static CallResult<SteamInventoryDefinitionUpdate_t> m_InventoryItemDefinitionsUpdate;
        private static CallResult<SteamInventoryRequestPricesResult_t> m_InventoryRequestPriceResult;
        private static CallResult<SteamInventoryStartPurchaseResult_t> m_InventoryStartPurchaseResult;
        private static Callback<MicroTxnAuthorizationResponse_t> m_MicroTxnAuthorizationReponse;
        private static Callback<GameOverlayActivated_t> m_GameOverlayActivated;
#endif

        public static MarketType GetMarketType()
        {
#if UNITY_IOS
            return MarketType.AppleStore;
#elif UNITY_ANDROID
#if USE_ONESTORE_IAP
            return MarketType.OneStore;
#else
            return MarketType.GooglePlay;
#endif
#elif UNITY_STANDALONE && USE_STEAM
            return MarketType.Steam;
#else
            return MarketType.None;
#endif
        }

#if UNITY_IOS
        private static ISN_PaymentManager s_paymentManager = new ISN_PaymentManager();
         public static bool IsAvailableItem(string productId)
        {
            foreach (ISN_SKProduct pd in ISN_SKPaymentQueue.Products)
            {
                if (pd.ProductIdentifier.Equals(productId)) return true;
            }
            return false;
        }
#endif

#if USE_ONESTORE_IAP
#endif
        public static void init(string[] skus, OnInitialized completeHandler)
        {
            _purchaseCompletionAction = null;
            _purchaseRestorationAction = null;
            _initializedAction = completeHandler;
#if UNITY_IOS || UNITY_TVOS
            if(ISN_SKPaymentQueue.IsReady)
            {
#if PROCESS_RECEIPT_WHEN_REINIT_IOS_IAP
                ISN_SKPaymentQueue.AddPayment("");
#else                
                completeHandler(true, false, null);
                _initializedAction = null;
#endif                
                return;
            }

            SA.iOS.ISN_Settings.Instance.InAppProducts.Clear();
            for (int i = 0; i < skus.Length; i++)
            {
                ISN_SKPaymentQueue.RegisterProductId(skus[i]);
            }

            ISN_SKPaymentQueue.Init((ISN_SKInitResult result) => {
#if MDEBUG
                Debug.Log("result.Products.Count " + result.Products.Count);
                Debug.Log("result.InvalidProductIdentifiers.Count " + result.InvalidProductIdentifiers.Count);
#endif
                s_paymentManager.init(OnTransactionComplete);
                OnStoreKitInitComplete(result);
            });
#elif UNITY_ANDROID

#if USE_ONESTORE_IAP
            OneStore.IAPManager.GetInstance().ConnectService(OnBillingSetupFinished);
#else
            if (_isInitialized)
            {
                OnBillingSetupFinishedProcess();
                return;
            }

            // In-App Product Info
            var settings = AN_Settings.Instance;
            settings.Vending = true;
            settings.Licensing = false;
            settings.RSAPublicKey = ProjectSettings.base64EncodedPublicKey;
            settings.InAppProducts.Clear();
            for (int i = 0; i < skus.Length; i++)
            {
                var pd = new AN_Product(skus[i], AN_ProductType.inapp);
                settings.InAppProducts.Add(pd);
            }
            AN_Billing.Connect((result) => {
#if MDEBUG
                Debug.Log("Connect result.IsSucceeded: " + result.IsSucceeded);
                Debug.Log("Connect result.IsInAppsAPIAvalible: " + result.IsInAppsAPIAvalible);
                Debug.Log("Connect result.IsSubsAPIAvalible: " + result.IsSubsAPIAvalible);
#endif
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    OnBillingSetupFinished(result);
                });
            });
#endif
#elif UNITY_STANDALONE && USE_STEAM
            if(SteamManager.Initialized)
            {
                m_InventoryItemDefinitionsUpdate = CallResult<SteamInventoryDefinitionUpdate_t>.Create(OnInventoryDefinitionUpdate);
                m_InventoryRequestPriceResult = CallResult<SteamInventoryRequestPricesResult_t>.Create(OnSteamRequestPriceResult);
                m_InventoryStartPurchaseResult = CallResult<SteamInventoryStartPurchaseResult_t>.Create(OnSteamStartPurchaseResult);
                m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
                m_MicroTxnAuthorizationReponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnSteamMicroTxnAuthorizationResponse);
                SteamInventory.LoadItemDefinitions();
                RequestPrices();
            }
            else 
            {
                _initializedAction(false, false, null);
                _initializedAction = null;
            }
#endif
        }

#if UNITY_STANDALONE && USE_STEAM
        private static void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
        {
        }

        private static void OnSteamMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t pCallBack)
        {
            if(pCallBack.m_bAuthorized == 1)
            {
                WebClient.GetInstance().RequestSteamFinalizeTransaction((long)pCallBack.m_ulOrderID, (WebClient.ErrorCode error, bool isSucceeded, int errorCode, string errorDesc, byte[] purchasedData) =>
                {
                    if(isSucceeded) 
                    {
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_SUCCESS, purchasedData, "ok");
                    }
                    else 
                    {
#if MDEBUG
                        Debug.Log("ErrorCode = " + errorCode + "   " + errorDesc);
#endif
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, errorDesc);
                    }
                });
            }
            else 
            {
                _purchaseCompletionAction(PurchaseResultCode.PURCHASE_USER_CANCEL, null, "User Cancel - Not Authorized");
            }
        }

        static string currencyCode; 

        private static void OnSteamRequestPriceResult(SteamInventoryRequestPricesResult_t pCallBack, bool bIOFailure)
        {
            currencyCode = pCallBack.m_rgchCurrency;
            _initializedAction(true, false, null);
            _initializedAction = null;
        }

        private static void OnSteamStartPurchaseResult(SteamInventoryStartPurchaseResult_t pCallBack, bool bIOFailure)
        {
        }

        private static void OnInventoryDefinitionUpdate(SteamInventoryDefinitionUpdate_t pCallback, bool bIOFailure)
        {
        }

        private static void RequestPrices()
        {
            m_InventoryRequestPriceResult.Set(SteamInventory.RequestPrices());
        }
#endif


#if UNITY_IOS
        private static bool IsIAPInitialized = false;
        static void OnStoreKitInitComplete(ISN_SKInitResult result)
        {
            if (result.IsSucceeded)
            {
                IsIAPInitialized = true;
                ISN_SKPaymentQueue.AddPayment("");
            }
            else
            {
                _initializedAction(false, false, null);
                _initializedAction = null;
            }
        }

        static void OnTransactionComplete(ISN_SKPaymentTransactionState state, TransactionErrorCode transactionErrorCode)
        {
            if(!IsIAPInitialized) return;
            switch (state)
            {
                case ISN_SKPaymentTransactionState.Purchased:
                case ISN_SKPaymentTransactionState.Restored:
                    {
                        string ReceiptString = ISN_SKPaymentQueue.AppStoreReceipt.AsBase64String;
                        if (string.IsNullOrEmpty(ReceiptString))
                        {
                            var request = new ISN_SKReceiptRefreshRequest(null);
                            request.Start((result) => {
#if MDEBUG
                                Debug.Log("Receipt Refresh Result: " + result.IsSucceeded);
#endif
                                ReceiptString = ISN_SKPaymentQueue.AppStoreReceipt.AsBase64String;
                                WebClient.GetInstance().RequestConsumeAppleReceipt(ReceiptString, OnConsumeAppleReceiptResult);
                            });
                        }
                        else
                        {
#if MDEBUG
                            Debug.Log("----------------------------------------------");
                            Debug.Log("ReceiptString : [" + ReceiptString + "]");
                            Debug.Log("----------------------------------------------");
#endif
                            WebClient.GetInstance().RequestConsumeAppleReceipt(ReceiptString, OnConsumeAppleReceiptResult);
                        }
                    }
                    break;
                case ISN_SKPaymentTransactionState.Deferred:
                    if (_purchaseCompletionAction != null)
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_DEFERRED, null, null);
                    break;
                case ISN_SKPaymentTransactionState.Failed:
                    if (transactionErrorCode == TransactionErrorCode.SKErrorPaymentCanceled) {
                        if (_purchaseCompletionAction != null)
                            _purchaseCompletionAction(PurchaseResultCode.PURCHASE_USER_CANCEL, null, null);
                    }
                    else {
                        if (_purchaseCompletionAction != null)
                            _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, null);
                    }
                    if (_initializedAction != null)
                    {
                        _initializedAction(true, false, null);
                        _initializedAction = null;
                    }
                    break;
            }
        }

        private static string currentProductId = null;
        public static void OnConsumeAppleReceiptResult(WebClient.ErrorCode error, StoreKitTransaction transaction, byte[] purchasedData) {
#if MDEBUG
            if(transaction != null)
                Debug.Log("OnConsumeAppleReceiptResult  " + error + ", " + transaction.TransactionId + "(" + transaction.TransactionState + ", " + transaction.ProductId + "), " + purchasedData);
            else
                Debug.Log("OnConsumeAppleReceiptResult  " + error + ", null, " + purchasedData);
#endif
            bool bProcessIAP = false;

            switch (error)
            {
                case WebClient.ErrorCode.SUCCESS:
                    switch (transaction.TransactionState)
                    {
                        case TransactionState.Consumed:
                            bProcessIAP = true;
                            s_paymentManager.FinishTransactionById(transaction.TransactionId);
                            if(currentProductId != null && currentProductId.Equals(transaction.ProductId))
                            {
                                currentProductId = null;
                                if (_purchaseCompletionAction != null)
                                {
                                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_SUCCESS, purchasedData, "ok");
                                }
                                if (_purchaseRestorationAction != null)
                                {
                                    _purchaseRestorationAction(transaction.ProductId);
                                }
                            }
                            break;
                        case TransactionState.Duplicated:
                            s_paymentManager.FinishTransactionById(transaction.TransactionId);
                            if (currentProductId != null && currentProductId.Equals(transaction.ProductId))
                            {
                                currentProductId = null;
                                if (_purchaseCompletionAction != null)
                                {
                                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "verification failed(Duplicated)");
                                }
                            }
                            break;
                        case TransactionState.NotConsumed:
                            if (currentProductId != null && currentProductId.Equals(transaction.ProductId))
                            {
                                currentProductId = null;
                                if (_purchaseCompletionAction != null)
                                {
                                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "verification failed(NotConsumed)");
                                }
                            }

                            break;
                        default:
                            if (currentProductId != null && currentProductId.Equals(transaction.ProductId))
                            {
                                currentProductId = null;
                                if (_purchaseCompletionAction != null)
                                {
                                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "unknown failed");
                                }
                            }
                            break;
                    }
                    break;
                case WebClient.ErrorCode.OTHER_DEVICE_LOGIN_ERROR:
                    // ?????? ???????????? ????????? ???
                    if (currentProductId != null && currentProductId.Equals(transaction.ProductId))
                    {
                        currentProductId = null;
                        if (_purchaseCompletionAction != null)
                        {
                            _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "other device login error");
                        }
                    }
                    break;
                case WebClient.ErrorCode.RECEIPT_NEED_TO_REFRESH:
                    // ?????? ???????????? ????????? ???
                    if (currentProductId != null && currentProductId.Equals(transaction.ProductId))
                    {
                        currentProductId = null;
                        if (_purchaseCompletionAction != null)
                        {
                            _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "empty receipt");
                        }
                    }
                    break;
                case WebClient.ErrorCode.RECEIPT_VERIFICATION_FAILED:
                    // ?????? ???????????? ????????? ???
                    if (currentProductId != null && currentProductId.Equals(transaction.ProductId))
                    {
                        currentProductId = null;
                        if (_purchaseCompletionAction != null)
                        {
                            _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "verification failed");
                        }
                    }
                    break;
                default:
                    // ???????????? ??????
                    if (currentProductId != null && currentProductId.Equals(transaction.ProductId))
                    {
                        currentProductId = null;
                        if (_purchaseCompletionAction != null)
                        {
                            _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "network error");
                        }
                    }
                    break;
            }
            if (_initializedAction != null)
            {
                _initializedAction(true, bProcessIAP, purchasedData);
                _initializedAction = null;
            }
        }
#elif UNITY_ANDROID

#if USE_ONESTORE_IAP
        private static void OnGetPurchases(bool exists, OneStore.PurchaseData[] data)
        {
#if MDEBUG
            Debug.Log("callback from GetPurchases + " + exists);
#endif
            if (exists == true)
            {
                List<OneStorePurchasedProduct> list = new List<OneStorePurchasedProduct>();
                for (int i = 0; i < data.Length; i++)
                {
                    OneStorePurchasedProduct p = new OneStorePurchasedProduct();
                    p.OrderId = data[i].orderId;
                    p.PackageName = data[i].packageName;
                    p.ProductId = data[i].productId;
                    p.PurchaseTime = data[i].purchaseTime;
                    p.PurchaseId = data[i].purchaseId;
                    p.DeveloperPayload = data[i].developerPayload;
                    p.PurchaseState = data[i].purchaseState;
                    p.RecurringState = data[i].recurringState;
                    list.Add(p);
                }
                if (list.Count > 0)
                {
#if MDEBUG
                    Debug.Log("call RequestConsumeOneStoreReceipt  " + list.Count);
#endif
                    WebClient webClient = WebClient.GetInstance();
                    webClient.RequestConsumeOneStoreReceipt(list, OnConsumeOneStoreReceiptResult);
                }
                else
                {
                    _initializedAction(true, false, null);
                    _initializedAction = null;
                }
            }
            else
            {
                _initializedAction(true, false, null);
                _initializedAction = null;
            }
        }

        private static void ProcessNotConsumedProduct(bool result)
        {
#if MDEBUG
            Debug.Log("ProcessNotConsumedProduct + " + result);
#endif
            if(result)
            {
                OneStore.IAPManager.GetInstance().GetPurchases(OnGetPurchases);
            }
            else
            {
                _initializedAction(false, false, null);
                _initializedAction = null;
            }
        }

        private static void OnBillingSetupFinished(bool result)
        {
            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                ProcessNotConsumedProduct(result);
            });
        }

        private static void OnBuyProductCallback(bool isSuccess, bool isCancelled, OneStore.PurchaseData data)
        {
#if MDEBUG
            Debug.Log("callback from BuyProduct " + isSuccess + ", " + isCancelled);
#endif
            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                OnProductPurchased(isSuccess, isCancelled, data);
            });
        }

        private static void OnProductPurchased(bool isSuccess, bool isCancelled, OneStore.PurchaseData purchase)
        {
            if (isCancelled)
            {
                if (_purchaseCompletionAction != null)
                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_USER_CANCEL, null, "");
            }
            else
            {
                if (isSuccess)
                {
                    WebClient webClient = WebClient.GetInstance();
                    List<OneStorePurchasedProduct> list = new List<OneStorePurchasedProduct>();
                    OneStorePurchasedProduct p = new OneStorePurchasedProduct();
                    p.OrderId = purchase.orderId;
                    p.PackageName = purchase.packageName;
                    p.ProductId = purchase.productId;
                    p.PurchaseTime = purchase.purchaseTime;
                    p.PurchaseId = purchase.purchaseId;
                    p.DeveloperPayload = purchase.developerPayload;
                    p.PurchaseState = purchase.purchaseState;
                    p.RecurringState = purchase.recurringState;
                    list.Add(p);
                    webClient.RequestConsumeOneStoreReceipt(list, OnConsumeOneStoreReceiptResult);
                }
                else
                {
                    if (_purchaseCompletionAction != null)
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "");
                }
            }
        }

        private static void OnProductConsumed(bool isSuccess)
        {
#if MDEBUG
            Debug.Log("OnProductConsumed " + isSuccess);
#endif
            if (isSuccess)
            {
                if (_purchaseCompletionAction != null)
                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_SUCCESS, _purchasedData, "ok");
            }
            else
            {
                if (_purchaseCompletionAction != null)
                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "failed to consume");
            }
        }

        public static void OnConsume(bool isSuccess, OneStore.PurchaseData consumedData)
        {
            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                OnProductConsumed(isSuccess);
            });
        }

        public static void OnConsumeOneStoreReceiptResult(WebClient.ErrorCode error, List<OneStorePurchasedProduct> consumedList, byte[] purchasedData)
        {
            bool bProcessIAP = false;
            switch (error)
            {
                case WebClient.ErrorCode.SUCCESS:
                    if (consumedList != null)
                    {
                        _purchasedData = null;
                        foreach (OneStorePurchasedProduct p in consumedList)
                        {
                            OneStore.PurchaseData purchase = new OneStore.PurchaseData();
                            purchase.orderId = p.OrderId;
                            purchase.packageName = p.PackageName;
                            purchase.productId = p.ProductId;
                            purchase.purchaseTime = p.PurchaseTime;
                            purchase.purchaseId = p.PurchaseId;
                            purchase.developerPayload = p.DeveloperPayload;
                            purchase.purchaseState = p.PurchaseState;
                            purchase.recurringState = p.RecurringState;
                            switch (p.ConsumptionState)
                            {
                                case ConsumptionState.Yet:  // yet consumed
                                    _purchasedData = purchasedData;
                                    bProcessIAP = true;
                                    OneStore.IAPManager.GetInstance().Consume(purchase, OnConsume);
                                    break;
                                case ConsumptionState.Consumed:  // consumed : ?????????????????? ?????? ????????? ?????????
                                    _purchasedData = purchasedData;
                                    bProcessIAP = true;
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "verification failed");
                                    else
                                    {
                                        OneStore.IAPManager.GetInstance().Consume(purchase, OnConsume);
                                    }
                                    break;
                                case ConsumptionState.Unauthorized:  // unauthorized : ?????? ???????????? ?????? ?????? ????????? ?????? ?????? 
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "verification failed");
                                    break;
                                case ConsumptionState.Invalid:  // invalid : ???????????? ?????? ?????????
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "verification failed");
                                    break;
                                case ConsumptionState.Unknown:  // unknown  
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "verification failed");
                                    break;
                                case ConsumptionState.Canceled:  // ???????????? ????????? 
                                case ConsumptionState.Duplicated:  // ?????? ???????????? ?????? ????????? ????????? 
                                    OneStore.IAPManager.GetInstance().Consume(purchase, OnConsume);
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "verification failed");
                                    break;
                            }
                        }
                    }
                    if (_initializedAction != null)
                    {
                        _initializedAction(true, bProcessIAP, _purchasedData);
                        _initializedAction = null;
                    }
                    break;
                case WebClient.ErrorCode.OTHER_DEVICE_LOGIN_ERROR:
                    // ?????? ???????????? ????????? ???
                    if (_purchaseCompletionAction != null)
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "other device login error");
                    if (_initializedAction != null)
                    {
                        _initializedAction(true, false, null);
                        _initializedAction = null;
                    }
                    break;
                default:
                    // ???????????? ??????
                    if (_purchaseCompletionAction != null)
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "network error");
                    if (_initializedAction != null)
                    {
                        _initializedAction(true, false, null);
                        _initializedAction = null;
                    }
                    break;
            }

        }
#else
        private static void OnBillingSetupFinishedProcess()
        {
            // ???????????? ??? ???????????? ?????? ????????? ?????? 
            List<AN_Purchase> list = new List<AN_Purchase>();
            foreach (AN_Purchase tpl in AN_Billing.Inventory.Purchases)
            {
#if MDEBUG
                Debug.Log("----------------------------------------------\n Not Yet Processed Product : " + tpl.ProductId + "\n--------------------------------------------------");
#endif
                AN_Product p = AN_Billing.Inventory.GetProductById(tpl.ProductId);
                if (p != null && p.IsConsumable && tpl.PurchaseState == 0)
                {
                    list.Add(tpl);
                }
            }
            if (list.Count > 0)
            {
                WebClient webClient = WebClient.GetInstance();
                webClient.RequestConsumeGoogleReceipt(list, OnConsumeGoogleReceiptResult);
                // OnConsumeGoogleReceiptResult ?????? _initializedAction??? ????????????. ????????? ?????? ????????????...
            }
            else
            {
                _initializedAction(true, false, null);
                _initializedAction = null;
            }
        }

        private static void OnBillingSetupFinished(SA.Android.Vending.Billing.AN_BillingConnectionResult result)
        {
#if MDEBUG
            Debug.Log("OnBillingSetupFinished + " + result.IsSucceeded);
#endif
            if (result.IsSucceeded)
            {
                _isInitialized = true;
                OnBillingSetupFinishedProcess();
            }
            else
            {
                _initializedAction(false, false, null);
                _initializedAction = null;
            }
        }

        private static void OnProductPurchased(AN_BillingPurchaseResult result)
        {
#if MDEBUG
            Debug.Log("OnProductPurchased(AN_BillingPurchaseResult result) ");
            if(result == null) Debug.Log("result = null");
            if(result.Error == null) Debug.Log("result.Error = null");
            Debug.Log("result.Error.Code = " + result.Error.Code);
            Debug.Log("result.IsSucceeded = " + result.IsSucceeded);
            if(result.Purchase == null) Debug.Log("result.Purchase = null");
#endif
            if(result.Error != null && result.Error.Code == -1005)
            {
                if (_purchaseCompletionAction != null)
                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_USER_CANCEL, null, result.Error.Message);
            }
            else
            {
                if (result.IsSucceeded)
                {
                    WebClient webClient = WebClient.GetInstance();
                    List<AN_Purchase> list = new List<AN_Purchase>();
                    list.Add(result.Purchase);
                    webClient.RequestConsumeGoogleReceipt(list, OnConsumeGoogleReceiptResult);
                }
                else
                {
                    //switch (result.Purchase.PurchaseState)
                    //{   
                        //case GooglePurchaseState.CANCELED:
                        //    if (_purchaseCompletionAction != null)
                        //        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_USER_CANCEL, null, result.Error.Message);
                        //    break;
                        //default:
                            if (_purchaseCompletionAction != null)
                                _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, result.Error.Message);
                        //    break;
                    //}
                }
            }
        }

        private static void OnProductConsumed(SA_Result result)
        {
            if (result.IsSucceeded)
            {
                if (_purchaseCompletionAction != null)
                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_SUCCESS, _purchasedData, "ok");
            }
            else
            {
                if (_purchaseCompletionAction != null)
                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "failed to consume");
            }
        }

        public static void OnConsumeGoogleReceiptResult(WebClient.ErrorCode error, List<GoogleConsumedProduct> consumedList, byte[] purchasedData)
        {
            bool bProcessIAP = false;
            switch (error)
            {
                case WebClient.ErrorCode.SUCCESS:
                    if (consumedList != null)
                    {
                        _purchasedData = null;
                        foreach (GoogleConsumedProduct p in consumedList)
                        {
                            switch (p.ConsumptionState)
                            {
                                case ConsumptionState.Yet:  // yet consumed
                                    _purchasedData = purchasedData;
                                    bProcessIAP = true;
                                    AN_Billing.Consume(AN_Billing.Inventory.GetPurchaseByProductId(p.ProductId), (SA_Result result) => {
                                        ThreadSafeDispatcher.Instance.Invoke(() =>
                                        {
                                            OnProductConsumed(result);
                                        });
                                    });
                                    break;
                                case ConsumptionState.Consumed:  // consumed : ???????????? ?????? ????????? ?????????
                                    _purchasedData = purchasedData;
                                    bProcessIAP = true;
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "verification failed");
                                    else
                                    {
                                        AN_Billing.Consume(AN_Billing.Inventory.GetPurchaseByProductId(p.ProductId), (SA_Result result) => {
                                            ThreadSafeDispatcher.Instance.Invoke(() =>
                                            {
                                                OnProductConsumed(result);
                                            });
                                        });
                                    }
                                    break;
                                case ConsumptionState.Unauthorized:  // unauthorized : ?????? ???????????? ?????? ?????? ????????? ?????? ?????? 
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "verification failed");
                                    break;
                                case ConsumptionState.Invalid:  // invalid : ???????????? ?????? ?????????
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_INVALID_RECEIPT, null, "verification failed");
                                    break;
                                case ConsumptionState.Unknown:  // unknown  
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "verification failed");
                                    break;
                                case ConsumptionState.Canceled:  // ???????????? ????????? 
                                case ConsumptionState.Duplicated:  // ?????? ???????????? ?????? ????????? ????????? 
                                    AN_Billing.Consume(AN_Billing.Inventory.GetPurchaseByProductId(p.ProductId), (SA_Result result) => {
                                        ThreadSafeDispatcher.Instance.Invoke(() =>
                                        {
                                            OnProductConsumed(result);
                                        });
                                    });
                                    if (_purchaseCompletionAction != null)
                                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "verification failed");
                                    break;
                            }
                        }
                    }
                    if(_initializedAction != null) 
                    {
                        _initializedAction(true, bProcessIAP, _purchasedData);
                        _initializedAction = null;
                    }
                    break;
                case WebClient.ErrorCode.OTHER_DEVICE_LOGIN_ERROR:
                    // ?????? ???????????? ????????? ???
                    if (_purchaseCompletionAction != null)
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "other device login error");
                    if (_initializedAction != null)
                    {
                        _initializedAction(true, false, null);
                        _initializedAction = null;
                    }
                    break;
                default:
                    // ???????????? ??????
                    if (_purchaseCompletionAction != null)
                        _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, "network error");
                    if (_initializedAction != null)
                    {
                        _initializedAction(true, false, null);
                        _initializedAction = null;
                    }
                    break;
            }
        }
#endif
#endif
        public static void requestProductData(string[] productIdentifiers, Action<List<IAPProduct>> completionHandler)
        {
#if UNITY_ANDROID
#if USE_ONESTORE_IAP
#if MDEBUG
            Debug.Log("GetProductDetails " + productIdentifiers.Length);
#endif
            List<IAPProduct> list = new List<IAPProduct>();

            OneStore.IAPManager.GetInstance().GetProductDetails(productIdentifiers, (bool result, OneStore.ProductDetail[] data) => {
#if MDEBUG
                Debug.Log("callback from GetProductDetails " + result);
#endif
                if (result)
                {
                    for(int i = 0; i < data.Length; i++)
                    {
                        IAPProduct product = new IAPProduct(data[i]);
                        list.Add(product);
                    }
                }
                completionHandler(list);
            });
#else
            List<IAPProduct> list = new List<IAPProduct>();
            for (int i = 0; i < productIdentifiers.Length; i++)
            {
                IAPProduct product = new IAPProduct(AN_Billing.Inventory.GetProductById(productIdentifiers[i]));
                list.Add(product);
            }
            completionHandler(list);
#endif
#elif UNITY_IOS || UNITY_TVOS
            List<IAPProduct> list = new List<IAPProduct>();
            for (int i = 0; i < productIdentifiers.Length; i++)
            {
                try
                {
                    IAPProduct product = new IAPProduct(ISN_SKPaymentQueue.GetProductById(productIdentifiers[i]));
                    list.Add(product);
                }
                catch
                {
                }
            }
            completionHandler(list);
#elif UNITY_STANDALONE && USE_STEAM
            WebClient.GetInstance().RequestSteamShopProductInfo(productIdentifiers, (WebClient.ErrorCode error, List<IAPProduct> list) =>
            {
                if(error == WebClient.ErrorCode.SUCCESS)
                {
                    completionHandler(list);                    
                }
                else 
                {
                    completionHandler(null);                    
                }
            });
            /* ???????????? ????????? ???????????? ??????  
            List<IAPProduct> list = new List<IAPProduct>();

            uint priceItemCount = SteamInventory.GetNumItemsWithPrices();
            SteamItemDef_t[] itemDefs = new SteamItemDef_t[priceItemCount];
            ulong[] prices = new ulong[priceItemCount];
            SteamInventory.GetItemsWithPrices(itemDefs, prices, priceItemCount);
            for (int i = 0; i < itemDefs.Length; i++)
            {
                SteamItemDef_t itemId = itemDefs[i];

                string name = "";
                string desc = "";
                string type = "";
                string ValueBuffer;
                uint ValueBufferSize = 1024;
                if (SteamInventory.GetItemDefinitionProperty(itemId, "name", out ValueBuffer, ref ValueBufferSize))
                    name = ValueBuffer;
                ValueBufferSize = 1024;
                if (SteamInventory.GetItemDefinitionProperty(itemId, "description", out ValueBuffer, ref ValueBufferSize))
                    desc = ValueBuffer;
                ValueBufferSize = 1024;
                if (SteamInventory.GetItemDefinitionProperty(itemId, "type", out ValueBuffer, ref ValueBufferSize))
                    type = ValueBuffer;
                IAPProduct product = new IAPProduct(itemId.ToString(), name, prices[i].ToString(), desc, currencyCode, type);
                list.Add(product);
            }
            completionHandler(list);
            */
#endif
        }

        public static void purchaseProduct(string productId, OnPurchaseCompletion completionHandler)
        {
            _purchaseCompletionAction = completionHandler;
            _purchaseRestorationAction = null;

#if UNITY_ANDROID
#if USE_ONESTORE_IAP
            OneStore.IAPManager.GetInstance().BuyProduct(productId, OnBuyProductCallback);
#else
            AN_Product product = new AN_Product(productId, AN_ProductType.inapp);
            AN_Billing.Purchase(product, (result) => {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    OnProductPurchased(result);
                });
            });
#endif
#elif UNITY_IOS || UNITY_TVOS
            currentProductId = productId;
            ISN_SKPaymentQueue.AddPayment(productId);
#elif UNITY_STANDALONE && USE_STEAM
            WebClient.GetInstance().RequestSteamInitTransaction(productId, (WebClient.ErrorCode error, bool isSucceeded, long orderId, long transId, string steamUrl, int errorCode, string errorDesc) =>  
            {
                if (isSucceeded)
                {
                    // ??????????????? UI??? ?????????. ???????????? ??????????????????????????? ???????????? ???????????? ?????????.
                    // OnSteamMicroTxnAuthorizationResponse ??? ?????????
                }
                else
                {
                    // ??????????????? ?????? ?????? 
                    _purchaseCompletionAction(PurchaseResultCode.PURCHASE_FAIL, null, errorDesc);
                }
            });
#endif
        }

        public static void restoreCompletedTransactions(OnPurchaseRestoration completionHandler)
        {
            _purchaseCompletionAction = null;
            _purchaseRestorationAction = completionHandler;

#if UNITY_IOS || UNITY_TVOS
            ISN_SKPaymentQueue.RestoreCompletedTransactions();
#endif
        }

        public static bool isConnected()
        {
#if UNITY_ANDROID
            return true;
#else
            return true;
#endif
        }
        
    }

}

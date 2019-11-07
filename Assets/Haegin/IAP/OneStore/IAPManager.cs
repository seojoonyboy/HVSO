using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OneStore
{
    public class IAPManager : MonoBehaviour
    {
        private static AndroidJavaObject iapRequestAdapter = null;
        private static AndroidJavaClass jc = null;
        private static bool isServiceCreated = false;
        private const int IAP_API_VERSION = 5;

        enum CBType
        {
            Connected,
            Disconnected,
            NeedUpdate,
            Success,
            Error,
            RemoteEx,
            SecurityEx,

        };

        Dictionary<CBType, string> preDefinedStrings = new Dictionary<CBType, string>() {
            { CBType.Connected, "onConnected" },
            { CBType.Disconnected, "onDisconnected" },
            { CBType.NeedUpdate, "onErrorNeedUpdateException" },
            { CBType.Success, "onSuccess" },
            { CBType.Error, "onError" },
            { CBType.RemoteEx, "onErrorRemoteException" },
            { CBType.SecurityEx, "onErrorSecurityException" }
        };


        public static IAPManager GetInstance()
        {
            GameObject gameObject = GameObject.Find("HaeginOneStoreIAP");
            if (gameObject == null)
            {
                gameObject = new GameObject("HaeginOneStoreIAP");
                DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<IAPManager>();
                IAPManager objIAPManager = gameObject.GetComponent<IAPManager>();
                

            }
            return gameObject.GetComponent<IAPManager>();
        }

        IAPManager()
        {
            jc = new AndroidJavaClass("com.haegin.haeginmodule.onestore.OneStoreNative");
            iapRequestAdapter = jc.CallStatic<AndroidJavaObject>("getInstance");
        }

        public delegate void ServiceConnectionCallback(bool isSuccess);
        private ServiceConnectionCallback serviceConnectionCallback = null;
        public void ConnectService(ServiceConnectionCallback callback)
        {
            serviceConnectionCallback = callback;
            iapRequestAdapter.Call("connect", Haegin.ProjectSettings.oneStoreBase64EncodedPublicKey);
        }
        public void ServiceConnectionListener(string result)
        {
#if MDEBUG
            Debug.Log("ServiceConnectionListener : " + result);
#endif
            if (result.Contains(preDefinedStrings[CBType.Connected]))
            {
                isServiceCreated = true;
                if(serviceConnectionCallback != null) serviceConnectionCallback(true);
            }
            else if(result.Contains(preDefinedStrings[CBType.NeedUpdate]))
            {
                if(serviceConnectionCallback != null) serviceConnectionCallback(false);
            }
            else
            {
                if (serviceConnectionCallback != null) serviceConnectionCallback(false);
            }
            serviceConnectionCallback = null;
        }

        public delegate void IsBillingSupportedCallback(bool isSupported);
        private IsBillingSupportedCallback isBillingSupportedCallback = null;
        public void IsBillingSupported(IsBillingSupportedCallback callback)
        {
            if(isServiceCreated)
            {
                isBillingSupportedCallback = callback;
                iapRequestAdapter.Call("isBillingSupported", IAP_API_VERSION);
            }
            else
            {
                callback(false);
            }
        }

        public bool GetResult(string result)
        {
            if (result.Contains(preDefinedStrings[CBType.Connected]))
            {
                return true;
            }
            else if (result.Contains(preDefinedStrings[CBType.Success]))
            {
                return true;
            }
            else if (result.Contains(preDefinedStrings[CBType.NeedUpdate]))
            {
                // 여기서 별로도로 뭔가를 처리해야할까?
                return false;
            }
            else if (result.Contains(preDefinedStrings[CBType.RemoteEx]))
            {
                return false;
            }
            else if (result.Contains(preDefinedStrings[CBType.SecurityEx]))
            {
                return false;
            }
            else
            {
                return false;
            }

        }

        public void BillingSupportedListener(string result)
        {
            if(isBillingSupportedCallback != null) isBillingSupportedCallback(GetResult(result));
        }

        public delegate void GetPurchasesCallback(bool isSuccess, PurchaseData[] val);
        private GetPurchasesCallback getPurchasesCallback = null;
        public void GetPurchases(GetPurchasesCallback callback)
        {
            if (isServiceCreated)
            {
                getPurchasesCallback = callback;
                iapRequestAdapter.Call("getPurchase", IAP_API_VERSION, "inapp");
            }
            else
            {
                callback(false, null);
            }
        }

        private PurchaseData GetPurchaseData(string data)
        {
            try
            {
                string[] split = data.Split('|');
                PurchaseData purchase = new PurchaseData();
                purchase.orderId = split[0];
                purchase.packageName = split[1];
                purchase.productId = split[2];
                purchase.purchaseTime = long.Parse(split[3]);
                purchase.purchaseId = split[4];
                purchase.developerPayload = split[5];
                purchase.purchaseState = int.Parse(split[6]);
                purchase.recurringState = int.Parse(split[7]);
                return purchase;
            }
            catch (System.Exception ex)
            {
#if MDEBUG
                Debug.Log("Exception GetPurchaseData");
                Debug.Log(ex.ToString());
                Debug.Log(ex.StackTrace);
#endif  
                return null;
            }
        }

        private PurchaseData[] GetPurchaseDataArray(string data)
        {
            try
            {
                string[] split = data.Split('|');
                int si = 1;
                int count = int.Parse(split[0]);
                if(count > 0)
                {
#if MDEBUG
                    for(int i = 0; i < split.Length; i++)
                    {
                        Debug.Log("[" + i + "] " + split[i]);
                    }
#endif


                    PurchaseData[] purchases = new PurchaseData[count];
                    for (int i = 0; i < count; i++)
                    {
                        purchases[i] = new PurchaseData();
                        purchases[i].orderId = split[si++];
                        purchases[i].packageName = split[si++];
                        purchases[i].productId = split[si++];
                        purchases[i].purchaseTime = long.Parse(split[si++]);
                        purchases[i].purchaseId = split[si++];
                        purchases[i].developerPayload = split[si++];
                        purchases[i].purchaseState = int.Parse(split[si++]);
                        purchases[i].recurringState = int.Parse(split[si++]);
                        si++; // skip ProductType 
                    }
                    return purchases;
                }
                else
                {
                    return null;
                }

            }
            catch(System.Exception ex)
            {
#if MDEBUG
                Debug.Log("Exception GetPurchasesDataArray");
                Debug.Log(ex.ToString());
                Debug.Log(ex.StackTrace);
#endif  
                return null;
            }
        }


        public void QueryPurchaseListener(string result)
        {
#if MDEBUG
            Debug.Log("QueryPurchaseListener  " + result);
#endif
            string data = findStringAfterCBType(result, CBType.Success);
            if (data.Length > 0)
            {
                PurchaseData[] purchases = GetPurchaseDataArray(data);
                if (getPurchasesCallback != null)
                {
#if MDEBUG
                    Debug.Log("call GetPurchasesCallback   1 " + (purchases != null));
#endif
                    getPurchasesCallback(purchases != null, purchases);
                }
            }
            else
            {
                //onError만 뒤에 추가데이터 IapResult 가 있으므로 추가 데이터만 전달해주고 나머지 에러들은 추가 데이터가 없으므로 callback 그대로만 전달해준다. 
                string errorData = findStringAfterCBType(result, CBType.Error);
                if (errorData.Length > 0)
                {
                    Debug.Log(errorData);
                }
                if (getPurchasesCallback != null)
                {
#if MDEBUG
                    Debug.Log("call GetPurchasesCallback   2");
#endif
                    getPurchasesCallback(false, null);
                }
            }
        }



        public delegate void GetProductDetailsCallback(bool isSuccess, ProductDetail[] val);
        private GetProductDetailsCallback getProductDetailsCallback = null;
        public void GetProductDetails(string[] skus, GetProductDetailsCallback callback)
        {
#if MDEBUG
            Debug.Log("GetProductDetails  " + skus.Length);
#endif
            if (isServiceCreated)
            {
                getProductDetailsCallback = callback;
                iapRequestAdapter.Call("getProductDetails", new object[] { IAP_API_VERSION, skus, "inapp" });
            }
            else
            {
                callback(false, null);
            }
        }

        private ProductDetail[] GetProductDetailArray(string data)
        {
            try
            {
                string[] split = data.Split('|');
                int si = 1;
                int count = int.Parse(split[0]);
                if(count > 0)
                {
                    ProductDetail[] details = new ProductDetail[count];
                    for (int i = 0; i < count; i++)
                    {
                        details[i] = new ProductDetail();
                        details[i].productId = split[si++];
                        details[i].type = split[si++];
                        details[i].price = split[si++];
                        details[i].title = split[si++];
                    }
                    return details;
                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception ex)
            {
#if MDEBUG
                Debug.Log("Exception GetProductDetailArray");
                Debug.Log(ex.ToString());
                Debug.Log(ex.StackTrace);
#endif  
                return null;
            }
        }

        public void QueryProductsListener(string callback)
        {
#if MDEBUG
            Debug.Log("QueryProductsListener  " + callback);
#endif
            string data = findStringAfterCBType(callback, CBType.Success);
            if (data.Length > 0)
            {
                try
                {
                    ProductDetail[] details = GetProductDetailArray(data);
                    if(getProductDetailsCallback != null) getProductDetailsCallback(true, details);
                }
                catch (System.Exception ex)
                {
                    if(getProductDetailsCallback != null) getProductDetailsCallback(false, null);
                }
            }
            else
            {
#if MDEBUG
                string errorData = findStringAfterCBType(callback, CBType.Error);
                Debug.Log(errorData);
#endif
                if(getProductDetailsCallback != null) getProductDetailsCallback(false, null);
            }
        }

        public delegate void BuyProductCallback(bool isSuccess, bool isCancelled, PurchaseData pdata);
        private BuyProductCallback buyProductCallback = null;
        public void BuyProduct(string productId, BuyProductCallback callback)
        {
            if (isServiceCreated)
            {
                buyProductCallback = callback;
                string gameUserId = "";
                bool promotionApplicable = false;
                string productType = "inapp";
                string payload = "";
                iapRequestAdapter.Call("launchPurchaseFlow", IAP_API_VERSION, productId, productType, payload, gameUserId, promotionApplicable);
            }
            else
            {
                callback(false, false, null);
            }
        }

        public void PurchaseFlowListener(string result)
        {
#if MDEBUG
            Debug.Log("PurchaseFlowListener  " + result);
#endif
            string data = findStringAfterCBType(result, CBType.Success);

            if (data.Length > 0)
            {
                try
                {
                    if (buyProductCallback != null)
                    {
                        PurchaseData purchaseData = GetPurchaseData(data);
#if MDEBUG
                        Debug.Log("PurchaseFlowListener  1 " + purchaseData.productId);
#endif
                        buyProductCallback(purchaseData != null, false, purchaseData);
                    }
                }
                catch (System.Exception ex)
                {
                    if (buyProductCallback != null)
                    {
#if MDEBUG
                        Debug.Log("PurchaseFlowListener  2");
#endif
                        buyProductCallback(false, false, null);
                    }
                }
            }
            else
            {
                if (buyProductCallback != null)
                {
                    string[] errorData = findStringAfterCBType(result, CBType.Error).Split('|');
                    switch(int.Parse(errorData[0]))
                    {
                        case 1:  // User Cancel
#if MDEBUG
                            Debug.Log("PurchaseFlowListener  User Cancel");
#endif
                            buyProductCallback(false, true, null);
                            break;
                        default:
#if MDEBUG
                            Debug.Log("PurchaseFlowListener  Error " + errorData[1]);
#endif
                            buyProductCallback(false, false, null);
                            break;
                    }
                }
            }
        }


        public delegate void ConsumeCallback(bool isSuccess, PurchaseData data);
        private ConsumeCallback consumeCallback = null;
        public void Consume(PurchaseData data, ConsumeCallback callback)
        {
            if (isServiceCreated)
            {
                consumeCallback = callback;
                string str = PurchaseDataToStr(data);
#if MDEBUG
                Debug.Log("PurchaseDataToStr [" + str + "]");
#endif
                iapRequestAdapter.Call("consumeItem", IAP_API_VERSION, str);
            }
            else
            {
                callback(false, data);
            }
        }

        private string PurchaseDataToStr(PurchaseData data)
        {
            string str = data.orderId + "|" + data.packageName + "|" + data.productId + "|" + data.purchaseTime.ToString() + "|" + data.purchaseId + "|" + data.developerPayload + "|" + data.purchaseState + "|" + data.recurringState;
#if MDEBUG
            Debug.Log("PurchaseDataToStr [" + str + "]");
#endif
            return str;
        }

        public void ConsumeListener(string callback)
        {
            string data = findStringAfterCBType(callback, CBType.Success);
            if (data.Length > 0)
            {
                PurchaseData purchaseData = GetPurchaseData(data);
                if(consumeCallback != null) consumeCallback(true, purchaseData);
            }
            else
            {
#if MDEBUG
                string errorData = findStringAfterCBType(callback, CBType.Error);
                Debug.Log(errorData);
#endif
                if(consumeCallback != null) consumeCallback(false, null);
            }
        }



        // 결과 callback string에서 CBType  string을 제외한 다음 문자열을 돌려준다.
        private string findStringAfterCBType(string data, CBType type)
        {
            int length = preDefinedStrings[type].Length;
            if (data.Substring(0, length).Equals(preDefinedStrings[type]))
            {
                return data.Substring(length);
            }
            else
            {
                return "";
            }
        }


    }
}

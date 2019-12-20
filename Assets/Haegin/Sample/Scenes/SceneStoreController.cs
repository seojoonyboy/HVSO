using UnityEngine;
using UnityEngine.UI;
using Haegin;
using HaeginGame;
using System.Collections.Generic;

public class SceneStoreController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject systemDialog;
    public GameObject eulaText;
    private GameObject helpDialog;
    public GameObject helpDialogPortrait;
    public GameObject helpDialogLandscape;

    private WebClient webClient;

    void Awake()
    {
        UGUICommon.ResetCanvasReferenceSize(canvas);
        if (Screen.height > Screen.width) helpDialog = helpDialogPortrait;
        else helpDialog = helpDialogLandscape;

        webClient = WebClient.GetInstance();

        webClient.ErrorOccurred += OnErrorOccurred;
        webClient.Processing += OnProcessing;
        webClient.RetryOccurred += RetryOccurred;
        webClient.RetryFailed += RetryFailed;
        webClient.MaintenanceStarted += OnMaintenanceStarted;
        webClient.Logged += (string log) =>
        {
#if MDEBUG
            Debug.Log("Unity   " + log);
#endif
        };

        ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(OnSystemBackKey);
    }

    void RetryOccurred(Protocol protocol, int retryCount)
    {
#if MDEBUG
        Debug.Log("Retry Occurred  " + retryCount);
#endif
    }

    void RetryFailed(Protocol protocol)
    {
        OnNetworkError();
    }

    void OnNetworkError()
    {
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.NetworkError), TextManager.GetString(TextManager.StringTag.NetworkErrorMessage), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnMaintenanceStarted()
    {
        // 메인터넌스가 시작되었다.
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.ServerMaintenanceTitle), TextManager.GetString(TextManager.StringTag.ServerMaintenance), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Ok)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnProcessing(ReqAndRes rar)
    {
    }

    public void OnErrorOccurred(int error)
    {
        OnNetworkError();
    }

    public void OnSystemBackKey()
    {
        UGUICommon.ShowYesNoDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.Quit), TextManager.GetString(TextManager.StringTag.QuitConfirm), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Yes)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnDestroy()
    {
        if (webClient != null)
        {
            webClient.ErrorOccurred -= OnErrorOccurred;
            webClient.Processing -= OnProcessing;
            webClient.RetryOccurred -= RetryOccurred;
            webClient.RetryFailed -= RetryFailed;
            webClient.MaintenanceStarted -= OnMaintenanceStarted;
        }

        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
    }


    void RewardedVideoAdOpenedEvent()
    {
    }

    void RewardedVideoAdClosedEvent()
    {
    }

    void RewardedVideoAvailabilityChangedEvent(bool available)
    {
        bool rewardedVideoAvailability = available;
    }

    void RewardedVideoAdStartedEvent()
    {
    }

    void RewardedVideoAdEndedEvent()
    {
    }

    void RewardedVideoAdRewardedEvent(IronSourcePlacement placement)
    {
    }

    void RewardedVideoAdShowFailedEvent(IronSourceError error)
    {
    }

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    void Start()
    {
        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;

#if MDEBUG
        IronSource.Agent.setAdaptersDebug(true);
#endif
        IronSource.Agent.setUserId(Account.GetAccountId());
#if UNITY_ANDROID
//        IronSource.Agent.init("7592f4ed"); // HomerunClash
        IronSource.Agent.init("8f3317a5"); // OVERDOX
#elif UNITY_IOS
//        IronSource.Agent.init("7592bb65"); // HomerunClash
        IronSource.Agent.init("8f32de1d"); // OVERDOX
#endif
        IronSource.Agent.validateIntegration();
        IronSource.Agent.shouldTrackNetworkState(true);
        var products = new string[] { "com_haegin_test1" };
        IAP.init(products, (bool result, bool bProcessIAP, byte[] purchasedData) =>
        {
            if(result)
            {
#if MDEBUG
                Debug.Log("call requestProductData");
#endif
                if (bProcessIAP)
                {
                    // 구매과정에서 지급되지 않았다가 재실행시 지급된 아이템 정보
                    // 
                    Debug.Log("지급되지 않았던 구매 아이템이 있었습니다. 정상적으로 지급 처리 되었습니다.");
                }

                IAP.requestProductData(products, productList =>
                {
                    Button btnBuy = GameObject.Find("ButtonBuy").GetComponent<Button>();
                    btnBuy.interactable = true;

                    Text textProductInfo = GameObject.Find("ProductInfoLabel").GetComponent<Text>();
                    textProductInfo.text = TextManager.GetString(TextManager.StringTag.NameTag) + productList[0].title + "\n" +
                        TextManager.GetString(TextManager.StringTag.DescTag) + productList[0].description + "\n" +
                        TextManager.GetString(TextManager.StringTag.PriceTag) + productList[0].price;
                });
            }
            else
            {
                Button btnBuy = GameObject.Find("ButtonBuy").GetComponent<Button>();
                btnBuy.interactable = true;
                Debug.Log("인앱 초기화에 실패했습니다.");
            }
        });
    }

    public void SetInteractableWebViewButton(bool value)
    {
        GameObject.Find("ButtonHelp").GetComponent<Button>().interactable = value;
        GameObject.Find("ButtonTOS").GetComponent<Button>().interactable = value;
        GameObject.Find("ButtonPP").GetComponent<Button>().interactable = value;
        GameObject.Find("ButtonCafe").GetComponent<Button>().interactable = value;
    }

    public void OnButtonHelpClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonHelpClick");
#endif
        SetInteractableWebViewButton(false);
#if USE_ORG_HELP
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.Main, "http://haegin.kr/Help/HomerunClash/v1_1_0", "UserId", "Nickname", "AppVersion", action => {
#else
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.ZendeskMain, "http://haegin.kr", "UserId", "Nickname", "AppVersion", action => {
#endif
            if (action == UGUICommon.HelpDialogAction.Close) {
                SetInteractableWebViewButton(true);
            }
        });
    }

    public void OnButtonTOSClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonTOSClick");
#endif
        SetInteractableWebViewButton(false);
#if USE_ORG_HELP
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.TermsOfService, "http://haegin.kr/Help/HomerunClash/v1_1_0", "UserId", "Nickname", "AppVersion", action => {
#else
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.ZendeskTermsOfService, "http://haegin.kr", "UserId", "Nickname", "AppVersion", action => {
#endif
            if (action == UGUICommon.HelpDialogAction.Close)
            {
                SetInteractableWebViewButton(true);
            }
        });
    }

    public void OnButtonPPClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonPPClick");
#endif
        SetInteractableWebViewButton(false);
#if USE_ORG_HELP
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.PrivacyPolicy, "http://haegin.kr/Help/HomerunClash/v1_1_0", "UserId", "Nickname", "AppVersion", action => {
#else
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.ZendeskPrivacyPolicy, "http://haegin.kr", "UserId", "Nickname", "AppVersion", action => {
#endif
        if (action == UGUICommon.HelpDialogAction.Close)
            {
                SetInteractableWebViewButton(true);
            }
        });
    }

    public void OnButtonAPClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonAPClick");
#endif
        SetInteractableWebViewButton(false);
#if USE_ORG_HELP
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.AcquirePossibility, "http://haegin.kr/Help/HomerunClash/v1_1_0", "UserId", "Nickname", "AppVersion", action => {
#else
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.ZendeskAcquirePossibility, "http://haegin.kr", "UserId", "Nickname", "AppVersion", action => {
#endif
            if (action == UGUICommon.HelpDialogAction.Close)
            {
                SetInteractableWebViewButton(true);
            }
        });
    }

    public void OnButtonCafeClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonCafeClick");
#endif
        SetInteractableWebViewButton(false);
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.None, "https://www.facebook.com/haeginkr", null, null, null, action => {
        if (action == UGUICommon.HelpDialogAction.Close)
            {
                SetInteractableWebViewButton(true);
            }
        });
    }

    public void OnButtonMailTo(string param)
    {
        Help.SendSupportMail("userid", "nickname", "appversion");
    }

    public void OnButtonRedeemCoupon(string param)
    {
        Coupon.RedeemCoupon("couponid123", (Coupon.RedeemResultCode redeemResult, byte[] data) =>
        {
            UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, "Coupon", "Result : " + redeemResult.ToString(), (UGUICommon.ButtonType buttonType) =>
            {
            });
        });
    }

    public void OnButtonRewardVideoAds(string param)
    {
        if(IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo("YOUR_PLACEMENT_NAME");
        }
    }

    public void OnButtonReadContacts(string param)
    {
        Contacts.GetContactsInfo((Contacts.ContactsResult result, List<ContactsRecord> records) => {
            switch(result)
            {
                case Contacts.ContactsResult.Success:
                    Debug.Log("---------------------------------------------------------");
                    for (int i = 0; i < records.Count; i++)
                    {
                        Debug.Log(records[i].Name + " : " + records[i].Phone);
                    }
                    Debug.Log("---------------------------------------------------------");
                    break;
            }
        });
    }

    public void OnButtonBuyClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonBuyClick");
#endif
        Button btnBuy = GameObject.Find("ButtonBuy").GetComponent<Button>();
        btnBuy.interactable = false;

        GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Purchasing);

        IAP.purchaseProduct("com_haegin_test1", (result, purchasedData, errorMsg) =>
        {
            btnBuy.interactable = true;
            switch (result)
            {
                case IAP.PurchaseResultCode.PURCHASE_SUCCESS:
                    GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Purchased);
#if MDEBUG
                    Debug.Log("Unity : item purchased..");
#endif
                    // 데이터 갱신
                    //
                    // purchasedData 에 byte[] 로 구매된 게임내 아이템 정보가 넘어옵니다. 이 정보를 이용해서 업데이트 하시면 되겠습니다. 
                    //
                    if (purchasedData != null)
                    {
#if MDEBUG
                        Debug.Log("PurchasedData Length = " + purchasedData.Length);
#endif
                        //
                        //   게임별로 네트워크 프로토콜 받을 때, PurchasedInfo class도 받아서 사용하시면 될 것 같습니다.
                        // 
                        PurchasedInfo purchasedInfo = PurchasedInfo.Deserialize(purchasedData);
#if MDEBUG
                        Debug.Log("Purchased Info : " + purchasedInfo.ProductId + ", " + purchasedInfo.TransactionId);
#endif
                    }
#if MDEBUG
                    else
                        Debug.Log("PurchasedData Length = " + 0);
#endif
                    break;
                case IAP.PurchaseResultCode.PURCHASE_FAIL:
                    GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.FailedToPurchase);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_USER_CANCEL:
                    GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Cancelled);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_INVALID_RECEIPT:
                    GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.VerificationFailed);
                    break;
            }
        });
    }
}

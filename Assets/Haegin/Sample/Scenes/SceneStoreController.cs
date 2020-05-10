using UnityEngine;
using UnityEngine.UI;
using Haegin;
using HaeginGame;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneStoreController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject systemDialog;
    public GameObject eulaText;
    private GameObject helpDialog;
    public GameObject helpDialogPortrait;
    public GameObject helpDialogLandscape;

    public InputField inputField;

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

        Debug.Log("UNITYCACHE --------7--------  after new scene loaded");
        Debug.Log("UNITYCACHE maximumAvailableStorageSpace = " + Caching.defaultCache.maximumAvailableStorageSpace);
        Debug.Log("UNITYCACHE spaceFree = " + Caching.defaultCache.spaceFree);
        Debug.Log("UNITYCACHE spaceOccupied = " + Caching.defaultCache.spaceOccupied);
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
                // 앱을 재시작합니다. 게임별로 별도의 작업이 필요할꺼에요.                
                SceneManager.LoadScene(0, LoadSceneMode.Single);
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
                // 앱을 재시작합니다. 게임별로 별도의 작업이 필요할꺼에요.                
                SceneManager.LoadScene(0, LoadSceneMode.Single);
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
#if MDEBUG
        Debug.Log("RewardedVideoAdClosedEvent");
#endif
        canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text += ",AdClose";
    }

    void RewardedVideoAvailabilityChangedEvent(bool available)
    {
        bool rewardedVideoAvailability = available;
    }

    void RewardedVideoAdStartedEvent()
    {
#if MDEBUG
        Debug.Log("RewardedVideoAdStartedEvent");
#endif
    }

    void RewardedVideoAdEndedEvent()
    {
    }

    void RewardedVideoAdRewardedEvent(IronSourcePlacement placement)
    {
#if MDEBUG
        Debug.Log("RewardedVideoAdRewardedEvent");
#endif
        canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text += ",AdReward";
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
#if !UNITY_EDITOR
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
        IronSource.Agent.init("7592f4ed"); // HomerunClash
//        IronSource.Agent.init("8f3317a5"); // OVERDOX
//        IronSource.Agent.init("a7db5965"); // Extreme Golf

#elif UNITY_IOS
        IronSource.Agent.init("7592bb65"); // HomerunClash
//        IronSource.Agent.init("8f32de1d"); // OVERDOX
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
                    Button btnBuy = canvas.transform.Find("ButtonBuy").GetComponent<Button>();
                    btnBuy.interactable = true;

                    Text textProductInfo = canvas.transform.Find("ProductInfoLabel").GetComponent<Text>();
                    textProductInfo.text = TextManager.GetString(TextManager.StringTag.NameTag) + productList[0].title + "\n" +
                        TextManager.GetString(TextManager.StringTag.DescTag) + productList[0].description + "\n" +
                        TextManager.GetString(TextManager.StringTag.PriceTag) + productList[0].price;
                });
            }
            else
            {
                Button btnBuy = canvas.transform.Find("ButtonBuy").GetComponent<Button>();
                btnBuy.interactable = true;
                Debug.Log("인앱 초기화에 실패했습니다.");
            }
        });
#endif
    }

    public void SetInteractableWebViewButton(bool value)
    {
        canvas.transform.Find("ButtonHelp").GetComponent<Button>().interactable = value;
        canvas.transform.Find("ButtonTOS").GetComponent<Button>().interactable = value;
        canvas.transform.Find("ButtonPP").GetComponent<Button>().interactable = value;
        canvas.transform.Find("ButtonCafe").GetComponent<Button>().interactable = value;
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

    public void OnButtonYoutubeClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonYoutubeClick");
#endif
        SetInteractableWebViewButton(false);

        string content = @"<iframe width='100%' height='100%' src='https://www.youtube.com/embed/PErqizZqLjI?controls=0' frameborder='0' allow='accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture'></iframe>";
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.None, content, null, null, null, action => {
            if (action == UGUICommon.HelpDialogAction.Close)
            {
                SetInteractableWebViewButton(true);
            }
        });
    }

    public void OnButtonNoticeClick(string param)
    {
#if MDEBUG
        Debug.Log("Unity : OnButtonNoticeClick");
#endif
        SetInteractableWebViewButton(false);
        UGUICommon.ShowHelpWindow(helpDialog, canvas, Help.HelpItem.None, "http://haegin.kr/cs/v2/test/ToS.html?random=" + Random.value, null, null, null, action => {
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

            canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = "AdStart";
        }
    }

    public void OnButtonInitIAP(string param)
    {
#if !UNITY_EDITOR
        var products = new string[] { "com_haegin_test1" };
        IAP.init(products, (bool result, bool bProcessIAP, byte[] purchasedData) =>
        {
            if (result)
            {
                if (bProcessIAP)
                {
                    canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = "재초기화 성공. 미지급상품 처리완료";
                }
                else
                {
                    canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = "재초기화 성공. 미지급상품 없음";
                }
            }
            else
            {
                canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = "재초기화 실패.";
            }
        });
#endif
    }

    public void OnButtonOpenURL(string param)
    {
        Application.OpenURL("https://hghomerunclash.page.link/?link=https%3A%2F%2Fdlink.haegin.kr%2F%3Finviteid%3D1234iav291%26id2%3D12345&apn=com.haegin.homerunclash&afl=http%3a%2f%2fhaegin.kr%2fdevtest%2fonestore_dlink_homerunclash.php&ibi=com.haegin.homerunclash&isi=1345750763&ofl=http%3A%2F%2Fhaegin.kr%2Ffailed&st=TestTitle&sd=Desc.&si=http%3A%2F%2Fhaegin.kr%2FNoticeImg%2FFox_st.png");
    }

    public void OnButtonChangeResolution(string param)
    {
#if MDEBUG
        int systemWidth = Display.main.systemWidth;
        int systemHeight = Display.main.systemHeight;
        int renderingWidth = Display.main.renderingWidth;
        int renderingHeight = Display.main.renderingHeight;
        int currentResolutionWidth = Screen.currentResolution.width;
        int currentResolutionHeight = Screen.currentResolution.height;
#endif
        if (Screen.currentResolution.width == Display.main.systemWidth)
        {
            Screen.SetResolution(Display.main.systemWidth / 2, Display.main.systemHeight / 2, true);
        }
        else
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
        }
#if MDEBUG
        Debug.Log("Display.displays.Lenght = " + Display.displays.Length);
        Debug.Log("Screen.resolutions.Lenght = " + Screen.resolutions.Length);

        //Debug.Log("Display.main.systemWidth = " + systemWidth + " ==> " + Display.main.systemWidth + "(" +  UGUICommon.GetDisplayWidth() + ")");
        //Debug.Log("Display.main.systemHeight = " + systemHeight + " ==> " + Display.main.systemHeight + "(" + UGUICommon.GetDisplayHeight() + ")");
        Debug.Log("Display.main.renderingWidth = " + renderingWidth + " ==> " + Display.main.renderingWidth);
        Debug.Log("Display.main.renderingHeight = " + renderingHeight + " ==> " + Display.main.renderingHeight);
        Debug.Log("Screen.currentResolution.width = " + currentResolutionWidth + " ==> " + Screen.currentResolution.width);
        Debug.Log("Screen.currentResolution.height = " + currentResolutionHeight + " ==> " + Screen.currentResolution.height);
#endif
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
        Button btnBuy = canvas.transform.Find("ButtonBuy").GetComponent<Button>();
        btnBuy.interactable = false;

        canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Purchasing);

        IAP.purchaseProduct("com_haegin_test1", (result, purchasedData, errorMsg) =>
        {
            btnBuy.interactable = true;
            switch (result)
            {
                case IAP.PurchaseResultCode.PURCHASE_SUCCESS:
                    canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Purchased);
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
                    canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.FailedToPurchase);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_USER_CANCEL:
                    canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Cancelled);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_INVALID_RECEIPT:
                    canvas.transform.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.VerificationFailed);
                    break;
            }
        });
    }

    static bool IS_IN(int val, int min, int max)
    {
        return (((val) >= (min)) && ((val) <= (max)));
    }

    public void OnInputFieldChanged()
    {
#if MDEBUG
        Debug.Log("[InputField OnInputFieldChanged] " + inputField.text);
#endif
        char[] inStr = inputField.text.ToCharArray();
        char[] outStr = new char[inStr.Length];
        int o = 0;

        for(int i = 0; i < inStr.Length;)
        {
            int hs = inStr[i];
            int ls;
            bool length2 = false;
            if (i + 1 < inStr.Length)
            {
                ls = inStr[i + 1];
                length2 = true;
            }
            else
            {
                ls = 0;
                length2 = false;
            }
            if (IS_IN(hs, 0xD800, 0xDBFF))
            {
                if (length2)
                {
                    int uc = ((hs - 0xD800) * 0x400) + (ls - 0xDC00) + 0x10000;

                    // Musical: [U+1D000, U+1D24F]
                    // Enclosed Alphanumeric Supplement: [U+1F100, U+1F1FF]
                    // Enclosed Ideographic Supplement: [U+1F200, U+1F2FF]
                    // Miscellaneous Symbols and Pictographs: [U+1F300, U+1F5FF]
                    // Supplemental Symbols and Pictographs: [U+1F900, U+1F9FF]
                    // Emoticons: [U+1F600, U+1F64F]
                    // Transport and Map Symbols: [U+1F680, U+1F6FF]
                    if (IS_IN(uc, 0x1D000, 0x1F9FF) || IS_IN(uc, 0x1FA70, 0x1FA74) || IS_IN(uc, 0x1FA78, 0x1FA7A) || IS_IN(uc, 0x1FA80, 0x1FA86)
                            || IS_IN(uc, 0x1FA90, 0x1FA9F) || IS_IN(uc, 0x1FAA0, 0x1FAA8) || IS_IN(uc, 0x1FAB0, 0x1FAA6) || IS_IN(uc, 0x1FAC0, 0x1FAC2)
                            || IS_IN(uc, 0x1FAD0, 0x1FAD6))
                    {
                        // 이모지
                        i+=2;
                    }
                    else
                    {
                        // 이모지 아님
                        outStr[o++] = inStr[i++];
                        outStr[o++] = inStr[i++];
                    }
                }
                else
                {
                    outStr[o++] = inStr[i++];
                }
            }
            else if (length2 && ls == 0x20E3)
            {
                // 이모지
                i += 2;
            }
            else
            {
                if (
                    // Latin-1 Supplement
                    hs == 0x00A9 || hs == 0x00AE
                    // General Punctuation
                    || hs == 0x203C || hs == 0x2049
                    // Letterlike Symbols
                    || hs == 0x2122 || hs == 0x2139
                    // Arrows
                    || IS_IN(hs, 0x2194, 0x2199) || IS_IN(hs, 0x21A9, 0x21AA)
                    // Miscellaneous Technical
                    || IS_IN(hs, 0x231A, 0x231B) || IS_IN(hs, 0x23E9, 0x23F3) || IS_IN(hs, 0x23F8, 0x23FA) || hs == 0x2328 || hs == 0x23CF
                    // Geometric Shapes
                    || IS_IN(hs, 0x25AA, 0x25AB) || IS_IN(hs, 0x25FB, 0x25FE) || hs == 0x25B6 || hs == 0x25C0
                    // Miscellaneous Symbols
                    || IS_IN(hs, 0x2600, 0x2604) || IS_IN(hs, 0x2614, 0x2615) || IS_IN(hs, 0x2622, 0x2623) || IS_IN(hs, 0x262E, 0x262F)
                    || IS_IN(hs, 0x2638, 0x263A) || IS_IN(hs, 0x2648, 0x2653) || IS_IN(hs, 0x2665, 0x2666) || IS_IN(hs, 0x2692, 0x2694)
                    || IS_IN(hs, 0x2696, 0x2697) || IS_IN(hs, 0x269B, 0x269C) || IS_IN(hs, 0x26A0, 0x26A1) || IS_IN(hs, 0x26AA, 0x26AB)
                    || IS_IN(hs, 0x26B0, 0x26B1) || IS_IN(hs, 0x26BD, 0x26BE) || IS_IN(hs, 0x26C4, 0x26C5) || IS_IN(hs, 0x26CE, 0x26CF)
                    || IS_IN(hs, 0x26D3, 0x26D4) || IS_IN(hs, 0x26D3, 0x26D4) || IS_IN(hs, 0x26E9, 0x26EA) || IS_IN(hs, 0x26F0, 0x26F5)
                    || IS_IN(hs, 0x26F7, 0x26FA)
                    || hs == 0x260E || hs == 0x2611 || hs == 0x2618 || hs == 0x261D || hs == 0x2620 || hs == 0x2626 || hs == 0x262A
                    || hs == 0x2660 || hs == 0x2663 || hs == 0x2668 || hs == 0x267B || hs == 0x267F || hs == 0x2699 || hs == 0x26C8
                    || hs == 0x26D1 || hs == 0x26FD
                    // Dingbats
                    || IS_IN(hs, 0x2708, 0x270D) || IS_IN(hs, 0x2733, 0x2734) || IS_IN(hs, 0x2753, 0x2755)
                    || IS_IN(hs, 0x2763, 0x2764) || IS_IN(hs, 0x2795, 0x2797)
                    || hs == 0x2702 || hs == 0x2705 || hs == 0x270F || hs == 0x2712 || hs == 0x2714 || hs == 0x2716 || hs == 0x271D
                    || hs == 0x2721 || hs == 0x2728 || hs == 0x2744 || hs == 0x2747 || hs == 0x274C || hs == 0x274E || hs == 0x2757
                    || hs == 0x27A1 || hs == 0x27B0 || hs == 0x27BF
                    // CJK Symbols and Punctuation
                    || hs == 0x3030 || hs == 0x303D
                    // Enclosed CJK Letters and Months
                    || hs == 0x3297 || hs == 0x3299
                    // Supplemental Arrows-B
                    || IS_IN(hs, 0x2934, 0x2935)
                    // Miscellaneous Symbols and Arrows
                    || IS_IN(hs, 0x2B05, 0x2B07) || IS_IN(hs, 0x2B1B, 0x2B1C) || hs == 0x2B50 || hs == 0x2B55
                )
                {
                    // 이모지
                    i++;
                }
                else
                {
                    // 이모지 아님
                    outStr[o++] = inStr[i++];
                }
            }
        }

#if MDEBUG
        string temp = "";
        for (int i = 0; i < o; i++)
        {
            temp = temp + "0x" + ((int)outStr[i]).ToString("X") + " ";
        }
        Debug.Log("[InputField] " + temp);
#endif
        inputField.text = new string(outStr, 0, o);
    }

    public void OnInputFieldEditEnd()
    {
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;
using Haegin;
using HaeginGame;
using System.Collections.Generic;

public class IAPSetup : Singleton<IAPSetup> {

    private Dictionary<string, string> productDictionary = 
        new Dictionary<string, string>{{"gold_1", "com_haegin_hvso_fistful_of_gold"},           //금화 한줌
                                        {"gold_2", "com_haegin_hvso_pocket_of_gold"},           //금화 주머니
                                        {"gold_3", "com_haegin_hvso_sack_of_gold"},             //금화 자루
                                        {"gold_4", "com_haegin_hvso_chest_of_gold"},            //금화 상자
                                        {"gold_5", "com_haegin_hvso_cart_of_gold"},             //금화 수레
                                        {"gold_6", "com_haegin_hvso_pile_of_gold"},             //금화 더미
                                        {"welcome_1", "com_haegin_hvso_welcome_package_1"},     //웰컴 패키지1
                                        {"welcome_2", "com_haegin_hvso_welcome_package_2"},     //웰컴 패키지2
                                        {"welcome_3", "com_haegin_hvso_welcome_package_3"},     //웰컴 패키지3
                                        {"hero_2", "com_haegin_hvso_hero_2lv_done_pakage"},     //영웅 2레벨 달성 패키지
                                        {"hero_3", "com_haegin_hvso_hero_3lv_done_pakage"},     //영웅 3레벨 달성 패키지
                                        {"beginner", "com_haegin_hvso_beginner_package"},       //초심자 패키지
                                        {"intermediate", "com_haegin_hvso_intermediate_package"},//중급자 패키지
                                        {"expert", "com_haegin_hvso_expert_package"},           //상급자 패키지
                                        {"master", "com_haegin_hvso_master_package"},           //최상급자 패키지
                                        {"strategist", "com_haegin_hvso_strategist_package"},   //전략가 패키지
                                        {"legend", "com_haegin_hvso_legend_package"},           //전설 패키지
                                        {"emoticon_1", "com_haegin_hvso_emoticon_package_1"},   //이모티콘 패키지1
                                        {"emoticon_2", "com_haegin_hvso_emoticon_package_2"},   //이모티콘 패키지2
                                        {"optain_hero_1", "com_haegin_hvso_obtain_hero_package_1"},//영웅 획득 패키지1
                                        {"optain_hero_2", "com_haegin_hvso_obtain_hero_package_2"}};//영웅 획득 패키지2
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject systemDialog;
    [SerializeField] private GameObject eulaText;
    private WebClient webClient;
    private List<IAPProduct> productList;

    public void Init() {
        WebClientInit();
        IAPInit();
    }

    private void WebClientInit() {
        webClient = WebClient.GetInstance();

        webClient.ErrorOccurred += OnErrorOccurred;
        webClient.Processing += OnProcessing;
        webClient.RetryOccurred += RetryOccurred;
        webClient.RetryFailed += RetryFailed;
        webClient.MaintenanceStarted += OnMaintenanceStarted;
        webClient.Logged += (string log) => {
            #if MDEBUG
            Debug.Log("Unity   " + log);
            #endif
        };
    }

    private void IAPInit() {
        string[] products = productDictionary.Values.ToArray();
        #if !UNITY_EDITOR
        IAP.init(products, (bool result, bool bProcessIAP, byte[] purchasedData) => {
            if(result) {
                #if MDEBUG
                Debug.Log("call requestProductData");
                #endif
                if (bProcessIAP) {
                    // 구매과정에서 지급되지 않았다가 재실행시 지급된 아이템 정보
                    // 
                    Debug.Log("지급되지 않았던 구매 아이템이 있었습니다. 정상적으로 지급 처리 되었습니다.");
                }

                IAP.requestProductData(products, productList => {
                    //가격 세팅
                    // Text textProductInfo = GameObject.Find("ProductInfoLabel").GetComponent<Text>();
                    // textProductInfo.text = TextManager.GetString(TextManager.StringTag.NameTag) + productList[0].title + "\n" +
                    //     TextManager.GetString(TextManager.StringTag.DescTag) + productList[0].description + "\n" +
                    //     TextManager.GetString(TextManager.StringTag.PriceTag) + productList[0].price;
                    #if MDEBUG
                    foreach(IAPProduct info in productList) {
                        Debug.Log(info.title + " " + info.description + " " + info.price);
                    }
                    #endif
                    this.productList = productList;
                });
            }
            else {
                Debug.Log("인앱 초기화에 실패했습니다.");
            }
        });
        #endif
    }

    void RetryOccurred(Protocol protocol, int retryCount) {
        #if MDEBUG
        Debug.Log("Retry Occurred  " + retryCount);
        #endif
    }

    void RetryFailed(Protocol protocol) {
        OnNetworkError();
    }

    void OnNetworkError() {
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.NetworkError), TextManager.GetString(TextManager.StringTag.NetworkErrorMessage), (UGUICommon.ButtonType buttonType) => {
            if (buttonType == UGUICommon.ButtonType.Ok) {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnMaintenanceStarted() {
        // 메인터넌스가 시작되었다.
        UGUICommon.ShowMessageDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.ServerMaintenanceTitle), TextManager.GetString(TextManager.StringTag.ServerMaintenance), (UGUICommon.ButtonType buttonType) => {
            if (buttonType == UGUICommon.ButtonType.Ok) {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnProcessing(ReqAndRes rar){ }

    public void OnErrorOccurred(int error) {
        OnNetworkError();
    }

    public void OnDestory() {
        if (webClient == null) return;
        webClient.ErrorOccurred -= OnErrorOccurred;
        webClient.Processing -= OnProcessing;
        webClient.RetryOccurred -= RetryOccurred;
        webClient.RetryFailed -= RetryFailed;
        webClient.MaintenanceStarted -= OnMaintenanceStarted;
    }

    public void OnButtonBuyClick(string productId, UnityAction callback) {
        #if MDEBUG
        Debug.Log(productId);
        Debug.Log(productDictionary[productId]);
        #endif

        IAP.purchaseProduct(productDictionary[productId] , (result, purchasedData, errorMsg) => {
            switch (result) {
                case IAP.PurchaseResultCode.PURCHASE_SUCCESS:
                    //GameObject.Find("PurchaseLabel").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.Purchased);
                    #if MDEBUG
                    Debug.Log("Unity : item purchased..");
                    #endif
                    // 데이터 갱신
                    //
                    // purchasedData 에 byte[] 로 구매된 게임내 아이템 정보가 넘어옵니다. 이 정보를 이용해서 업데이트 하시면 되겠습니다. 
                    //
                    callback();
                    if (purchasedData != null) {
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
                    Modal.instantiate("구매 실패했습니다", Modal.Type.CHECK);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_USER_CANCEL:
                    Modal.instantiate("구매 취소됐습니다", Modal.Type.CHECK);
                    break;
                case IAP.PurchaseResultCode.PURCHASE_INVALID_RECEIPT:
                    Modal.instantiate("영수증 검증 실패했습니다", Modal.Type.CHECK);
                    break;
            }
        });
    }
}
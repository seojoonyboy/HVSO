using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ShopManager : MonoBehaviour
{
    [SerializeField] BoxRewardManager boxRewardManager;
    [SerializeField] Transform AdvertiseWindow;
    [SerializeField] Transform ProductWindow;
    [SerializeField] Transform[] supplyBoxes;
    [SerializeField] Transform levelUpPackageWindow;
    private IAPSetup iapSetup;

    int goldItemCount;
    int x2couponCount;
    int supplyBoxCount;
    int packageCount;
    bool buyBox = false;
    bool buying = false;
    string adRewardKind;
    int supplyBoxNum;
    string selectedBox;

    private void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_BUY, OpenBoxByBuying);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, BuyFinished) ;
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_AD_SHOPLIST, SetAdWindow);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_SHOP, OpenAdRewardWindow);
        iapSetup = IAPSetup.Instance;
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_BUY, OpenBoxByBuying);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, BuyFinished);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_AD_SHOPLIST, SetAdWindow);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_SHOP, OpenAdRewardWindow);
        iapSetup = null;
    }

    GameObject checkModal;

    public void RefreshLine() {
        Canvas.ForceUpdateCanvases();
        GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        Invoke("UpdateContentHeight", 0.25f);
    }

    public void SetShop() {
        var mainSceneStateHandler = MainSceneStateHandler.Instance;
        if (mainSceneStateHandler.GetState("IsTutorialFinished")) {
            for (int i = 0; i < transform.Find("ShopWindowParent/ShopWindow/PackageShop/ItemList").childCount; i++)
                transform.Find("ShopWindowParent/ShopWindow/PackageShop/ItemList").GetChild(i).gameObject.SetActive(false);
            goldItemCount = 0;
            x2couponCount = 0;
            supplyBoxCount = 0;
            packageCount = 0;
            
            foreach (dataModules.Shop item in AccountManager.Instance.shopItems) {
                switch (item.category) {
                    case "gold":
                        SetGoldItem(item);
                        break;
                    case "package":
                        SetPackage(item);
                        break;
                    case "x2coupon":
                        SetCouponItem(item);
                        break;
                    case "supplyBox":
                        //SetBoxItem(item);
                        break;
                    default:
                        Logger.Log(item.category + "doesn't exist now");
                        break;
                }
            }
            supplyBoxNum = 1;
            SetSupplyBoxPrice("box_reinforced");
            transform.Find("ShopWindowParent/ShopWindow/Supply2XCouponShop/haveCouponNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text
                = AccountManager.Instance.userData.supplyX2Coupon.ToString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.Find("ShopWindowParent/ShopWindow/PackageShop/ItemList").GetComponent<RectTransform>());
            transform.Find("ShopWindowParent/ShopWindow/PackageShop").GetComponent<RectTransform>().sizeDelta
                = new Vector2(100, transform.Find("ShopWindowParent/ShopWindow/PackageShop/ItemList").GetComponent<RectTransform>().rect.height + 40);
            transform.gameObject.SetActive(false);
            transform.gameObject.SetActive(true);
        }
    }

    public void SetSupplyBoxPrice(string box) {
        selectedBox = box;
        for (int i = 0; i < 3; i++) 
                supplyBoxes[i].Find("Selected").gameObject.SetActive(supplyBoxes[i].name == box);
        transform.Find("ShopWindowParent/ShopWindow/SupplyBoxShop/ShopParent/Number/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + supplyBoxNum;
        dataModules.Shop serachedItem = AccountManager.Instance.shopItems.Find(item => item.id.CompareTo(box + "_" + supplyBoxNum) == 0);
        transform.Find("ShopWindowParent/ShopWindow/SupplyBoxShop/ShopParent/BuyButton/Price").GetComponent<TMPro.TextMeshProUGUI>().text = serachedItem.prices.GOLD.ToString();
        Button buyBtn = transform.Find("ShopWindowParent/ShopWindow/SupplyBoxShop/ShopParent/BuyButton").GetComponent<Button>();
        buyBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.AddListener(() => PopBuyModal(serachedItem, true));
    }

    public void SetSupplyBoxNum(bool up) {
        switch (supplyBoxNum) {
            case 1:
                if (up) supplyBoxNum = 5;
                break;
            case 5:
                if (up) supplyBoxNum = 10;
                else supplyBoxNum = 1;
                break;
            case 10:
                if (!up) supplyBoxNum = 5;
                break;
        }
        SetSupplyBoxPrice(selectedBox);
    }

    public void SetBoxItem(dataModules.Shop item) {
        Button buyBtn;
        switch (item.items[0].kind) {
            case "reinforcedBox":
                buyBtn = transform.Find("ShopWindowParent/ShopWindow/SupplyBoxShop/ItemList/reinforcedBox/BuyButtons/" + item.items[0].amount.ToString()).GetComponent<Button>();
                break;
            case "largeBox":
                buyBtn = transform.Find("ShopWindowParent/ShopWindow/SupplyBoxShop/ItemList/largeBox/BuyButtons/" + item.items[0].amount.ToString()).GetComponent<Button>();
                break;
            case "extraLargeBox":
                buyBtn = transform.Find("ShopWindowParent/ShopWindow/SupplyBoxShop/ItemList/extraLargeBox/BuyButtons/" + item.items[0].amount.ToString()).GetComponent<Button>();
                break;
            default:
                buyBtn = transform.Find("ShopWindowParent/ShopWindow/SupplyBoxShop/ItemList/reinforcedBox/BuyButtons/" + item.items[0].amount.ToString()).GetComponent<Button>();
                break;
        }
        buyBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.AddListener(() => PopBuyModal(item, true));
    }

    public void SetGoldItem(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/GoldShop/GoldContent").GetChild(goldItemCount);
        target.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = "\\ " + item.prices.KRW.ToString();
        target.Find("Value/goldPaid").GetComponent<TMPro.TextMeshProUGUI>().text = item.items[0].amount;
        if (item.items.Length > 1) {
            target.Find("Value/Plus").gameObject.SetActive(true);
            target.Find("Value/goldFree").gameObject.SetActive(true);
            target.Find("Value/goldFree").GetComponent<TMPro.TextMeshProUGUI>().text = item.items[1].amount.ToString();
        }
        else {
            target.Find("Value/Plus").gameObject.SetActive(false);
            target.Find("Value/goldFree").gameObject.SetActive(false);
        }
        target.GetComponent<Button>().onClick.RemoveAllListeners();
        target.GetComponent<Button>().onClick.AddListener(() => PopBuyModal(item));
        goldItemCount++;
    }

    public void SetCouponItem(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/Supply2XCouponShop").GetChild(x2couponCount);
        target.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = item.prices.GOLD.ToString();
        target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = item.items[0].amount;
        target.GetComponent<Button>().onClick.RemoveAllListeners();
        target.GetComponent<Button>().onClick.AddListener(() => PopBuyModal(item));
        x2couponCount++;
    }

    public void SetPackage(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/PackageShop/ItemList").GetChild(packageCount);
        target.GetComponent<Button>().onClick.RemoveAllListeners();
        target.GetComponent<Button>().onClick.AddListener(() => OpenProductWindow(item));
        target.gameObject.SetActive(true);
        if (item.id.Contains("welcome")) {
            target.GetComponent<Image>().sprite = AccountManager.Instance.resource.packageImages["welcome"];
            target.Find("PackageImage").GetComponent<Image>().sprite = AccountManager.Instance.resource.packageImages[item.id];
            target.Find("PackageText/TypeText").GetComponent<TMPro.TextMeshProUGUI>().text = "Welcome";
        }
        target.Find("BuyButton/Price").GetComponent<TMPro.TextMeshProUGUI>().text = "\\" + item.prices.KRW.ToString();
        int itemNum = 0;
        for (int i = 0; i < target.Find("Items").childCount; i++)
            target.Find("Items").GetChild(i).gameObject.SetActive(false);
        for (int i = 0; i < item.items.Length; i++) {
            if (item.items[i].kind.Contains("gold")) {
                Transform slot = target.Find("Items/Gold");
                slot.gameObject.SetActive(true);
                slot.GetComponent<TMPro.TextMeshProUGUI>().text = "금화 x" + item.items[i].amount.ToString();
            }
            else {
                Transform slot = target.Find("Items/" + itemNum.ToString());
                itemNum++;
                slot.gameObject.SetActive(true);
                if (item.items[i].kind.Contains("Coupon"))
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "x2쿠폰 x" + item.items[i].amount;
                else if(item.items[i].kind == "reinforcedBox")
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "보강된 보급상자 x" + item.items[i].amount;
                else if (item.items[i].kind == "largeBox")
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "대형 보급상자 x" + item.items[i].amount;
                else if (item.items[i].kind == "extraLargeBox")
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "초대형 보급상자 x" + item.items[i].amount;
            }
        }
        packageCount++;
    }

    // public void SetSupplyBoxItem(dataModules.Shop item) {
    //     Transform target = transform.Find("ShopWindowParent/ShopWindow/SuppluBoxShop").GetChild(supplyBoxCount);
    //     target.Find("Button/Price").GetComponent<TMPro.TextMeshProUGUI>().text = item.price.ToString();
    //     target.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();
    //     target.Find("Button").GetComponent<Button>().onClick.AddListener(() => PopBuyModal(item, true));
    //     supplyBoxCount++;
    // }

    public void PopBuyModal(dataModules.Shop item, bool isBox = false) {
        if (buying) return;
        if (!item.isRealMoney) {
            if (item.prices.GOLD <= AccountManager.Instance.userResource.gold) {
                checkModal = Modal.instantiate("상품을 구매 하시겠습니까?", Modal.Type.YESNO, () => {
                    BuyItem(item.id, isBox);
                }, CancelBuy);
            }
            else {
                checkModal = Modal.instantiate("재화가 부족합니다.", Modal.Type.CHECK, () => {
                    CancelBuy();
                });
            }
        }
        else {
            checkModal = Modal.instantiate("상품을 구매 하시겠습니까?", Modal.Type.YESNO, () => {
                #if UNITY_EDITOR
                BuyItem(item.id, isBox, new Haegin.PurchasedInfo());
                #else
                iapSetup.InAppPurchaseClick(item.id, (purchasedInfo)=>BuyItem(item.id, isBox, purchasedInfo));
                #endif
            }, CancelBuy);
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(CancelBuy);
    }

    public void CancelBuy() {
        DestroyImmediate(checkModal, true);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CancelBuy);
    }

    public void BuyItem(string itemId, bool isBox = false, Haegin.PurchasedInfo purchasedInfo = null) {
        buying = true;
        AccountManager.Instance.RequestBuyItem(itemId, purchasedInfo);
        buyBox = isBox;
    }

    public void OpenBoxByBuying(Enum Event_Type, Component Sender, object Param) {
        if (buyBox) {
            List<List<RewardClass>> rewards = new List<List<RewardClass>>();
            for (int i = 0; i < AccountManager.Instance.buyBoxInfo.items[0].amount; i++) 
                rewards.Add(AccountManager.Instance.buyBoxInfo.items[0].boxes[i]);
            boxRewardManager.OpenMultipleBoxes(rewards);
        }
        else
            Modal.instantiate("구매하신 상품이 우편함으로 보내졌습니다.", Modal.Type.CHECK);
        AccountManager.Instance.RequestUserInfo();
    }
    public void BuyFinished(Enum Event_Type, Component Sender, object Param) {
        buying = false;
        transform.Find("ShopWindowParent/ShopWindow/Supply2XCouponShop/haveCouponNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text
                = AccountManager.Instance.userData.supplyX2Coupon.ToString();
    }

    public void OpenAdvertiseList() {
        AdvertiseWindow.gameObject.SetActive(true);
        AdvertiseWindow.Find("Block").gameObject.SetActive(true);
        AccountManager.Instance.RequestShopAds();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseAdvertiseList);
    }
    public void CloseAdvertiseList() {
        AdvertiseWindow.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseAdvertiseList);
    }

    public void SetAdWindow(Enum Event_Type, Component Sender, object Param) {
        AdvertiseWindow.Find("Block").gameObject.SetActive(false);
        Transform adBtnList = AdvertiseWindow.Find("Ads");
        dataModules.AdReward[] ads = AccountManager.Instance.shopAdsList;
        bool open = false;
        for(int i = 0; i < ads.Length; i++) {
            if (!ads[i].claimed && !open) {
                open = true;
                adBtnList.GetChild(i).Find("Block").gameObject.SetActive(false);
                adBtnList.GetChild(i).Find("Resource/Block").gameObject.SetActive(false);
            }
            else {
                adBtnList.GetChild(i).Find("Resource/Block").gameObject.SetActive(true);
            }
        }
        transform.Find("ShopWindowParent/ShopWindow/FreeItems/Frame/NewAds").gameObject.SetActive(!ads[4].claimed);
    }

    public void OpenAdRewardWindow(Enum Event_Type, Component Sender, object Param) {
        if (!AccountManager.Instance.adRewardResult.claimComplete) return;
        Transform rewardWindow = AdvertiseWindow.Find("Reward");
        rewardWindow.gameObject.SetActive(true);
        adRewardKind = AccountManager.Instance.adRewardResult.items[0].kind;
        if (adRewardKind.Contains("Box"))
            adRewardKind = "supplyBox";
        rewardWindow.Find("Resource").GetComponent<Image>().sprite 
            = AccountManager.Instance.resource.rewardIcon["ad_" + adRewardKind];        
        string resourceName = "";
        switch (adRewardKind) {
            case "crystal":
                resourceName = "마력 수정";
                break;
            case "x2Coupon":
                resourceName = "2배 쿠폰";
                break;
            case "supplyBox":
                resourceName = "보급 상자";
                break;
        }
        rewardWindow.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = resourceName;
        rewardWindow.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + AccountManager.Instance.adRewardResult.items[0].amount.ToString();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseRewardWindow);
    }
    public void CloseRewardWindow() {
        AdvertiseWindow.Find("Block").gameObject.SetActive(true);
        AdvertiseWindow.Find("Reward").gameObject.SetActive(false);
        if (adRewardKind == "supplyBox") 
            boxRewardManager.SetRewardBoxAnimation(AccountManager.Instance.adRewardResult.items[0].boxes[0].ToArray());
        AccountManager.Instance.RequestShopAds();
        AccountManager.Instance.RequestUserInfo();
        AccountManager.Instance.RequestInventories();
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseRewardWindow);
    }


    public void OpenProductWindow(dataModules.Shop item) {
        ProductWindow.gameObject.SetActive(true);
        ProductWindow.Find("ProductName/Text").GetComponent<TMPro.TextMeshProUGUI>().text = item.name;
        ProductWindow.Find("ProductText/Text").GetComponent<TMPro.TextMeshProUGUI>().text = item.desc;
        ProductWindow.Find("ProductInfo/Valuablity/Value").GetComponent<TMPro.TextMeshProUGUI>().text = item.valuablity;
        ProductWindow.Find("ProductInfo/ProductImage").GetComponent<Image>().sprite = AccountManager.Instance.resource.packageImages[item.id];
        ProductWindow.Find("BuyBtn/PriceText").GetComponent<TMPro.TextMeshProUGUI>().text = "\\" + item.prices.KRW.ToString();
        ProductWindow.Find("BuyBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        ProductWindow.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(() => PopBuyModal(item));
        int itemNum = 0;
        for (int i = 0; i < ProductWindow.Find("ProductInfo/Items").childCount; i++)
            ProductWindow.Find("ProductInfo/Items").GetChild(i).gameObject.SetActive(false);
        for (int i = 0; i < item.items.Length; i++) {
            if (item.items[i].kind.Contains("gold")) {
                Transform slot = ProductWindow.Find("ProductInfo/Items/Gold");
                slot.gameObject.SetActive(true);
                slot.GetComponent<TMPro.TextMeshProUGUI>().text = "금화 x" + item.items[i].amount.ToString();
            }
            else {
                Transform slot = ProductWindow.Find("ProductInfo/Items/" + itemNum.ToString());
                itemNum++;
                slot.gameObject.SetActive(true);
                if (item.items[i].kind.Contains("Coupon"))
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "x2쿠폰 x" + item.items[i].amount;
                else if (item.items[i].kind == "reinforcedBox")
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "보강된 보급상자 x" + item.items[i].amount;
                else if (item.items[i].kind == "largeBox")
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "대형 보급상자 x" + item.items[i].amount;
                else if (item.items[i].kind == "extraLargeBox")
                    slot.GetComponent<TMPro.TextMeshProUGUI>().text = "초대형 보급상자 x" + item.items[i].amount;
            }
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseProductWindow);
    }
    public void CloseProductWindow() {
        ProductWindow.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseProductWindow);
    }

    public void OpenLevelUpPackageWindow() {
        levelUpPackageWindow.gameObject.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseLevelUpPackageWindow);
    }

    public void CloseLevelUpPackageWindow() {
        levelUpPackageWindow.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseLevelUpPackageWindow);
    }
}

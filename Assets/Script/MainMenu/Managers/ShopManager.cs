using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] BoxRewardManager boxRewardManager;

    int goldItemCount;
    int x2couponCount;
    int supplyBoxCount;
    bool buyBox = false;
    bool buying = false;
    private void Start() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_BUY, OpenBoxByBuying);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, BuyFinished) ;
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_BUY, OpenBoxByBuying);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, BuyFinished);
    }

    GameObject checkModal;


    public void SetShop() { 
        goldItemCount = 0;
        x2couponCount = 0;
        supplyBoxCount = 0;
        foreach(dataModules.Shop item in AccountManager.Instance.shopItems) {
            switch (item.category) {
                case "gold":
                    SetGoldItem(item);
                    break;
                case "supplyBox":
                    SetSupplyBoxItem(item);
                    break;
                case "x2coupon":
                    SetCouponItem(item);
                    break;
            }
        }
    }

    public void SetGoldItem(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/GoldShop/GoldContent").GetChild(goldItemCount);
        target.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = "\\ " + item.price.ToString();
        target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = item.items[0].amount;
        if (item.items.Length > 1) {
            target.Find("FreeGold").gameObject.SetActive(true);
            target.Find("FreeGold").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + item.items[1].amount.ToString();
        }
        else
            target.Find("FreeGold").gameObject.SetActive(false);
        target.GetComponent<Button>().onClick.RemoveAllListeners();
        target.GetComponent<Button>().onClick.AddListener(() => PopBuyModal(item));
        goldItemCount++;
    }

    public void SetCouponItem(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/Supply2XCouponShop").GetChild(x2couponCount);
        target.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = item.price.ToString();
        target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = item.items[0].amount;
        target.GetComponent<Button>().onClick.RemoveAllListeners();
        target.GetComponent<Button>().onClick.AddListener(() => PopBuyModal(item));
        x2couponCount++;
    }

    public void SetSupplyBoxItem(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/SuppluBoxShop").GetChild(supplyBoxCount);
        target.Find("Button/Price").GetComponent<TMPro.TextMeshProUGUI>().text = item.price.ToString();
        target.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();
        target.Find("Button").GetComponent<Button>().onClick.AddListener(() => PopBuyModal(item, true));
        supplyBoxCount++;
    }

    public void PopBuyModal(dataModules.Shop item, bool isBox = false) {
        if (buying) return;
        if (item.category == "supplyBox") {
            if (item.price <= AccountManager.Instance.userResource.gold) {
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
                BuyItem(item.id, isBox);
            }, CancelBuy);
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(CancelBuy);
    }

    public void CancelBuy() {
        DestroyImmediate(checkModal, true);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CancelBuy);
    }

    public void BuyItem(string itemId, bool isBox = false) {
        buying = true;
        AccountManager.Instance.RequestBuyItem(itemId);
        buyBox = isBox;
    }

    public void OpenBoxByBuying(Enum Event_Type, Component Sender, object Param) {
        if(buyBox)
            boxRewardManager.OpenBox();
        AccountManager.Instance.RequestUserInfo();
    }
    public void BuyFinished(Enum Event_Type, Component Sender, object Param) {
        buying = false;
    }
}

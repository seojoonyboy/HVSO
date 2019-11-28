using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] BoxRewardManager boxRewardManager;

    int goldItemCount;
    int x2couponCount;
    int supplyBoxCount;
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
                    break;
                case "x2coupon":
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
        goldItemCount++;
    }

    public void SetCouponItem(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/Supply2XCouponShop").GetChild(x2couponCount);
        target.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = item.price.ToString();
        target.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = item.items[0].amount;
        x2couponCount++;
    }

    public void SetSupplyBoxItem(dataModules.Shop item) {
        Transform target = transform.Find("ShopWindowParent/ShopWindow/SuppluBoxShop").GetChild(supplyBoxCount);
        target.Find("Price").GetComponent<TMPro.TextMeshProUGUI>().text = item.price.ToString();
        supplyBoxCount++;
    }

    public void BuySmallBox() {
        boxRewardManager.OpenBox();
    }
}

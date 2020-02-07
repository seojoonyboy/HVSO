using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsController : MonoBehaviour {
    private AdsManager adsManager;

    public void Awake() {
        adsManager = AdsManager.Instance;
    }

    public void ShowRewardedMainBtn() {
        adsManager.ShowRewardedBtn("main");
    }

    public void ShowRewardedShopBtn() {
        adsManager.ShowRewardedBtn("shop");
    }

    public void ShowRewardedChestBtn() {
        adsManager.ShowRewardedBtn("chest");
    }

    public void ShowRewardedShopChestBtn() {
        adsManager.ShowRewardedBtn("shop_chest");
    }
}

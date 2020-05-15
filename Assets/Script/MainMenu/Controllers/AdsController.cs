using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsController : MonoBehaviour {
    private AdsManager adsManager;

    public void Awake() {
        adsManager = AdsManager.Instance;
    }

    public void ShowRewardedMainBtn() {
        if (Input.touchCount > 1) return;
        adsManager.ShowRewardedBtn("main");
    }

    public void ShowRewardedShopBtn() {
        if (Input.touchCount > 1) return;
        adsManager.ShowRewardedBtn("shop");
    }

    public void ShowRewardedChestBtn() {
        if (Input.touchCount > 1) return;
        adsManager.ShowRewardedBtn("chest");
    }

    public void ShowRewardedShopChestBtn() {
        if (Input.touchCount > 1) return;
        adsManager.ShowRewardedBtn("shop_chest");
    }
}

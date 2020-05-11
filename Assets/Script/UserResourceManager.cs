using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]

public class UserResourceManager : SerializedMonoBehaviour {
    // Start is called before the first frame update
    public uint lv;
    public uint exp;
    public uint lvExp;
    public uint nextLvExp;

    public int gold;
    public int crystal;
    public int supplyStore;
    public float suppllyTimerTime;
    public float adsTimerTime;
    public int supplyStoreTime;
    public int mainAdTimeRemain;
    public int mainAdCount;
    public string supplyStoreTimer;
    public string mainAdTimer;
    public int supply;
    public int supplyBox;
    public int supplyX2Coupon;

    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI adsTimerText;
    public TMPro.TextMeshProUGUI adsLeftNum;
    public bool timerOn;

    private void Start() {
    }
    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_MAIN, OnMainAdOver);
    }

    public void SetResource(uint lv,
                            uint exp,
                            uint lvExp,
                            uint nextLvExp, 
                            int gold, 
                            int crystal, 
                            int supplyStore, 
                            int supplyStoreTime, 
                            int mainAdTimeRemain, 
                            int mainAdCount, 
                            int supply, 
                            int supplyBox, 
                            int supplyX2Coupon) {
        timerOn = false;
        this.lv = lv;
        this.exp = exp;
        this.lvExp = lvExp;
        this.nextLvExp = nextLvExp;
        this.gold = gold;
        this.crystal = crystal;
        this.supplyStore = supplyStore;
        SetSupplyTimer(supplyStoreTime);
        SetMainAdsTimer(mainAdTimeRemain);
        this.mainAdCount = mainAdCount;
        if (adsLeftNum != null) {
            adsLeftNum.text = this.mainAdCount.ToString();
            if (this.mainAdCount == 0)
                adsLeftNum.transform.parent.parent.Find("ReceiveBtn").GetComponent<Button>().interactable = false;
            else
                adsLeftNum.transform.parent.parent.Find("ReceiveBtn").GetComponent<Button>().interactable = true;
            if (timerText != null)
                timerText.transform.Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = mainAdCount + "/3";
        }
        this.supply = supply;
        this.supplyBox = supplyBox;
        this.supplyX2Coupon = supplyX2Coupon;
        timerOn = true;
    }

    void Update() {
        //if (supplyStore == 200) return;
        if (!timerOn) return;
        if (timerText != null) {
            if (supplyStore < 200) {
                suppllyTimerTime -= Time.deltaTime;
                if (suppllyTimerTime <= 0) {
                    AccountManager.Instance.RequestUserInfo();
                    timerOn = false;
                    return;
                }
                if ((supplyStoreTime * 0.001f) - suppllyTimerTime >= 1) {
                    SetSupplyTimer(supplyStoreTime - 1000);
                }
            }
        }
        if (adsTimerText != null && adsTimerText.transform.parent.parent.gameObject.activeSelf) {
            if (mainAdCount != 3) {
                adsTimerTime -= Time.deltaTime;
                if (adsTimerTime <= 0) {
                    AccountManager.Instance.RequestUserInfo();
                    timerOn = false;
                    return;
                }
                if ((mainAdTimeRemain * 0.001f) - adsTimerTime >= 1) {
                    SetMainAdsTimer(mainAdTimeRemain - 1000);
                }
            }
        }
    }

    public void LinkTimer(TMPro.TextMeshProUGUI timerText, Transform adsWindow) {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_MAIN, OnMainAdOver);
        this.timerText = timerText;
        adsTimerText = adsWindow.Find("Timer/TimerText").GetComponent<TMPro.TextMeshProUGUI>();
        adsLeftNum = adsWindow.Find("Timer/LeftValue").GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void SetSupplyTimer(int supplyStoreTime) {
        TimeSpan time = TimeSpan.FromMilliseconds(supplyStoreTime);
        
        this.supplyStoreTime = supplyStoreTime;
        suppllyTimerTime = supplyStoreTime * 0.001f;
        if (suppllyTimerTime > 0) {
            supplyStoreTimer = time.Hours.ToString() + ":" + time.Minutes.ToString() + ":" + time.Seconds.ToString() + "후 +60";
        }
        else {
            //string fullText = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_battle_supplyready");
            //supplyStoreTimer = fullText;
            var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
            string localizedText = translator.GetLocalizedText("MainUI", "ui_page_battle_supplyready");
            supplyStoreTimer = localizedText;
        }
        if (timerText != null) 
            timerText.text = supplyStoreTimer;
    }

    public void SetMainAdsTimer(int mainAdTimeRemain) {
        TimeSpan time = TimeSpan.FromMilliseconds(mainAdTimeRemain);

        this.mainAdTimeRemain = mainAdTimeRemain;
        adsTimerTime = mainAdTimeRemain * 0.001f;
        if (adsTimerTime > 0) {
            mainAdTimer = time.Hours.ToString() + ":" + time.Minutes.ToString() + ":" + time.Seconds.ToString();
        }
        else
            mainAdTimer = "00:00:00";
        if (adsTimerText != null)
            adsTimerText.text = mainAdTimer;
    }
    public void OnMainAdOver(Enum Event_Type, Component Sender, object Param) {
        AccountManager.Instance.RequestUserInfo();
        GameObject checkModal = Modal.instantiate(AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_main_adrewardgetcheck"), Modal.Type.CHECK);
    }

}

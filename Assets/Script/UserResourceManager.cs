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
    public float timerTime;
    public int supplyStoreTime;
    public string supplyStoreTimer;
    public int supply;
    public int supplyBox;
    public int additionalPreSupply;

    public TMPro.TextMeshProUGUI timerText;
    public bool timerOn;

    public void SetResource(uint lv, uint exp, uint lvExp, uint nextLvExp, int gold, int crystal, int supplyStore, int supplyStoreTime, int supply, int supplyBox, int additionalPreSupply) {
        this.lv = lv;
        this.exp = exp;
        this.lvExp = lvExp;
        this.nextLvExp = nextLvExp;
        this.gold = gold;
        this.crystal = crystal;
        this.supplyStore = supplyStore;
        SetTimer(supplyStoreTime);
        this.supply = supply;
        this.supplyBox = supplyBox;
        this.additionalPreSupply = additionalPreSupply;
        timerOn = true;
    }

    void Update() {
        if (supplyStore == 200) return;
        if (!timerOn) return;
        if (timerText != null) {
            timerTime -= Time.deltaTime;
            if(timerTime <= 0) {
                AccountManager.Instance.RequestUserInfo();
                timerOn = false;
                return;
            }
            if ((supplyStoreTime * 0.001f) - timerTime >= 1) {
                SetTimer(supplyStoreTime - 1000);
            }
        }
    }

    public void LinkTimer(TMPro.TextMeshProUGUI timerText) {
        this.timerText = timerText;
    }

    public void SetTimer(int supplyStoreTime) {
        TimeSpan time = TimeSpan.FromMilliseconds(supplyStoreTime);
        
        this.supplyStoreTime = supplyStoreTime;
        timerTime = supplyStoreTime * 0.001f;
        if (timerTime > 0) {
            supplyStoreTimer = time.Hours.ToString() + ":" + time.Minutes.ToString() + ":" + time.Seconds.ToString() + "후 +20";
        }
        else
            supplyStoreTimer = "창고 가득참";
        if (timerText != null)
            timerText.text = supplyStoreTimer;
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using SRDebugger;
using SRDebugger.Services;
using SRF;
using SRF.Service;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class SROptions
{
    private int remainShield = 0;
    private int enemyRemainShield = 0;

    private int playerShieldGauge = 0;
    private int enemyShieldGauge = 0;

    private int playerMana = 0;
    private int enemyMana = 0;

    private int playerHealth = 0;
    private int enemyHealth = 0;

    private int cardDraw = 0;

    //private bool playerNoDamage = false;
    private bool noCardCost = false;
    //private bool foolComputer = false;


    [Category("ShieldCount")]
    [NumberRange(0,3)]
    public int PlayerRemainShield {
        get { return remainShield; }
        set {
            OnValueChanged("PlayerRemainShield", value);
            remainShield = value;
        }
    }
    [Category("ShieldCount")]
    public void ShieldCountSet() {
        if(PlayMangement.instance == null) return;
        JObject args = new JObject();
        args["method"] = "shield_count";
        args["value"] = remainShield;
        PlayMangement.instance.socketHandler.SendMethod("cheat", args);
    }
    //[Category("ShieldCount")]
    //[NumberRange(0, 3)]
    //public int EnemyRemainShield {
    //    get { return enemyRemainShield; }
    //    set {
    //        OnValueChanged("EnemyRemainShield", value);
    //        enemyRemainShield = value;
    //    }
    //}

    [Category("ShieldGauge")]
    [NumberRange(0,8)]
    public int PlayerShieldGauge {
        get { return playerShieldGauge; }
        set {
            OnValueChanged("PlayerShieldGauge", value);
            playerShieldGauge = value;
        }
    }

    [Category("ShieldGauge")]
    public void ShieldGaugeSet() {
        if(PlayMangement.instance == null) return;
        JObject args = new JObject();
        args["method"] = "shield_gauge";
        args["value"] = playerShieldGauge;
        PlayMangement.instance.socketHandler.SendMethod("cheat", args);
    }

    //[Category("ShieldGauge")]
    //[NumberRange(0, 8)]
    //public int EnemyShieldGauge {
    //    get { return enemyShieldGauge; }
    //    set {
    //        OnValueChanged("EnemyShieldGauge", value);
    //        enemyShieldGauge = value;
    //    }
    //}

    [Category("SetMana")]
    [NumberRange(0,10)]
    public int PlayerMana {
        get { return playerMana; }
        set {
            OnValueChanged("PlayerMana", value);
            playerMana = value;
        }
    }

    [Category("SetMana"), DisplayName("마나 셋")]
    public void ConfirmSetMana() {
        if (PlayMangement.instance == null) return;
        JObject args = new JObject();
        args["method"] = "resource";
        args["value"] = playerMana;
        PlayMangement.instance.socketHandler.SendMethod("cheat", args);
    }


    //[Category("SetMana")]
    //[NumberRange(0, 10)]
    //public int EnemyMana {
    //    get { return enemyMana; }
    //    set {
    //        OnValueChanged("EnemyMana", value);
    //        enemyMana = value;
    //    }
    //}

    [Category("SetHealth")]
    [NumberRange(1, 20)]
    public int PlayerHealth {
        get { return playerHealth; }
        set {
            OnValueChanged("PlayerHealth", value);
            playerHealth = value;
        }
    }

    [Category("SetHealth"), DisplayName("체력 셋")]
    public void ConfirmSetHealth() {
        if (PlayMangement.instance == null) return;
        JObject args = new JObject();
        args["method"] = "hp";
        args["value"] = playerHealth;
        PlayMangement.instance.socketHandler.SendMethod("cheat", args);
    }
    //[Category("SetHealth")]
    //[NumberRange(1, 20)]
    //public int EnemyHealth {
    //    get { return enemyHealth; }
    //    set {
    //        OnValueChanged("EnemyHealth", value);
    //        enemyHealth = value;
    //    }
    //}

    [Category("CardDraw")]
    [NumberRange(1, 10)]
    public int CardDraw {
        get { return cardDraw; }
        set {
            OnValueChanged("Card", value);
            cardDraw = value;
        }
    }

    //[Category("Button"), DisplayName("턴 시간 무제한")]
    //public bool InfinityTurnTime {
    //    get { return noCardCost; }
    //    set {
    //        noCardCost = !noCardCost;
    //        OnToggleButton("카드코스트", noCardCost);
    //    }
    //}

    [Category("Button"), DisplayName("카드 코스트 0")]
    public bool NoCardCost {
        get { return noCardCost; }
        set {
            noCardCost = !noCardCost;
            OnToggleButton("카드코스트", noCardCost);
        }
    }



    private void OnValueChanged(string n, object newValue) {
        Debug.Log("[SRDebug] {0} value changed to {1}".Fmt(n, newValue));
        OnPropertyChanged(n);
    }

    private void OnToggleButton(string n, object value) {
        Debug.Log("[SRDebug] {0} value changed to {1}".Fmt(n, value));
    }



}

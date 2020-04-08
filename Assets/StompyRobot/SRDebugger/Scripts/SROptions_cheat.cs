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
    private bool timerOff = false;
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
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.cheat, args);
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
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.cheat, args);
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
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.cheat, args);
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
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.cheat, args);
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

    [Category("Button"), DisplayName("턴 시간 무제한")]
    public bool InfinityTurnTime {
       get { return timerOff; }
       set {
           timerOff = !timerOff;
           OnToggleButton("턴 시간 무제한", timerOff);
            if (PlayMangement.instance == null) return;
            JObject args = new JObject();
            args["method"] = "time_stop";
            args["value"] = timerOff;
            PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.cheat, args);
        }
    }

    [Category("Button"), DisplayName("카드 코스트 0")]
    public bool NoCardCost {
        get { return noCardCost; }
        set {
            noCardCost = !noCardCost;
            OnToggleButton("카드코스트", noCardCost);
            if (PlayMangement.instance == null) return;
            JObject args = new JObject();
            args["method"] = "free_card";
            args["value"] = noCardCost;
            PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.cheat, args);
        }
    }

    [Category("GetAllCard")]
    public void GetAllCard() {
        AccountManager.Instance.RequestAllCardCheat((req, res) => {
            if (res.StatusCode == 200 || res.StatusCode == 304)
                Modal.instantiate("모든 카드를 얻으셨습니다.\n메인 화면을 다시 불러옵니다.", Modal.Type.CHECK, 
                    () => FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE));
            else {
                AccountManager.Instance.OccurNetworkErrorModal("POST", res.Message.ToString());
            }
        });
    }

    private string _cardId= "ac10001";

    [Category("GetIdCard")]
    public string CardId {
        get { return _cardId; }
        set { _cardId = value; }
    }

    [Category("GetIdCard")]
    public void GetIdCard() {
        if (PlayMangement.instance == null) return;
        JObject args = new JObject();
        args["method"] = "draw";
        args["value"] = _cardId;
        PlayMangement.instance.SettingMethod(BattleConnector.SendMessageList.cheat, args);
    }



    private void OnValueChanged(string n, object newValue) {
        Debug.Log("[SRDebug] {0} value changed to {1}".Fmt(n, newValue));
        OnPropertyChanged(n);
    }

    private void OnToggleButton(string n, object value) {
        Debug.Log("[SRDebug] {0} value changed to {1}".Fmt(n, value));
    }



}

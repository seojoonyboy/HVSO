using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleConnectSceneAnimController : MonoBehaviour {
    private Timer changeWaitMessageTimer = null;
    [SerializeField] private Text waitingMessage;
    public void EntryAnimFinished() {
        string battleType = PlayerPrefs.GetString("SelectedBattleType");
        PlayerPrefs.DeleteKey("ReconnectData");
        
        if(battleType == "league") {
            FindObjectOfType<BattleConnector>().OpenLobby();
        }
        else {
            FindObjectOfType<BattleConnector>().OpenSocket();
        }

        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        string heroid = PlayerPrefs.GetString("selectedHeroId");
        
        Animator animator = GetComponent<Animator>();
        GameObject portraitObject;
        Sprite portrait = AccountManager.Instance.resource.heroPortraite[heroid];

        switch (race) {
            case "human":
                portraitObject = gameObject.transform.Find("PlayerCharacter/Zerod").gameObject;
                portraitObject.GetComponent<Image>().sprite = portrait;
                animator.Play("HumanWait");              
                break;
            case "orc":
                portraitObject = gameObject.transform.Find("PlayerCharacter/Kracus").gameObject;
                portraitObject.GetComponent<Image>().sprite = portrait;
                animator.Play("OrcWait");                
                break;
        }

        if(battleType == "story") {
            AccountManager.Instance.prevSceneName = "Story";
        }
        else if(battleType == "league" || battleType == "leagueTest") {
            AccountManager.Instance.prevSceneName = "League";
        }
        else if(battleType == "solo") {
            AccountManager.Instance.prevSceneName = "Solo";
        }
        
        Fbl_Translator translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        var msg = translator.GetLocalizedText("MainUI", "ui_page_league_expandmatch");
        changeWaitMessageTimer = Timer.Register(10, () => {
            if (!string.IsNullOrEmpty(msg)) waitingMessage.text = msg;
        });
    }

    public void PlayStartBattleAnim() {
        Timer.Cancel(changeWaitMessageTimer);
        
        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        Animator animator = GetComponent<Animator>();
        switch (race) {
            case "human":
                animator.Play("StartHumanBattle");
                break;
            case "orc":
                animator.Play("StartOrcBattle");
                break;
        }
    }

    public void StartBattleAnimFinished() {
        FindObjectOfType<BattleConnector>().StartBattle();
    }
}

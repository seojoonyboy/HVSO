using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckListHandlerInBattleReady : MonoBehaviour {
    AccountManager accountManager;
    [SerializeField] Transform content;
    [SerializeField] BattleReadySceneController parentController;

    void Awake() {
        accountManager = AccountManager.Instance;
    }

    void OnEnable() {
        parentController.HudController.SetBackButton(() => {
            gameObject.SetActive(false);
        });
        ResetMyDecks();
        LoadMyDecks();
    }

    void OnDisable() {
        parentController.HudController.SetBackButton(() => {
            parentController.OnBackButton();
        });
    }

    public void LoadMyDecks() {
        var humanDecks = accountManager.humanDecks;
        var orcDecks = accountManager.orcDecks;

        for(int i=0; i<humanDecks.Count; i++) {
            content.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void ResetMyDecks() {
        foreach(Transform child in content) {
            child.gameObject.SetActive(false);
        }
    }
}

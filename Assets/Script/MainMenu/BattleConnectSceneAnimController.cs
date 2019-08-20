using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleConnectSceneAnimController : MonoBehaviour {
    public void EntryAnimFinished() {
        FindObjectOfType<BattleConnector>().OpenSocket();

        string race = PlayerPrefs.GetString("SelectedRace").ToLower();
        Animator animator = GetComponent<Animator>();
        switch (race) {
            case "human":
                animator.Play("HumanWait");
                break;
            case "orc":
                animator.Play("OrcWait");
                break;
        }
    }

    public void PlayStartBattleAnim() {
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

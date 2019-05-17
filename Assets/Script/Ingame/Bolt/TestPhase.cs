using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using System;

public class TestPhase : MonoBehaviour {
    [SerializeField] GameObject phaseManager;

    private void Start() {
        //string selectedRace = Variables.Saved.Get("SelectedRace").ToString();
        //Debug.Log(selectedRace);
    }

    private void Awake() {
        //GetComponent<TurnChanger>().onTurnChanged.AddListener(() => OnTurnChange());
    }

    private void OnTurnChange() {
        //Debug.Log("턴이 바뀜");
    }

    public void PrintCurrentSceneName() {
        string currentTurn = Variables.Scene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).Get("CurrentTurn").ToString();
        //Debug.Log(currentTurn);
    }

    // Update is called once per frame
    void Update() {

    }
}

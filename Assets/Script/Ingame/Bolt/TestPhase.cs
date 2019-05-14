using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class TestPhase : MonoBehaviour {
    [SerializeField] GameObject phaseManager;
    // Start is called before the first frame update

    IEnumerator Start() {
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        PrintCurrentSceneName();
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        PrintCurrentSceneName();
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        PrintCurrentSceneName();
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        PrintCurrentSceneName();
    }

    public void PrintCurrentSceneName() {
        string currentTurn = Variables.Scene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).Get("CurrentTurn").ToString();
        Debug.Log(currentTurn);
    }

    // Update is called once per frame
    void Update() {

    }
}

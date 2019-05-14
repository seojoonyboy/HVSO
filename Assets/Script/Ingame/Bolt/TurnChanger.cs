using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnChanger : MonoBehaviour {
    // Start is called before the first frame update
    private int index = -1;
    TurnType turn;

    public void NextTurn() {
        turn = (TurnType)((++index) % 4);
        //Debug.Log(turn.ToString());
        Variables.Scene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        ).Set("CurrentTurn", turn.ToString());
    }

    public enum TurnType {
        ZOMBIE = 0,
        PLANT = 1,
        SECRET = 2,
        BATTLE = 3
    }
}

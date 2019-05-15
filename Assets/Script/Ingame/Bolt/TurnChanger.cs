using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnChanger : MonoBehaviour {
    public UnityEvent onTurnChanged;

    // Start is called before the first frame update
    private int index = -1;
    TurnType turn;

    /// <summary>
    /// Machine 전용 함수
    /// </summary>
    public void NextTurn() {
        turn = (TurnType)((++index) % 4);
        //Debug.Log(turn.ToString());
        Variables.Scene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        ).Set("CurrentTurn", turn.ToString());

        onTurnChanged.Invoke();
        Debug.Log(index);
    }

    public enum TurnType {
        ZOMBIE = 0,
        PLANT = 1,
        SECRET = 2,
        BATTLE = 3
    }
}

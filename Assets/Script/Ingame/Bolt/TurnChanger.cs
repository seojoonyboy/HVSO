using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnChanger : MonoBehaviour {
    public UnityEvent onTurnChanged;
    public UnityEvent onPrepareTurn;
    // Start is called before the first frame update
    private int index = -1;
    TurnType turn;

    /// <summary>
    /// Machine 전용 함수
    /// </summary>
    public void NextTurn() {
        turn = (TurnType)((++index) % 4);
        //Logger.Log(turn.ToString());
        Variables.Scene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        ).Set("CurrentTurn", turn.ToString());

        onTurnChanged.Invoke();
    }

    /// <summary>
    /// 준비 턴(멀리건 처리 턴)
    /// Machine 전용 함수
    /// </summary>
    public void OnPrepareTurn() {
        onPrepareTurn.Invoke();
        Logger.Log("준비 턴");
        //StartCoroutine(TestNextTurn());
    }

    IEnumerator TestNextTurn() {
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(gameObject, "EndTurn");
        //Logger.Log("턴 종료");
    }

    public enum TurnType {
        ZOMBIE = 0,
        PLANT = 1,
        SECRET = 2,
        BATTLE = 3
    }
}

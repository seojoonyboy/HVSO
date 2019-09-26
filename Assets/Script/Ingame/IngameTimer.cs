using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngameTimer : MonoBehaviour {
    [SerializeField] GameObject timerUI;
    [SerializeField] TextMeshProUGUI value;
    public int uiStartSec = 10;

    int currentSec;
    int decreaseAmount = 1; //pause 처리를 위함
    IEnumerator TimerCoroutine, UICoroutine;

    private void OnTimerUI() {
        Logger.Log("Timer UI 시작");
        timerUI.SetActive(true);

        UICoroutine = TimerUIOn();
        StartCoroutine(UICoroutine);
    }

    /// <summary>
    /// UI 타이머 Coroutine
    /// </summary>
    /// <returns></returns>
    IEnumerator TimerUIOn() {
        while (currentSec > 0) {
            value.text = currentSec.ToString();
            yield return new WaitForSeconds(1.0f);
            currentSec -= decreaseAmount;
        }
        EndTimer();
    }

    /// <summary>
    /// 전체 타이머 Coroutine
    /// </summary>
    /// <param name="totalSec"></param>
    /// <returns></returns>
    IEnumerator TimerOn(int totalSec) {
        currentSec = totalSec;
        while (currentSec > uiStartSec) {
            Logger.Log("Timer : " + currentSec);
            yield return new WaitForSeconds(1.0f);
            currentSec -= decreaseAmount;
        }

        OnTimerUI();
    }

    /// <summary>
    /// 타이머 시작
    /// </summary>
    /// <param name="totalSec"></param>
    public void BeginTimer(int totalSec = 40) {
        TimerCoroutine = TimerOn(totalSec);
        StartCoroutine(TimerCoroutine);
    }

    /// <summary>
    /// 타이머 종료
    /// </summary>
    public void EndTimer() {
        timerUI.SetActive(false);
        StopAllCoroutines();
    }

    /// <summary>
    /// 타이머 일시정지 (ex. 상대 실드가 터져 잠시 대기 할때)
    /// </summary>
    public void PauseTimer() {
        decreaseAmount = 0;
    }

    /// <summary>
    /// 타이머 재개
    /// </summary>
    public void ResumeTimer() {
        decreaseAmount = 1;
    }

#if UNITY_EDITOR
    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            BeginTimer();
        }
    }
#endif
}

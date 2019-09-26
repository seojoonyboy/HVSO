using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngameTimer : MonoBehaviour {
    [SerializeField] GameObject timerUI;
    [SerializeField] TextMeshProUGUI value;
    public int startSec = 10;
    int currentSec;
    IEnumerator coroutine;
    public void OnTimerUI() {
        timerUI.SetActive(true);

        coroutine = TimerOn();
        StartCoroutine(coroutine);
    }

    IEnumerator TimerOn() {
        currentSec = startSec;
        while (currentSec > 0) {
            value.text = currentSec.ToString();
            yield return new WaitForSeconds(1.0f);
            currentSec -= 1;
        }
        OffTimerUI();
    }

    public void OffTimerUI() {
        timerUI.SetActive(false);

        if (coroutine == null) return;
        StopCoroutine(coroutine);
    }

#if UNITY_EDITOR
    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            OnTimerUI();
        }
    }
#endif
}

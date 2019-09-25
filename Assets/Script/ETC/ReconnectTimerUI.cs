using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReconnectTimerUI : MonoBehaviour {
    [SerializeField] Text text;

    void Start() {
        StartCoroutine(MoveSceneRoutine());
    }

    IEnumerator MoveSceneRoutine() {
        float elapsedTime = 0f;
        float duration = 0f;

        while (elapsedTime < duration) {
            yield return 0;
            elapsedTime += Time.unscaledDeltaTime;
            var time = TimeSpan.FromSeconds(elapsedTime);
            string resultText = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                time.Hours,
                time.Minutes,
                time.Seconds);

            text.text = "상대를 기다리는 중...\n대기시간 : " + resultText;
        }
    }
}

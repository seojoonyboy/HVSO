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
        float _time = 30f;

        while (true) {
            yield return 0;
            _time -= Time.unscaledDeltaTime;
            var time = TimeSpan.FromSeconds(_time);
            string resultText = string.Format("{0:D2}:{1:D2}:{2:D2}",
                time.Hours,
                time.Minutes,
                time.Seconds);

            text.text = "상대를 기다리는 중...\n남은시간 : " + resultText;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class HideModalTimer : MonoBehaviour {
    IDisposable observer_1;
    private const float WAIT_TIME = 3.0f;

    private void OnEnable() {
        float currentTime = 0;
        observer_1 = Observable
            .EveryUpdate()
            .Select(_ => currentTime += Time.deltaTime)
            .SkipWhile(x => x < WAIT_TIME)
            .First()
            .Subscribe(_ => gameObject.SetActive(false));
    }

    void OnDisable() {
        observer_1.Dispose();
    }
}

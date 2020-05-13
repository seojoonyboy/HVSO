using System;
using System.Collections;
using System.Collections.Generic;
using SA.Android.GMS.Games;
using TMPro;
using UniRx;
using UnityEngine;

public class DestroyTimer : MonoBehaviour {
    IDisposable observer_1, observer_2;
    private float targetTime = 3;
    private float currentTime;
    
    public void StartTimer(float time) {
        targetTime = time;
        currentTime = 0;
        
        observer_1 = Observable
            .EveryUpdate()
            .Select(_ => currentTime += Time.deltaTime)
            .SkipWhile(x => x < targetTime)
            .First()
            .Subscribe(_ => Destroy(gameObject));
    }

    public void StartTimer(float time, TextMeshProUGUI textTarget) {
        targetTime = time;
        currentTime = 0;

        TextMeshProUGUI textComp = textTarget;
        observer_1 = Observable
            .EveryUpdate()
            .Select(_ => currentTime += Time.deltaTime)
            .SkipWhile(x => x < targetTime)
            .First()
            .Subscribe(_ => Destroy(gameObject));

        observer_2 = Observable
            .Interval(TimeSpan.FromMilliseconds(1000))
            .Subscribe(_ => {
                textComp.text = "..." + (int) (targetTime - currentTime) + "...";
            });
    }
    
    void OnDisable() {
        observer_1?.Dispose();
        observer_2?.Dispose();
    }

    private void OnDestroy() {
        observer_1?.Dispose();
        observer_2?.Dispose();
    }
}
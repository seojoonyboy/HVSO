using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MenuTimerController : Singleton<MenuTimerController> {

    protected MenuTimerController() { }
    // Start is called before the first frame update
    public delegate void timeFuction();

    public Dictionary<TimerType, TimerClass> timerList;

    private void Awake() {
        timerList = new Dictionary<TimerType, TimerClass>();
    }

    // Update is called once per frame
    void Update() {
        if (timerList.Count == 0) return;

        foreach (TimerClass timeData in timerList.Values) {
            if (!timeData.timerOn) continue;
            timeData.remainTime -= Time.deltaTime * 1000;
            if (timeData.remainTime <= 0) {
                timeData.function?.Invoke();
                timeData.timerOn = false;
                continue;
            }
            else
                timeData.outputText.text = SetTime(timeData.remainTime);
        }
    }

    public void SetTimer(TimerType key, int time, TMPro.TextMeshProUGUI text, timeFuction func = null) {
        bool onTimer = time > 0;
        if (timerList.ContainsKey(key)) {
            timerList[key] = new TimerClass { remainTime = (float)time, outputText = text, function = func, timerOn = onTimer };
            return; 
        }
        timerList.Add(key, new TimerClass { remainTime = (float)time, outputText = text, function = func, timerOn = onTimer });
    }

    protected string SetTime(float timeRemain) {
        TimeSpan time = TimeSpan.FromMilliseconds(timeRemain);
        string timerString;
        string minute;
        string second;

        if (time.Minutes < 10)
            minute = "0" + time.Minutes.ToString();
        else
            minute = time.Minutes.ToString();

        if (time.Seconds < 10)
            second = "0" + time.Seconds.ToString();
        else
            second = time.Seconds.ToString();

        timerString = time.Hours.ToString() + ":" + minute + ":" + second;

        return timerString;
    }


    public class TimerClass {
        public float remainTime;
        public TMPro.TextMeshProUGUI outputText;
        public timeFuction function;
        public bool timerOn;
    }

    public enum TimerType {
        SHOP_AD_BOX,
        QUEST_REFRESH,
    }
}

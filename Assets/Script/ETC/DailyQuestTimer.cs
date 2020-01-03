using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quest;
using UniRx;
using System;
using BestHTTP;

public class DailyQuestTimer : MonoBehaviour {
    IDisposable observable_1;

    void Start() {
        DateTime currentTime = DateTime.UtcNow;
        var korCurrentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentTime, "Korea Standard Time");

        DateTime tommorowTime = korCurrentTime.AddDays(1).AddTicks(-1);
        DateTime resetStandardTime = new DateTime(
            tommorowTime.Year,
            tommorowTime.Month,
            tommorowTime.Day,
            0,
            0,
            0
        );

        var lastSec = resetStandardTime.Subtract(DateTime.MinValue).TotalSeconds - currentTime.Subtract(DateTime.MinValue).TotalSeconds;
        TimeSpan t = TimeSpan.FromSeconds(lastSec);
        //Logger.Log(lastSec + "초 뒤에 DailyQuest 갱신함");
        observable_1 = Observable
                    .EveryUpdate()
                    .Select(_ => lastSec -= Time.deltaTime)
                    .SkipWhile(x => x > 0)
                    .First()
                    .Subscribe(_ => {
                        Start();
                        PlayerPrefs.SetInt("IsQuestLoaded", 0);
                    });
    }

    private void OnDailyQuestRequestFinished(HTTPRequest originalRequest, HTTPResponse response) {
        throw new NotImplementedException();
    }
}

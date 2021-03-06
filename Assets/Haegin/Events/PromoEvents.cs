using HaeginGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Haegin
{
    public class PromoEvents
    {
        public delegate void OnFinishedDelegate();
        public delegate void OnCloseWindow(bool doNotShowAgainToday);
        public delegate void OnOpenPromoEventWindow(string imageUrl, string destUrl, OnCloseWindow onCloseWindow);

        public static void CheckPromoEvents(OnOpenPromoEventWindow onOpenPromoEventWindow, OnFinishedDelegate onFinished)
        {
            if (System.DateTime.Now < GetDoNotShowUntil())
            {
                onFinished();
            }
            else {
                WebClient.GetInstance().RequestEventsList((WebClient.ErrorCode error, List<EventItem> list) => {
                    if (error == WebClient.ErrorCode.SUCCESS)
                    {
                        if (list == null || list.Count <= 0)
                        {
                            onFinished();
                        }
                        else
                        {
                            ShowEvents(list, 0, onOpenPromoEventWindow, onFinished);
                        }
                    }
                    else
                    {
                        onFinished();
                    }
                });
            }
        }

        public static void ShowEvents(List<EventItem> events, int eventIndex, OnOpenPromoEventWindow onOpenPromoEventWindow, OnFinishedDelegate onFinished)
        {
            if (eventIndex < events.Count)
            {
                onOpenPromoEventWindow(events[eventIndex].Image, events[eventIndex].Uri, (bool doNotShowAgainToday) => {
                    if (doNotShowAgainToday)
                    {
                        ResetDoNotShowUntil();
                        eventIndex = events.Count;
                    }
                    eventIndex++;
                    ShowEvents(events, eventIndex, onOpenPromoEventWindow, onFinished);
                });
            }
            else
            {
                onFinished();
            }
        }

        static System.DateTime GetDoNotShowUntil()
        {
            return System.DateTime.FromBinary(long.Parse(PlayerPrefs.GetString("EventDoNotShowUntil", "0")));
        }

        static void ResetDoNotShowUntil()
        {
            System.DateTime until = System.DateTime.Now;
            until = until.AddHours(-until.Hour);
            until = until.AddMinutes(-until.Minute);
            until = until.AddSeconds(-until.Second);
            until = until.AddMilliseconds(-until.Millisecond);
            until = until.AddHours(24.0f);

            PlayerPrefs.SetString("EventDoNotShowUntil", "" + until.ToBinary());
        }
    }
}
using System;
using UnityEngine;
#if UNITY_STANDALONE && USE_STEAM
using Steamworks;
#elif UNITY_ANDROID
using SA.Android.App;
using SA.Android.GMS.Games;
#elif UNITY_IOS
using SA.iOS.GameKit;
#endif


namespace Haegin
{
    public class GameServiceAchievements
    {
        public delegate void OnAchievementChanged();

        public static void ShowAchievementsUI()
        {
            if (Account.IsLoggedInGameService() == false) return;
#if UNITY_ANDROID
            var client = AN_Games.GetAchievementsClient();
            if(client != null)
            {
                client.GetAchievementsIntent((result) => {
                    if (result.IsSucceeded)
                    {
                        var intent = result.Intent;
                        AN_ProxyActivity proxy = new AN_ProxyActivity();
                        proxy.StartActivityForResult(intent, (intentResult) => {
                            proxy.Finish();
                            //TODO you might want to check is user had sigend out with that UI
                        });
                    }
                    else
                    {
#if MDEBUG
                        Debug.Log("Failed to Get Achievements Intent " + result.Error.FullMessage);
#endif
                    }
                });
            }
#elif UNITY_IOS
            ISN_GKGameCenterViewController viewController = new ISN_GKGameCenterViewController();
            viewController.ViewState = ISN_GKGameCenterViewControllerState.Achievements;
            viewController.Show();
#elif UNITY_STANDALONE

#endif
        }

        public static void UnlockAchievement(string id, OnAchievementChanged callback)
        {
            if (Account.IsLoggedInGameService() == false) return;
#if UNITY_ANDROID
            var client = AN_Games.GetAchievementsClient();
            if(client != null)
            {
                client.UnlockImmediate(id, (result) =>
                {
                    ThreadSafeDispatcher.Instance.Invoke(() => {
                        callback();
                    });
                });
            }
#elif UNITY_IOS
            ProgressAchievement(id, 1, 1, callback);
#elif UNITY_STANDALONE && USE_STEAM
            if (SteamUserStats.RequestCurrentStats())
            {
                SteamUserStats.SetAchievement(id);
                if (SteamUserStats.StoreStats())
                {
                    SteamUserStats.IndicateAchievementProgress(id, 2, 10);
                    callback();
                }
            }
#endif
        }

        public static void ProgressAchievement(string id, int currentStep, int goalStep, OnAchievementChanged callback)
        {
            if (Account.IsLoggedInGameService() == false) return;
#if UNITY_ANDROID
            var client = AN_Games.GetAchievementsClient();
            if(client != null)
            {
                client.Load(false, (result) => {
                    if(result.IsSucceeded)
                    {
                        AN_Achievement ta = null;
                        foreach(var a in result.Achievements)
                        {
                            if(a.AchievementId.Equals(id))
                            {
                                ta = a;
                                currentStep -= ta.CurrentSteps;
                                client.IncrementImmediate(id, currentStep, (result2) => {
#if MDEBUG
                                    Debug.Log("Operation Succeeded: " + result2.IsSucceeded);
#endif
                                    ThreadSafeDispatcher.Instance.Invoke(() => {
                                        callback();
                                    });
                                });
                                break;
                            }
                        }
                        if(ta == null)
                        {
                            ThreadSafeDispatcher.Instance.Invoke(() => {
                                callback();
                            });
                        }
                    }
                    else
                    {
                        ThreadSafeDispatcher.Instance.Invoke(() => {
                            callback();
                        });
                    }
                });
            }
#elif UNITY_IOS
            ISN_GKAchievement achievement1 = new ISN_GKAchievement(id);
            float progress = (float)currentStep / (float)goalStep * 100.0f;
            if (progress < 0.0f) progress = 0.0f;
            else if (progress > 100.0f) progress = 100.0f;
            achievement1.PercentComplete = progress;
            achievement1.Report((result) => {
#if MDEBUG
                if (result.IsSucceeded)
                {
                    Debug.Log("ReportAchievements succeeded");
                }
                else
                {
                    Debug.Log("LoadAchievements failed! With: " + result.Error.Code + " and description: " + result.Error.Message);
                }
#endif
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    callback();
                });
            });
#elif UNITY_STANDALONE && USE_STEAM
            if(SteamUserStats.RequestCurrentStats()) 
            {
                SteamUserStats.SetStat("stat_" + id, currentStep);
                if(SteamUserStats.StoreStats())
                {
                    int data;
                    SteamUserStats.GetStat("stat_" + id, out data);
#if MDEBUG
                    Debug.Log("Stat : [" + data + "]");
#endif
                    SteamUserStats.IndicateAchievementProgress(id, (uint)currentStep, (uint)goalStep);
                    callback();
                }
            }
#endif
        }
    }
}

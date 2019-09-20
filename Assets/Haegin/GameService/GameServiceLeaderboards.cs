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
    public class GameServiceLeaderboards
    {
        public delegate void OnScoreSubmit();

        public static void ShowLeaderboardUI(string id = null)
        {
            if (Account.IsLoggedInGameService() == false) return;
#if UNITY_ANDROID
            if (id == null)
            {
                var leaderboards = AN_Games.GetLeaderboardsClient();
                if(leaderboards != null)
                {
                    leaderboards.GetAllLeaderboardsIntent((result) => {
                        if (result.IsSucceeded)
                        {
                            var intent = result.Intent;
                            AN_ProxyActivity proxy = new AN_ProxyActivity();
                            proxy.StartActivityForResult(intent, (intentResult) => {
                                proxy.Finish();
                                //Note: you might want to check is user had sigend out with that UI
                            });
                        }
                        else
                        {
#if MDEBUG
                            Debug.Log("Failed to Get leaderboards Intent " + result.Error.FullMessage);
#endif
                        }
                    });
                }
            }
            else
            {
                var leaderboards = AN_Games.GetLeaderboardsClient();
                if(leaderboards != null)
                {
                    leaderboards.GetLeaderboardIntent(id, (result) => {
                        if (result.IsSucceeded)
                        {
                            var intent = result.Intent;
                            AN_ProxyActivity proxy = new AN_ProxyActivity();
                            proxy.StartActivityForResult(intent, (intentResult) => {
                                proxy.Finish();
                                //Note: you might want to check is user had sigend out with that UI
                            });
                        }
                        else
                        {
#if MDEBUG
                            Debug.Log("Failed to Get leaderboards Intent " + result.Error.FullMessage);
#endif
                        }
                    });
                }
            }
#elif UNITY_IOS
            ISN_GKGameCenterViewController viewController = new ISN_GKGameCenterViewController();
            viewController.LeaderboardIdentifier = id;
            viewController.ViewState = ISN_GKGameCenterViewControllerState.Leaderboards;
            viewController.Show();
#elif UNITY_STANDALONE && USE_STEAM
            SteamFriends.ActivateGameOverlay("Achievements");
#endif
    }

        public static void SubmitScore(string id, double score, OnScoreSubmit callback)
        {
            if (Account.IsLoggedInGameService() == false) return;
#if UNITY_ANDROID
            var leaderboards = AN_Games.GetLeaderboardsClient();
            if(leaderboards != null)
            {
                leaderboards.SubmitScoreImmediate(id, (long)score, "", (result) => {
                    if (result.IsSucceeded)
                    {
                        var scoreSubmissionData = result.Data;
#if MDEBUG
                        Debug.Log("SubmitScoreImmediate completed");
                        Debug.Log("scoreSubmissionData.PlayerId: " + scoreSubmissionData.PlayerId);
                        Debug.Log("scoreSubmissionData.LeaderboardId: " + scoreSubmissionData.LeaderboardId);
#endif
                        foreach (AN_Leaderboard.TimeSpan span in (AN_Leaderboard.TimeSpan[])System.Enum.GetValues(typeof(AN_Leaderboard.TimeSpan)))
                        {
                            var scoreSubmissionResult = scoreSubmissionData.GetScoreResult(span);
#if MDEBUG
                            Debug.Log("scoreSubmissionData.FormattedScore: " + scoreSubmissionResult.FormattedScore);
                            Debug.Log("scoreSubmissionData.NewBest: " + scoreSubmissionResult.NewBest);
                            Debug.Log("scoreSubmissionData.RawScore: " + scoreSubmissionResult.RawScore);
                            Debug.Log("scoreSubmissionData.ScoreTag: " + scoreSubmissionResult.ScoreTag);
#endif
                        }
                    }
                    else
                    {
#if MDEBUG
                        Debug.Log("Failed to Submit Score Immediate " + result.Error.FullMessage);
#endif
                    }
                    ThreadSafeDispatcher.Instance.Invoke(() =>
                    {
                        callback();
                    });
                });
            }
#elif UNITY_IOS
            ISN_GKScore scoreReporter = new ISN_GKScore(id);
            scoreReporter.Value = (long)score;
            scoreReporter.Report((result) => {
#if MDEBUG
                if (result.IsSucceeded)
                {
                    Debug.Log("Score Report Success");
                }
                else
                {
                    Debug.Log("Score Report failed! Code: " + result.Error.Code + " Message: " + result.Error.Message);
                }
#endif
                ThreadSafeDispatcher.Instance.Invoke(() => {
                    callback();
                });
            });
#elif UNITY_STANDALONE && USE_STEAM
            CallResult<LeaderboardFindResult_t> findResult = new CallResult<LeaderboardFindResult_t>();
            SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(id);
            findResult.Set(hSteamAPICall, (LeaderboardFindResult_t pCallback, bool failure) => {
                if(failure) 
                {
                    callback();    
                }
                else 
                {
                    SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(pCallback.m_hSteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (int)score, null, 0);
                    CallResult<LeaderboardScoreUploaded_t> uploadResult = new CallResult<LeaderboardScoreUploaded_t>();
                    uploadResult.Set(handle, (LeaderboardScoreUploaded_t pUploadCallback, bool uploadFailure) => {
                        callback();
                    });
                }
            });
#endif
        }
    }
}

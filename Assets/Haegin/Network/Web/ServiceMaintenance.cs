using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Haegin
{
    public class ServiceMaintenance
    {
        public enum Action
        {
            DisableInput,
            EnableInput
        }

        public delegate void OnRetry();
        public delegate void OnLive(string ServerUrl, string PatchUrl);
        public delegate void OnLiveV2(string CommonUrl, string GameUrl, string PatchUrl);
        public delegate void ShowDialog(bool IsError, string contents, string time, OnRetry onRetry);
        public delegate void OnAction(Action op, string contents, string time);

        static private WebClient webClient;

        public static void CheckStatus(string url, string serverName, ShowDialog showDialog, OnAction onAction, OnLive callback)
        {
            // 서버에 체크하고 
            // 라이브
            webClient = WebClient.GetInstance(url, false);
            webClient.RetryMax = 1;
            webClient.TimeOut = 5000;
            webClient.ErrorOccurred += (int error) => {
                CheckStatusRetry("", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), url, serverName, showDialog, onAction, callback, true);
            };
            webClient.RetryFailed += (protocol) => {
                CheckStatusRetry("", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), url, serverName, showDialog, onAction, callback, true);
            };
            webClient.Logged += (string log) =>
            {
#if MDEBUG
                Debug.Log("Unity   " + log);
#endif
            };
            ServerReq(serverName, (bool IsLive, string contents, DateTime startUtc, DateTime endUtc, string ServerUrl, string PatchUrl) =>
            {
                if(IsLive)
                {
                    GameObject obj = GameObject.Find("HaeginWebClient" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url)));
                    if (obj != null)
                    {
                        obj.transform.SetParent(null);
                        GameObject.Destroy(obj);
                    }
                    callback(ServerUrl, PatchUrl);
                }
                else
                {
                    CheckStatusRetry(contents, startUtc, endUtc, url, serverName, showDialog, onAction, callback, false);
                }
            });
        }

        static void CheckStatusRetry(string contents, DateTime startUtc, DateTime endUtc, string url, string serverName, ShowDialog showDialog, OnAction onAction, OnLive callback, bool isError)
        {
            DateTime start = startUtc.ToLocalTime();
            DateTime end = endUtc.ToLocalTime();
            string time = start.ToShortDateString() + " " + start.ToShortTimeString() + " ~ " + end.ToShortDateString() + " " + end.ToShortTimeString();

            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                showDialog(isError, contents, time, () => {
                    onAction(Action.DisableInput, contents, time);
                    ServerReq(serverName, (bool IsLiveRetry, string contentsRetry, DateTime startUtcRetry, DateTime endUtcRetry, string ServerUrlRetry, string PatchUrlRetry) =>
                    {
                        if (IsLiveRetry)
                        {
                            GameObject obj = GameObject.Find("HaeginWebClient" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url)));
                            if (obj != null)
                            {
                                obj.transform.SetParent(null);
                                GameObject.Destroy(obj);
                            }
                            callback(ServerUrlRetry, PatchUrlRetry);
                        }
                        else
                        {
                            start = startUtcRetry.ToLocalTime();
                            end = endUtcRetry.ToLocalTime();
                            time = start.ToShortDateString() + " " + start.ToShortTimeString() + " ~ " + end.ToShortDateString() + " " + end.ToShortTimeString();
                            onAction(Action.EnableInput, contentsRetry, time);
                        }
                    });
                });
            });
        }

                delegate void OnStatus(bool IsLive, string contetns, DateTime startUtc, DateTime endUtc, string serverUrl, string patchUrl);
        static void ServerReq(string serverName, OnStatus callback)
        {
            webClient.RequestMaintenanceCheck(serverName, (WebClient.ErrorCode error, bool isMaintenance, string contents, DateTime startTime, DateTime endTime, string serverUrl, string patchUrl) =>
            {
                callback(!isMaintenance, contents, startTime, endTime, serverUrl, patchUrl);
            });
        }





        public static void CheckStatusV2(string url, string serverName, ShowDialog showDialog, OnAction onAction, OnLiveV2 callback)
        {
            // 서버에 체크하고 
            // 라이브
            webClient = WebClient.GetInstance(url, false);
            webClient.RetryMax = 1;
            webClient.TimeOut = 5000;
            webClient.ErrorOccurred += (int error) => {
                CheckStatusV2Retry("", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), url, serverName, showDialog, onAction, callback, true);
            };
            webClient.RetryFailed += (protocol) => {
                CheckStatusV2Retry("", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), url, serverName, showDialog, onAction, callback, true);
            };
            webClient.Logged += (string log) =>
            {
#if MDEBUG
                Debug.Log("Unity   " + log);
#endif
            };
            ServerV2Req(serverName, (bool IsLive, string contents, DateTime startUtc, DateTime endUtc, string CommonUrl, string GameUrl, string PatchUrl) =>
            {
                if (IsLive)
                {
                    GameObject obj = GameObject.Find("HaeginWebClient" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url)));
                    if (obj != null)
                    {
                        obj.transform.SetParent(null);
                        GameObject.Destroy(obj);
                    }
                    callback(CommonUrl, GameUrl, PatchUrl);
                }
                else
                {
                    CheckStatusV2Retry(contents, startUtc, endUtc, url, serverName, showDialog, onAction, callback, false);
                }
            });
        }

        static void CheckStatusV2Retry(string contents, DateTime startUtc, DateTime endUtc, string url, string serverName, ShowDialog showDialog, OnAction onAction, OnLiveV2 callback, bool isError)
        {
            DateTime start = startUtc.ToLocalTime();
            DateTime end = endUtc.ToLocalTime();
            string time = start.ToShortDateString() + " " + start.ToShortTimeString() + " ~ " + end.ToShortDateString() + " " + end.ToShortTimeString();

            ThreadSafeDispatcher.Instance.Invoke(() => {
                showDialog(isError, contents, time, () => {
                    onAction(Action.DisableInput, contents, time);
                    ServerV2Req(serverName, (bool IsLiveRetry, string contentsRetry, DateTime startUtcRetry, DateTime endUtcRetry, string CommonUrlRetry, string GameUrlRetry, string PatchUrlRetry) =>
                    {
                        if (IsLiveRetry)
                        {
                            GameObject obj = GameObject.Find("HaeginWebClient" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url)));
                            if (obj != null)
                            {
                                obj.transform.SetParent(null);
                                GameObject.Destroy(obj);
                            }
                            callback(CommonUrlRetry, GameUrlRetry, PatchUrlRetry);
                        }
                        else
                        {
                            start = startUtcRetry.ToLocalTime();
                            end = endUtcRetry.ToLocalTime();
                            time = start.ToShortDateString() + " " + start.ToShortTimeString() + " ~ " + end.ToShortDateString() + " " + end.ToShortTimeString();
                            onAction(Action.EnableInput, contentsRetry, time);
                        }
                    });
                });
            });
        }

        delegate void OnStatusV2(bool IsLive, string contetns, DateTime startUtc, DateTime endUtc, string commonUrl, string gameUrl, string patchUrl);
        static void ServerV2Req(string serverName, OnStatusV2 callback)
        {
            webClient.RequestMaintenanceCheckV2(serverName, (WebClient.ErrorCode error, bool isMaintenance, string contents, DateTime startTime, DateTime endTime, string commonUrl, string gameUrl, string patchUrl) =>
            {
                callback(!isMaintenance, contents, startTime, endTime, commonUrl, gameUrl, patchUrl);
            });
        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NetworkModules {
    public class Request : MonoBehaviour {
        int count;
        
        public void StartRequest(string method, string url, WWWForm data, int tryCount, NetworkManager.Callback callback, NetworkManager.CallbackRetryOccured retryMessageCallback) {
            count = tryCount;
            StartCoroutine(_request(method, url, data, tryCount, callback, retryMessageCallback));
        }

        IEnumerator _request(string method, string url, WWWForm data, int tryCount, NetworkManager.Callback callback, NetworkManager.CallbackRetryOccured retryMessageCallback) {
            UnityWebRequest _www;
            switch (method) {
                case "POST":
                    _www = UnityWebRequest.Post(url, data);
                    break;
                case "PUT":
                    _www = UnityWebRequest.Put(url, data.data);
                    _www.SetRequestHeader("Content-Type", "application/json");
                    break;
                case "DELETE":
                    _www = UnityWebRequest.Delete(url);
                    break;
                case "GET":
                default:
                    _www = UnityWebRequest.Get(url);
                    break;
            }
            //if(count == 1) {
            //    _www.timeout = 10;
            //}
            //else {
            //    _www.timeout = 3;
            //}
            _www.timeout = 10;
            yield return _www.SendWebRequest();

            if(count > 0) {
                if (_www.isNetworkError) {
                    retryMessageCallback.Invoke("재요청을 시작합니다." + (tryCount - count + 1) + "회 시도중");
                    count--;
                    Logger.Log(count);
                    StartCoroutine(_request(method, url, data, tryCount, callback, retryMessageCallback));
                }
                else {
                    callback.Invoke(new HttpResponse(_www));
                    Destroy(gameObject);
                }
            }
            else {
                callback.Invoke(new HttpResponse(_www));
                Destroy(gameObject);
            }
        }
    }
}

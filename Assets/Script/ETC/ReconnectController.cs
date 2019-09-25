using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 게임이 끊겼을 때 메인화면에서부터 재접속 처리를 위한 Controllers
/// </summary>
public class ReconnectController : MonoBehaviour {
    [SerializeField] MyDecksLoader decksLoader;
    List<string> preProcess;
    UnityEvent waitSecEvent = new UnityEvent();

    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void AddProcess(string eventName) {
        UnityEvent unityEvent;
        switch (eventName) {
            case "OnLoadFinished":
                unityEvent = decksLoader.OnLoadFinished;
                break;
            case "OnTemplateLoadFinished":
                unityEvent = decksLoader.OnTemplateLoadFinished;
                break;
            case "OnInvenLoadFinished":
                unityEvent = decksLoader.OnInvenLoadFinished;
                break;
            case "OnOpenSocket":
                unityEvent = BattleConnector.OnOpenSocket;
                break;
            case "WaitSec":
                unityEvent = waitSecEvent;
                break;
            default:
                unityEvent = null;
                break;
        }

        if (unityEvent == null) return;
        preProcess.Add(eventName);
        UnityAction action = null;
        action = () => {
            OnEventOccured(eventName);
            unityEvent.RemoveListener(action);
        };
        unityEvent.AddListener(action);
    }

    private void OnEventOccured(string eventName) {
        if(eventName == "OnInvenLoadFinished") {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }
        preProcess.Remove(eventName);

        if (preProcess.Count == 0) PreSettingFinished();
    }

    public void Init(MyDecksLoader decksLoader) {
        this.decksLoader = decksLoader;

        preProcess = new List<string>();
        string reconnectData = PlayerPrefs.GetString("ReconnectData");
        NetworkManager.ReconnectData serializedData = dataModules.JsonReader.Read< NetworkManager.ReconnectData>(reconnectData);
        string gameId = serializedData.gameId;

        //User Data Setting
        AddProcess("OnLoadFinished");
        AddProcess("OnTemplateLoadFinished");
        AddProcess("OnInvenLoadFinished");

        //BattleConnect
        AddProcess("OnOpenSocket");
    }

    private void ProcessSocketConnect() {

        StartCoroutine(Wait_sec());
    }

    private void PreSettingFinished() {
        Destroy(GetComponent<ReconnectController>());
        Destroy(gameObject);
    }

    IEnumerator Wait_sec() {
        yield return new WaitForSeconds(3.0f);
        waitSecEvent.Invoke();
    }
}

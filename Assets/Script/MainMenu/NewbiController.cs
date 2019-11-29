using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 첫 로그인시 튜토리얼 강제 진행 처리를 위한 컴포넌트
/// </summary>
public class NewbiController : MonoBehaviour {
    MyDecksLoader decksLoader;
    [SerializeField] Transform loadingPanel;
    List<string> preProcess;
    UnityEvent waitSecEvent = new UnityEvent();
    public MenuSceneController menuSceneController;


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
            case "OnScenarioSceneLoaded":
                unityEvent = ScenarioManager.OnLobbySceneLoaded;
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
        preProcess.Remove(eventName);

        if (preProcess.Count == 0) PreSettingFinished();
    }

    void PreSettingFinished() {
        Logger.Log("PreSettingFinished");
        Destroy(gameObject);
    }

    private void ProcessSocketConnect() {
        PlayerPrefs.SetString("SelectedRace", "human");
        PlayerPrefs.SetString("SelectedBattleType", "story");
        PlayerPrefs.SetString("StageNum", "1");
        PlayerPrefs.SetString("SelectedDeckId", string.Empty);
        PlayerPrefs.SetString("selectedHeroId", "h10001");

        //ScenarioGameManagment.chapterData = 

        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);

        StartCoroutine(Wait_sec());
    }

    IEnumerator Wait_sec() {
        yield return new WaitForSeconds(3.0f);
        waitSecEvent.Invoke();
    }

    public void Init(MyDecksLoader decksLoader, ScenarioManager scenarioManager, GameObject loadingModal) {
        this.decksLoader = decksLoader;
        loadingModal.SetActive(true);
        DontDestroyOnLoad(loadingModal.gameObject);

        preProcess = new List<string>();

        var chapterData = scenarioManager.human_chapterDatas[0];
        ScenarioGameManagment.chapterData = chapterData;
        ScenarioGameManagment.challengeDatas = scenarioManager.human_challengeDatas[0].challenges;

        ProcessSocketConnect();

        //BattleConnect
        AddProcess("OnOpenSocket");

        //Wait Until Ingame Muligun Begin
        AddProcess("WaitSec");
    }
}

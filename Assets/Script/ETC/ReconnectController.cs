using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임이 끊겼을 때 메인화면에서부터 재접속 처리를 위한 Controllers
/// </summary>
public class ReconnectController : MonoBehaviour {
    private Queue<PreSetting> presettings;
    private bool _dequeueing = false;

    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private GameObject innerModal;
    
    private bool _onOpenSocket = false;
    void Awake() {
        _onOpenSocket = false;
        DontDestroyOnLoad(gameObject);
    }
    
    IEnumerator Start() {
        AccountManager accountManager = AccountManager.Instance;
        //TODO : MyDecksLoader에서 RequestLeagueInfo를 가장 마지막에 수행하므로 League 데이터가 세팅되는 시점을 대기시켰으나
        //TODO : 이전의 모든 Data를 기다리게 해야할지 판단이 필요함.
        yield return new WaitUntil(() => accountManager.scriptable_leagueData.leagueInfo != null);
        presettings = new Queue<PreSetting>();
        
        presettings.Enqueue(new PreSetting("Open_loadingModal"));     //로딩 모달 활성화
        presettings.Enqueue(new PreSetting("BeginBattleConnect"));    //BattleConnector 생성
        presettings.Enqueue(new PreSetting("Close_loadingModal"));    //로딩 모달 비활성화
        presettings.Enqueue(new PreSetting("End"));                   //로딩 종료
    }

    private void Update() {
        StartCoroutine(CustomUpdateFunc());
    }

    public IEnumerator CustomUpdateFunc() {
        yield return new WaitForSeconds(1.0f);
        if (presettings == null || presettings.Count == 0 || _dequeueing) yield break;

        _dequeueing = true;
        var presseting = presettings.Dequeue();
        switch (presseting.method) {
            case "Open_loadingModal":
                innerModal.SetActive(true);
                _dequeueing = false;
                break;
            case "BeginBattleConnect":
                yield return InitBattleConnector();
                break;
            case "Close_loadingModal":
                innerModal.SetActive(false);
                _dequeueing = false;
                break;
            case "End":
                Destroy(gameObject);
                _dequeueing = false;
                break;
        }
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    private BattleConnector battleConnector;
    IEnumerator InitBattleConnector() {
        GameObject battleConnecterObj = new GameObject();
        battleConnecterObj.name = "BattleConnector";
        battleConnector = battleConnecterObj.AddComponent<BattleConnector>();
        
        battleConnector.OpenSocket(true);
        battleConnector.ForceDequeing(false);
        BattleConnector.OnOpenSocket.AddListener(() => _onOpenSocket = true);
        
        yield return LoadScene();
        yield return new WaitUntil(() => _onOpenSocket);
    }

    IEnumerator LoadScene() {
        string battleType = PlayerPrefs.GetString("SelectedBattleType");
        AsyncOperation asyncLoadScene;
        if (battleType == "story") {
            asyncLoadScene =
                SceneManager.LoadSceneAsync("TutorialScene", LoadSceneMode.Single);
        }
        else {
            asyncLoadScene =
                SceneManager.LoadSceneAsync("IngameScene", LoadSceneMode.Single);
        }

        asyncLoadScene.completed += OnSceneLoadComplete;
        yield return 0;
    }

    private void OnSceneLoadComplete(AsyncOperation obj) {
        battleConnector.ForceDequeing(false);
        _dequeueing = false;
    }

    private class PreSetting {
        public string method;
        private string[] args;

        public PreSetting(string method, string[] args = null) {
            this.method = method;
            if (args != null) this.args = args;
        }
    }
}

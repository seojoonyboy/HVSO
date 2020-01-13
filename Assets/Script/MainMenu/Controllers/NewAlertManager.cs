using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class NewAlertManager : MonoBehaviour
{
    public static NewAlertManager Instance;
    public GameObject alertPref;
    public List<string> buttonName;
    public Dictionary<string, GameObject> buttonDic;

    public string iconName = "NewAlertIcon";
    public string playerPrefKey = "AlertIcon";

    NoneIngameSceneEventHandler eventHandler;
    private void Awake() {
        Instance = this;
        eventHandler = NoneIngameSceneEventHandler.Instance;
    }

    private void OnDestroy() {
        if (Instance != null)
            Instance = null;
        if (buttonName.Count > 0) {
            SaveButtonName();
            buttonName.Clear();
        }       
    }

    private void SaveButtonName() {
        string nameData = "";
        for(int i = 0; i < buttonName.Count; i++) {
            nameData += buttonName[i];
            nameData += '/';
        }
        PlayerPrefs.SetString(playerPrefKey, nameData);
    }

    private void LoadButtonName() {
        string nameData = PlayerPrefs.GetString(playerPrefKey);
        string[] data = nameData.Split('/');
        buttonName.AddRange(data);
    }

    public void SetUpButtonToAlert(GameObject button) {
        buttonDic.Add(button.name, button);
        button.GetComponent<Button>()
            .OnClickAsObservable()
            .First()
            .Subscribe(_ => DisableButtonToAlert(button)).AddTo(button);

        button.SetActive(true);
    }

    //TODO
    //알람이 비활성화 되었다가 나중에 다시 알람이 필요한 경우에 대한 처리
    //기존대로 Destroy하면 안될 듯 (예 : 카드 메뉴에서 상자를 깐 이후에 카드 갯수 변화)
    //Dictionary Key값을 그냥 gameobject의 name으로 하면 nameData로 PlayerPrefab을 통해 관리할 때 
    //뭐가 뭔지 알 수 없을 것 같음... (체계가 필요) 그냥 id 붙여서 할까....
    //런타임 중에 동적으로 부모 객체가 생성되는 경우는 어떻하지... 뭐 예를 들어 영웅 조각을 모아 새로운 영웅을 생성했을 때
    private void DisableButtonToAlert(GameObject button) {
        //eventHandler.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.API_ALERT, this, butt)
    }
}

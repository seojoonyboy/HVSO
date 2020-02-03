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

    private void Awake() {
        Instance = this;
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
        Button targetButton = button.GetComponent<Button>();
        if (targetButton == null) return;
        if (button.transform.Find(iconName).gameObject != null) return;

        RectTransform rect = button.GetComponent<RectTransform>();
        Vector3[] buttonRect = new Vector3[4];
        rect.GetWorldCorners(buttonRect);

        Vector3 iconPos = new Vector3(rect.position.x + buttonRect[2].x / 2, buttonRect[2].y, 0);

        GameObject Icon = Instantiate(alertPref, rect);
        Icon.transform.position = iconPos;
        Icon.name = iconName;
        
        buttonDic.Add(button.name, button);
        targetButton.OnClickAsObservable().First().Subscribe(_ => DisableButtonToAlert(button)).AddTo(Icon);
        Icon.SetActive(true);
    }

    private void DisableButtonToAlert(GameObject button) {
        if (button.GetComponent<Button>() == null) return;
        Transform icon = button.transform.Find(iconName);
        if (icon == null) return;
        buttonDic.Remove(button.name);
        Destroy(icon.gameObject);
    }
}

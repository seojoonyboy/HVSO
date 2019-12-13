using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class NewAlertManager : MonoBehaviour
{
    public static NewAlertManager Instance;
    public GameObject alertPref;
    public static List<string> buttonName;
    public string iconName = "NewAlertIcon";
    
    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance != null)
            Instance = null;
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

        buttonName.Add(button.name);
        targetButton.OnClickAsObservable().Subscribe(_ => DisableButtonToAlert(button)).AddTo(Icon);
        Icon.SetActive(true);
    }

    public void DisableButtonToAlert(GameObject button) {
        if (button.GetComponent<Button>() == null) return;
        Transform icon = button.transform.Find(iconName);
        if (icon == null) return;
        buttonName.Remove(button.name);
        Destroy(icon.gameObject);
    }
}

public class AlartNameData {
    public List<string> menuNameList;
}

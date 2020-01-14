using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode()]
public partial class NewAlert : MonoBehaviour {
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/AddNewAlert")]
    public static void AddNewAlert() {
        GameObject obj = Instantiate(Resources.Load<GameObject>("UI/NewAlert"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
        obj.transform.SetAsLastSibling();
        obj.name = "NewAlertIcon";

        RectTransform rect = obj.transform.GetComponentInParent<RectTransform>();
        if(rect == null) {
            Logger.LogWarning("부모의 RectTransform를 찾을 수 없습니다!");
            return;
        }

        Vector3[] buttonRect = new Vector3[4];
        rect.GetWorldCorners(buttonRect);

        Vector3 iconPos = new Vector3(buttonRect[2].x, buttonRect[2].y, 0);
        obj.transform.position = iconPos;

        obj.SetActive(false);
    }

    /// <summary>
    /// Canvas를 추가하여 최상단으로 배치가 필요할 때
    /// </summary>
    [MenuItem("GameObject/UI/AddNewAlertWithCanvas")]
    public static void AddNewAlertWithCanvas() {
        GameObject obj = Instantiate(Resources.Load<GameObject>("UI/NewAlertWithCanvas"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
        obj.transform.SetAsLastSibling();
        obj.name = "NewAlertIcon";

        Canvas rootCanvas = obj.transform.root.GetComponent<Canvas>();
        int rootOrder = rootCanvas.sortingOrder;
        Canvas objCanvas = obj.GetComponent<Canvas>();
        if(objCanvas == null) {
            Logger.LogWarning("Root Canvas를 찾을 수 없습니다!");
            return;
        }
        objCanvas.sortingOrder = rootOrder + 1;

        RectTransform rect = obj.transform.parent.GetComponentInParent<RectTransform>();
        Vector3[] buttonRect = new Vector3[4];
        rect.GetWorldCorners(buttonRect);

        Vector3 iconPos = new Vector3(buttonRect[2].x, buttonRect[2].y, 0);
        obj.transform.GetChild(0).position = iconPos;

        obj.SetActive(false);
    }
#endif
}

public partial class NewAlert : MonoBehaviour {
    NoneIngameSceneEventHandler eventHandler;
    // Start is called before the first frame update
    void Start() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
        eventHandler.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ALERT, OnAlert);
    }

    private void OnAlert(Enum Event_Type, Component Sender, object Param) {

    }

    void OnDestroy() {
        eventHandler.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ALERT, OnAlert);
    }
}
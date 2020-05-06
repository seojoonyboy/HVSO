using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAlertMailListener : MonoBehaviour {
    [SerializeField] GameObject alert;
    public bool alertSettingFinished = false;
    
    void Start() {
        alertSettingFinished = false;
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_UPDATE, RequestMailOver);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_RECEIVE, RequestMailOver);
        AccountManager.Instance.RequestMailBox();
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_UPDATE, RequestMailOver);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_RECEIVE, RequestMailOver);
    }

    private void RequestMailOver(Enum Event_Type, Component Sender, object Param) {
        var totalMail = AccountManager.Instance.mailList;
        int mailCount = totalMail.Count;

        if (mailCount > 0) {
            alert.SetActive(true);
            alert.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = mailCount.ToString();
        }
        else alert.SetActive(false);

        alertSettingFinished = true;
    }
}

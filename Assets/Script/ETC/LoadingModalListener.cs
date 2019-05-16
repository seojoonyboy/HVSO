using System;
using UnityEngine;
using eventHandler = NoneIngameSceneEventHandler;

public class LoadingModalListener : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        eventHandler.Instance.AddListener(eventHandler.EVENT_TYPE.NETWORK_EROR_OCCURED, OnNetworkError);
    }

    private void OnNetworkError(Enum Event_Type, Component Sender, object Param) {
        Destroy(gameObject);
    }
}

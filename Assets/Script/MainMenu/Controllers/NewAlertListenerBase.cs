using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NewAlertListenerBase : MonoBehaviour {
    protected NoneIngameSceneEventHandler eventHandler;
    protected NewAlertManager alertManager;

    protected virtual void Awake() {
        Initialize();
        AddListener();
    }

    // Start is called before the first frame update
    protected virtual void Start() {
        alertManager = NewAlertManager.Instance;
    }

    public virtual void Initialize() {
        eventHandler = NoneIngameSceneEventHandler.Instance;
    }

    protected virtual void OnDestroy() {
        RemoveListener();
    }

    public abstract void AddListener();
    public abstract void RemoveListener();
}
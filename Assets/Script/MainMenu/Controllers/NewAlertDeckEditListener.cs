using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAlertDeckEditListener : NewAlertListenerBase {
    protected override void Awake() {
        base.Awake();
        alertManager = NewAlertManager.Instance;
    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
    }

    public override void AddListener() {

    }

    public override void RemoveListener() {

    }
}

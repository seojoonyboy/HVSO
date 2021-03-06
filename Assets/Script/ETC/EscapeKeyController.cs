using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EscapeKeyController : MonoBehaviour {

    public static EscapeKeyController escapeKeyCtrl;
    public List<System.Action> escapeFunc;
    private void Awake() {
        escapeKeyCtrl = this;
        escapeFunc = new List<System.Action>();
    }
    void Update() {
        if (escapeFunc.Count == 0) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        bool isTutorialOnGoing = System.Convert.ToBoolean(MainSceneStateHandler.Instance.GetState("IsTutorialOnGoing"));
        bool isTutorialFinished = System.Convert.ToBoolean(MainSceneStateHandler.Instance.GetState("IsTutorialFinished"));
        
        if(!isTutorialFinished || isTutorialOnGoing) return;
        
        escapeFunc[escapeFunc.Count - 1]();
    }

    public void AddEscape(System.Action function) {
        escapeFunc.Add(function);
    }

    public void RemoveEscape(System.Action function) {
        escapeFunc.Remove(function);
    }

    public void ResetEscapeList() {
        escapeFunc.Clear();
    }
}

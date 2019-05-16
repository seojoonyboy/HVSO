using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleHandler : MonoBehaviour {
    protected Toggle toggle;
    protected int id;
    protected AccountManager accountManager;

    [SerializeField] public BattleReadySceneController controller;
    // Start is called before the first frame update
    void Start() {
        toggle = GetComponent<Toggle>();
        id = GetComponent<IntergerIndex>().Id;
        accountManager = AccountManager.Instance;
    }

    // Update is called once per frame
    void Update() {

    }

    public virtual void OnValueChanged() {
        bool isOn = GetComponent<Toggle>().isOn;

        transform.Find("Selected").gameObject.SetActive(isOn);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class NewAlertManager : MonoBehaviour
{
    public static NewAlertManager Instance;
    public GameObject alertPref;

    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance != null)
            Instance = null;
    }

    public void SetUpButtonToAlert(GameObject button) {
        

    }



}

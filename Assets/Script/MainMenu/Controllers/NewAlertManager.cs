using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAlertManager : MonoBehaviour
{
    public static NewAlertManager Instance;

    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance != null)
            Instance = null;
    }



}

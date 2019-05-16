using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoadingModal : MonoBehaviour {
    public static GameObject instantiate(UnityAction function = null) {
        GameObject modal = Resources.Load("Prefabs/LoadingCanvas", typeof(GameObject)) as GameObject;
        Canvas canvas = (Canvas)FindObjectOfType(typeof(Canvas));

        GameObject obj = null;
        if(canvas != null) {
            obj = Instantiate(modal);
        }
        return obj;
    }
}

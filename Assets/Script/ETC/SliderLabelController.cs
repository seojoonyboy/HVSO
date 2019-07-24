using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderLabelController : MonoBehaviour {
    TMPro.TextMeshProUGUI text;

    void Start() {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void valueUpdate(float value) {
        text.text = value.ToString();
    }
}

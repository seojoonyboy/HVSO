using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Reflection;

public class QuestContentController : MonoBehaviour {
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI info;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderInfo;
    [SerializeField] private Button getBtn;
    [SerializeField] private Button rerollBtn;

    public QuestData data;
    
    private void Start() {
        if(data == null) return;
        title.text = data.title;
        info.text = data.info;
        slider.maxValue = (float)data.reachNum;
        slider.value = (float)data.currentNum;
        if(data.isClear) {
            getBtn.enabled = false;
            getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "진행중";
        }
        else {
            getBtn.enabled = false;
            getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "획득하기";
        }
    }

    private void GetReward() {
        gameObject.SetActive(false);
    }

    public void ActiveTutorial() {
        for(int i = 0; i < data.tutorials.Length; i++) {
            MethodInfo theMethod = this.GetType().GetMethod(data.tutorials[i].method);
            object[] args = data.tutorials[i].args;
            theMethod.Invoke(this, args);
        }
    }
}
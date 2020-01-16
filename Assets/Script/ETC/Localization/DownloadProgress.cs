using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DownloadProgress : MonoBehaviour {
    [SerializeField] Slider progressBar;
    [SerializeField] TextMeshProUGUI message;
    [SerializeField] TextMeshProUGUI label;

    public virtual void StartProgress() {
        if(!gameObject.activeSelf) gameObject.SetActive(true);
    }

    public virtual void OnProgress(long downloaded, long downloadLength) {
        progressBar.maxValue = downloadLength;
        progressBar.value = downloaded;
        label.text = downloaded + "/" + downloadLength;
    }

    public virtual void OnFinished() {
        if(gameObject.activeSelf) gameObject.SetActive(false);
    }

    void OnDisable() {
        label.text = string.Empty;
        message.text = string.Empty;
        progressBar.value = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameNotice : MonoBehaviour {
    private static IngameNotice _instance;
    public static IngameNotice instance {
        get {
            if (_instance == null) {
                _instance = (IngameNotice)FindObjectOfType(typeof(IngameNotice));
                if (_instance == null) {
                    Logger.LogWarning("아직 준비가 안됐습니다. 양해바랍니다.");
                }
            }
            return _instance;
        }
    }

    public void Awake() {
        _instance = this;
        gameObject.SetActive(false);
    }

    public void OnDestroy() {
        _instance = null;
    }

    private List<string> noticeList = new List<string>();
    [SerializeField] private TextMeshProUGUI noticeText;
    private float colorAlpha;

    private void Update() {
        noticeText.color = new Vector4(noticeText.color.r, noticeText.color.g, noticeText.color.b, colorAlpha);
        colorAlpha -= 0.005f;
        if (colorAlpha <= 0.01f) gameObject.SetActive(false);
    }

    public void SetNotice(string text) {
        colorAlpha = 1f;
        noticeText.text = text;
        gameObject.SetActive(true);
    }
}

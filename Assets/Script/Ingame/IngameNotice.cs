using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class IngameNotice : MonoBehaviour {
    private static IngameNotice _instance;
    public static IngameNotice instance {
        get {
            if (_instance == null) {
                _instance = (IngameNotice)FindObjectOfType(typeof(IngameNotice));
                if (_instance == null) {
                    //Logger.LogWarning("아직 준비가 안됐습니다. 양해바랍니다.");
                }
            }
            return _instance;
        }
    }
    private UnityAction update;

    public void Awake() {
        _instance = this;
        gameObject.SetActive(false);
        update = slowDown;
    }

    public void OnDestroy() {
        _instance = null;
    }

    private List<string> noticeList = new List<string>();
    [SerializeField] private TextMeshProUGUI noticeText;
    [SerializeField] private Image noticeImage;
    private float colorAlpha;

    private void Update() {
        update();
    }

    private void slowUp() {
        noticeText.color = new Vector4(noticeText.color.r, noticeText.color.g, noticeText.color.b, colorAlpha);
        colorAlpha += 0.005f;
        if (colorAlpha >= 0.95f) update = slowDown;
    }

    private void slowDown() {
        noticeText.color = new Vector4(noticeText.color.r, noticeText.color.g, noticeText.color.b, colorAlpha);
        colorAlpha -= 0.005f;
        if (colorAlpha <= 0.05f) update = slowUp;
    }

    public void SetNotice(string text) {
        noticeImage.gameObject.SetActive(false);
        noticeText.gameObject.SetActive(true);
        colorAlpha = 1f;
        noticeText.text = text;
        gameObject.SetActive(true);
    }

    public void SetNotice() {
        noticeText.gameObject.SetActive(false);
        noticeImage.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void CloseNotice() {
        gameObject.SetActive(false);
        colorAlpha = 1f;
        noticeText.color = new Vector4(noticeText.color.r, noticeText.color.g, noticeText.color.b, colorAlpha);
        update = slowDown;
    }
}

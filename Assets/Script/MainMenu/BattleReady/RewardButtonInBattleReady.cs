using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class RewardButtonInBattleReady : MonoBehaviour {
    Image image;
    IDisposable observer;
    [SerializeField] RewardProgressController progressController;
    [SerializeField] GameObject progressBar;
    [SerializeField] Sprite[] toggleImages;

    RectTransform rect;
    private void Awake() {
        rect = GetComponent<RectTransform>();
        image = transform.Find("Image").GetComponent<Image>();
    }

    IEnumerator Start() {
        yield return new WaitForEndOfFrame();
        MMRChanged();

        observer = Observable
            .EveryUpdate()
            .Where(x => progressController.isMoving == true)
            .Subscribe(_ => MMRChanged());
    }

    private void MMRChanged() {
        var barX = progressBar.GetComponent<RectTransform>().rect.width + 50;

        if(rect.localPosition.x >= barX) {
            image.sprite = toggleImages[0];
        }
        else {
            image.sprite = toggleImages[1];
        }
    }

    private void OnDestroy() {
        if(observer != null) observer.Dispose();
    }
}

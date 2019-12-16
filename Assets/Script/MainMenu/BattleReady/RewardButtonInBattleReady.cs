using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using BestHTTP;

public class RewardButtonInBattleReady : MonoBehaviour {
    Image image;
    IDisposable observer;
    [SerializeField] RewardProgressController progressController;
    [SerializeField] GameObject progressBar;
    [SerializeField] Sprite[] toggleImages;

    RectTransform rect;
    public AccountManager.Reward rewardData;

    bool rewardAvailable;
    bool rewardDataLoaded;

    private void Awake() {
        rect = GetComponent<RectTransform>();
        image = transform.Find("Image").GetComponent<Image>();

        rewardAvailable = false;
    }

    IEnumerator Start() {
        yield return new WaitUntil(() => rewardDataLoaded);
        yield return new WaitForEndOfFrame();
        MMRChanged();

        observer = Observable
            .EveryUpdate()
            .Where(x => progressController.isMoving == true)
            .Subscribe(_ => MMRChanged());
    }

    private Color32 deactiveColor = new Color32(70, 70, 70, 255);
    private Color32 activeColor = new Color32(255, 255, 255, 255);
    private void MMRChanged() {
        var barX = progressBar.GetComponent<RectTransform>().rect.width + 50;

        //조건이 안됨
        if (!rewardData.canClaim) {
            image.sprite = toggleImages[0];
            image.color = activeColor;
            transform.Find("CheckMark").gameObject.SetActive(false);
            rewardAvailable = false;
        }
        //조건이 됨
        else {
            //받지 않았음
            if (!rewardData.claimed) {
                image.sprite = toggleImages[1];
                image.color = activeColor;
                transform.Find("CheckMark").gameObject.SetActive(false);
                rewardAvailable = true;
            }
            //이미 받았음
            else {
                image.sprite = toggleImages[0];
                image.color = deactiveColor;
                transform.Find("CheckMark").gameObject.SetActive(true);
                rewardAvailable = false;
            }
        }
    }

    private void OnDestroy() {
        if(observer != null) observer.Dispose();
    }

    public void RequestReward() {
        if (!rewardAvailable) return;

        int id = GetComponentInChildren<dataModules.IntergerIndex>().Id;
        AccountManager.Instance.RequestLeagueReward(OnRewardCallback, id);
    }

    private void OnRewardCallback(HTTPRequest originalRequest, HTTPResponse response) {
        Logger.Log("OnRewardCallback : " + response.DataAsText);
        if(response.DataAsText.Contains("not allowed")) {
            Modal.instantiate("요청 불가", Modal.Type.CHECK);
        }
        else {
            Modal.instantiate("우편으로 발송되었습니다.", Modal.Type.CHECK, () => {

            });
        }
    }

    public void SetRewardData(AccountManager.Reward reward) {
        rewardData = reward;
        rewardDataLoaded = true;
    }
}

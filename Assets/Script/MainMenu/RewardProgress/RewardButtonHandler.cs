using BestHTTP;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class RewardButtonHandler : MonoBehaviour {
    private Color32 deactiveColor = new Color32(70, 70, 70, 255);
    private Color32 activeColor = new Color32(255, 255, 255, 255);

    private float ratingPoint;
    private int id;

    AccountManager.Reward reward;

    Transform glow, checkmark;
    Animator animator;

    void Awake() {
        glow = transform.Find("Glow");
        animator = glow.GetComponent<Animator>();
        checkmark = transform.Find("Check");
    }

    public void Init(AccountManager.Reward reward) {
        this.reward = reward;
        id = reward.id;
        ratingPoint = reward.point;

        CheckRecivable();
    }

    public void OnClick() {
        if (reward.canClaim == false) return;
        Fbl_Translator fbl_Translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string message = "보상을 수령하시겠습니까?";
        Modal.instantiate(message, Modal.Type.YESNO, () => {
            AccountManager.Instance.RequestLeagueReward(OnRewardCallBack, id);
        });
    }

    private void OnRewardCallBack(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.DataAsText.Contains("not allowed")) {
            Modal.instantiate("요청 불가", Modal.Type.CHECK);
        }
        else {
            var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
            string message = translator.GetLocalizedText("UIPopup", "ui_popup_mailsent");
            string okBtn = translator.GetLocalizedText("UIPopup", "ui_popup_check");
            string header = translator.GetLocalizedText("UIPopup", "ui_popup_check");

            Modal.instantiate(message, Modal.Type.CHECK, () => { }, headerText: header, btnTexts: new string[] { okBtn });

            RewardProgressController rewardProgressController = GetComponentInParent<RewardProgressController>();
            StartCoroutine(rewardProgressController.StartSetting());
        }
    }

    public void CheckRecivable() {
        transform.Find("Indicator").gameObject.SetActive(true);
        transform.Find("Indicator2").gameObject.SetActive(false);

        if (reward.canClaim) {
            if(!reward.claimed) {
                checkmark.gameObject.SetActive(false);
                animator.enabled = true;
                GetComponent<Button>().enabled = true;

                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else {
                checkmark.gameObject.SetActive(true);
                animator.GetComponent<Image>().enabled = false;
                animator.enabled = false;
                GetComponent<Button>().enabled = false;

                transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);

                transform.Find("Indicator").gameObject.SetActive(false);
                transform.Find("Indicator2").gameObject.SetActive(true);
            }
        }
        else {
            checkmark.gameObject.SetActive(false);
            animator.GetComponent<Image>().enabled = false;
            animator.enabled = false;
            GetComponent<Button>().enabled = false;

            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }
}

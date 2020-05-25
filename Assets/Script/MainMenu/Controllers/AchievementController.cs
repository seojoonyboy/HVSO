using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Haegin;
using HaeginGame;
using System;

public class AchievementController : MonoBehaviour {
    [SerializeField] private Sprite google;
    [SerializeField] private Sprite gamecenter;
    [SerializeField] private FblTextConverter textConverter;
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject hideModal;
    [SerializeField] private AccountDialogController accountDialog;

    private void Awake() {
#if UNITY_IOS
        image.sprite = gamecenter;
        image.rectTransform.sizeDelta = new Vector2(64f,64f);
        textConverter.Init("MainUI", "ui_page_setting_gamecenter", FblTextConverter.TextType.TEXTMESHPROUGUI);
        textConverter.RefreshText();
#elif UNITY_ANDROID
        image.sprite = google;
#endif
        button.onClick.AddListener(OpenAchievement);
    }

    private void OpenAchievement() {
        if(!Account.IsLoggedInGameService()) ShouldLogin();
        else GameServiceAchievements.ShowAchievementsUI();
    }

    private void ShouldLogin() {
        GameObject modal = MonoBehaviour.Instantiate(hideModal, canvas.transform);
		Account.LoginAccount(Account.HaeginAccountType.AppleGameCenter, accountDialog.OpenSelectDialog, (bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid) => {
#if MDEBUG
            Debug.Log("LogintAccount  result=" + result + "    code=" + code + " blockSuid=" + blockSuid);
#endif
            MonoBehaviour.Destroy(modal);
            if (result && code == WebClient.AuthCode.SUCCESS)
                LoginComplete();
            else
                Modal.instantiate("로그인 과정에서 문제가 발생했습니다.", Modal.Type.CHECK);
        });
    }

    private void LoginComplete() {
        WebClient.GetInstance().Request(new IssueJWTReq());
    }
}

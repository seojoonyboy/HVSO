using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLeagueInfoCanvasController : MonoBehaviour {
    [SerializeField] Transform myInfoArea, leaderBoardArea;
    [SerializeField] Transform rankingContent;
    [SerializeField] GameObject mmrSliderArea;
    [SerializeField] GameObject rankingPool;
    [SerializeField] HUDController hudController;
    [SerializeField] BattleReadySceneController battleReadySceneController;
    // Start is called before the first frame update
    void Start() {

    }

    void OnPanel() {
        gameObject.SetActive(true);

        EscapeKeyController.escapeKeyCtrl.AddEscape(OffPanel);
    }

    public void OnPanelByMain() {
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            OffPanelByMain();
        });

        OnPanel();
    }

    public void OnPanelByBattleReady() {
        hudController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
        hudController.SetBackButton(() => {
            OffPanelByBattleReady();
        });

        OnPanel();
    }

    void OffPanel() {
        gameObject.SetActive(false);

        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OffPanel);
    }

    public void OffPanelByBattleReady() {
        hudController.SetBackButton(() => battleReadySceneController.OnBackButton());
        OffPanel();
    }

    public void OffPanelByMain() {
        hudController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        OffPanel();
    }
}

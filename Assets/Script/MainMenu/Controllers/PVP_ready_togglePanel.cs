using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVP_ready_togglePanel : TogglePanel {
    [SerializeField] GameObject 
        CasualPanel,
        RankPanel;
    [SerializeField] GameObject
        PlantDeckPanel,
        ZombieDeckPanel;
    [SerializeField] EasyTween 
        casualPanelClickOpen,
        casualPanelClickClose,
        rankPanelClickOpen,
        rankPanelClickClose;
    [SerializeField] EasyTween
        plantDeckClickOpen,
        plantDeckClickClose,
        zombieDeckClickOpen,
        zombieDeckClickClose;

    //TODO : 선택된 덱과 매칭 종류 관리
    
    void Start() {
        casualPanelClickOpen.OpenCloseObjectAnimation();
        plantDeckClickOpen.OpenCloseObjectAnimation();
    }

    public void InitializeCasualPanel() {
        CasualPanel.SetActive(true);
        RankPanel.SetActive(false);
    }

    public void InitializeRankPanel() {
        RankPanel.SetActive(true);
        CasualPanel.SetActive(false);
    }

    public void InitializePlantDeckPanel() {
        PlantDeckPanel.SetActive(true);
        ZombieDeckPanel.SetActive(false);
    }

    public void InitializeZombieDeckPanel() {
        ZombieDeckPanel.SetActive(true);
        PlantDeckPanel.SetActive(false);
    }
}

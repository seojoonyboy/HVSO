using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using dataModules;
using System;
using Spine.Unity;

public class StoryEnemyHeroInfo : MonoBehaviour {
    [SerializeField] HUDController HUDController;
    [SerializeField] ScenarioManager scenarioManager;

    [SerializeField] TextMeshProUGUI name;
    [SerializeField] Transform 
        heroSpinesParent,
        storyInfo, 
        skillInfo,
        abilityInfo,
        heroDialog,
        buttons,
        BannerImage;

    [SerializeField] Sprite[] backgrounds;
    string enemyHeroId;

    void OnEnable() {
        HUDController.SetBackButton(OnEscapeButton);
        EscapeKeyController.escapeKeyCtrl.AddEscape(OnEscapeButton);
        SetDefaultState();

        foreach (Transform spineTf in heroSpinesParent) {
            if(spineTf.name == enemyHeroId) {
                spineTf.gameObject.SetActive(true);
            }
        }
    }

    void OnEscapeButton() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnEscapeButton);
        scenarioManager.SetBackButton(2);
        gameObject.SetActive(false);
    }

    void OnDisable() {
        foreach (Transform spineTf in heroSpinesParent) {
            spineTf.gameObject.SetActive(false);
        }

        storyInfo.Find("Description").GetComponent<TextMeshProUGUI>().text = "";
    }

    private void SetDefaultState() {
        buttons.GetChild(0).GetComponent<Button>().onClick.Invoke();
    }

    public void SetData(object _data) {
        object[] data = (object[])_data;
        bool isHuman = (bool)data[0];

        if (!isHuman) {
            transform.Find("BackGround").GetComponent<Image>().sprite = backgrounds[0];
            BannerImage.transform.Find("Orc").gameObject.SetActive(false);
            BannerImage.transform.Find("Human").gameObject.SetActive(true);
        }
        else {
            transform.Find("BackGround").GetComponent<Image>().sprite = backgrounds[1];
            BannerImage.transform.Find("Orc").gameObject.SetActive(true);
            BannerImage.transform.Find("Human").gameObject.SetActive(false);
        }

        StageButton stageButton = (StageButton)data[1];

        string enemyHeroId = stageButton.chapterData.enemyHeroId;
        this.enemyHeroId = enemyHeroId;
        name.text = GetHeroName(enemyHeroId);
        var desc = GetStoryDescription(enemyHeroId);

        if(desc != null) {
            storyInfo.Find("Description").GetComponent<TextMeshProUGUI>().text = desc;
        }
    }

    private string GetStoryDescription(string heroId) {
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string key = heroId
            .Contains("qh") ? "hero_npc_" + heroId + "_backstory" : "hero_pc_" + heroId + "_backstory";
        string result = translator.GetLocalizedText("Hero", key);
        return result;
    }

    private string GetHeroName(string heroId) {
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string key = heroId
            .Contains("qh") ? "hero_npc_" + heroId + "_name" : "hero_pc_" + heroId + "_name";
        string result = translator.GetLocalizedText("Hero", key);
        return result;
    }
}

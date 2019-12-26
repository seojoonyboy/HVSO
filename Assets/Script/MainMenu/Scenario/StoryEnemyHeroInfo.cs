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
    List<HeroDescription> heroDescriptions;
    string enemyHeroId;

    // Start is called before the first frame update
    void Start() {

    }

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
        ReadFile();

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

    private void ReadFile() {
        string dataAsJson = ((TextAsset)Resources.Load("TutorialDatas/HeroFlavourDatas")).text;
        heroDescriptions = JsonReader.Read<List<HeroDescription>>(dataAsJson);
    }

    private string GetStoryDescription(string heroId) {
        var desc = heroDescriptions.Find(x => x.id == heroId);
        if (desc == null) return null;
        return desc.description;
    }

    private string GetHeroName(string heroId) {
        switch (heroId) {
            case "qh10001":
                return "레이 첸 민";
            case "qh10002":
                return "오크 부족장";
            default:
                return "";
        }
    }

    public class HeroDescription {
        public string id;
        public string description;
    }
}

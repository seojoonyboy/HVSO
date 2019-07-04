using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using dataModules;
using TMPro;

public class TemplateMenu : MonoBehaviour {
    [SerializeField]
    private GameObject heroButtonLayout;
    private GameObject heroPortrait;
    private TextMeshProUGUI heroName;
    private GameObject heroProperty;
    private GameObject heroCardGroup;
    [SerializeField]
    private GameObject deckLayout;

    private string previewID;

    private void Awake() {
        Transform upper = transform.Find("Upper");
        Transform footer = transform.Find("Footer");
        Transform heroSelect = upper.Find("HeroSelect");

        heroButtonLayout = heroSelect.Find("HeroButton").Find("HeroBtnLayout").gameObject;
        heroPortrait = heroSelect.Find("Portrait").gameObject;
        heroName = heroSelect.Find("NameTamplate").GetComponentInChildren<TextMeshProUGUI>();
        heroProperty = heroSelect.Find("HeroProperty").gameObject;
        heroCardGroup = upper.Find("HeroCard").gameObject;
        deckLayout = footer.Find("HeroBtnLayout").gameObject;

        SetHeroBtnID();
    }


    public void SetHeroBtnID() {
        if (AccountManager.Instance == null) return;
        ResourceManager resource = AccountManager.Instance.resource;

        bool isHuman = (gameObject.name.Contains("Human") == true) ? true : false;
        Transform btnLayout = heroButtonLayout.transform;
        int count = 0;

        foreach (Transform child in btnLayout) {
            TemplateHeroBtn templateHeroBtn = child.gameObject.AddComponent<TemplateHeroBtn>();
            Button button = child.gameObject.GetComponent<Button>();

            if (isHuman == true)
                templateHeroBtn.heroID = (child.GetSiblingIndex() == 0) ? "h10001" : "h10003";
            else
                templateHeroBtn.heroID = (child.GetSiblingIndex() == 0) ? "h10002" : "h10004";

            templateHeroBtn.menu = this;
            if (button != null)
                button.onClick.AddListener(delegate () { templateHeroBtn.HeroSelectBtn(); });

            if (count < 2) {
                GameObject skeletonData = resource.heroPreview[templateHeroBtn.heroID].gameObject;
                GameObject preview = Instantiate(skeletonData, heroPortrait.transform);
                preview.transform.position = heroPortrait.transform.position;
                preview.SetActive((count == 0) ? true : false);
                preview.name = templateHeroBtn.heroID;
            }

            if (count > 0)
                button.enabled = false;

            count++;
        }
        previewID = (isHuman == true) ? "h10001" : "h10002";
    }

    public void ChangeHeroID(string heroID) {
        string id = heroID;
        ChangeHeroSkeleton(id);
        ChangeHeroData(id);
    }

    private void ChangeHeroSkeleton(string heroID) {
        heroPortrait.transform.Find(previewID).gameObject.SetActive(false);
        heroPortrait.transform.Find(heroID).gameObject.SetActive(true);
        previewID = heroID;
    }

    private void ChangeHeroData(string heroID) {
        HeroInventory heroData = AccountManager.Instance.myHeroInventories[heroID];
        int cardCount = 0;

        foreach (HeroCard card in heroData.heroCards) {
            Transform heroCardObject = heroCardGroup.transform.GetChild(cardCount);

            heroCardObject.Find("Name").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = card.name;
            heroCardObject.Find("Cost").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = card.cost.ToString();
            cardCount++;
        }

        int childcount = 0;
        foreach (Transform child in heroProperty.transform) {
            child.gameObject.GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[heroData.heroClasses[childcount]];
            child.GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + heroData.heroClasses[childcount]];

            childcount++;
        }
    }

    public void SelectDeckBtn() {

    }

    public void StartEditBtn() {

    }


    public void ReturnToMenu() {
        gameObject.SetActive(false);
    }
}

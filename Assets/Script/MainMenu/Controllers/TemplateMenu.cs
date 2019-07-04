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
    MyDecksLoader decksLoader;

    [SerializeField]
    private GameObject deckLayout;

    private string previewID;
    public bool isHuman;
    private void Awake() {
        Transform upper = transform.Find("Upper");
        Transform footer = transform.Find("Footer");
        Transform heroSelect = upper.Find("HeroSelect");

        heroButtonLayout = heroSelect.Find("HeroButton/HeroBtnLayout").gameObject;
        heroPortrait = heroSelect.Find("Portrait").gameObject;
        heroName = heroSelect.Find("NameTamplate").GetComponentInChildren<TextMeshProUGUI>();
        heroProperty = heroSelect.Find("HeroProperty").gameObject;
        heroCardGroup = upper.Find("HeroCard").gameObject;
        deckLayout = footer.Find("HeroBtnLayout").gameObject;
        gameObject.SetActive(false);
        SetHeroBtnID();
    }

    public void AttachDecksLoader(ref MyDecksLoader decksLoader) {
        this.decksLoader = decksLoader;
        this.decksLoader.OnLoadFinished.AddListener(() => { ChangeHeroID("h10001"); });
    }


    public void SetHeroBtnID() {
        if (AccountManager.Instance == null) return;
        ResourceManager resource = AccountManager.Instance.resource;

        isHuman = (gameObject.name.Contains("Human") == true) ? true : false;
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
            count++;
        }
        previewID = (isHuman == true) ? "h10001" : "h10002";
    }

    public void ChangeHeroID(string heroID) {
        string id = heroID;
        ChangeHeroSkeleton(id);
        ChangeHeroCard(id);
    }

    private void ChangeHeroSkeleton(string heroID) {
        heroPortrait.transform.Find(previewID).gameObject.SetActive(false);
        heroPortrait.transform.Find(heroID).gameObject.SetActive(true);
    }

    private void ChangeHeroCard(string heroID) {
        HeroInventory heroData = AccountManager.Instance.myHeroInventories[heroID];
        int cardCount = 0;

        foreach (HeroCard card in heroData.heroCards) {
            Transform heroCardObject = heroCardGroup.transform.GetChild(cardCount);
            heroCardObject.GetComponent<MenuCardHandler>().DrawCard(card.cardId, isHuman);
            cardCount++;
        }
        previewID = heroID;
    }



    public void SelectDeckBtn() {

    }

    public void StartEditBtn() {

    }


    public void ReturnToMenu() {
        gameObject.SetActive(false);
    }
}

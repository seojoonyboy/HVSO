using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using dataModules;
using TMPro;

public class TamplateMenu : MonoBehaviour
{
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

        heroButtonLayout = heroSelect.Find("HeroButton").gameObject;
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
            TamplateHeroBtn tamplateHeroBtn = child.gameObject.AddComponent<TamplateHeroBtn>();
            Button button = child.gameObject.GetComponent<Button>();

            if (isHuman == true)
                tamplateHeroBtn.heroID = (child.GetSiblingIndex() == 0) ? "h10001" : "h10003";
            else
                tamplateHeroBtn.heroID = (child.GetSiblingIndex() == 0) ? "h10002" : "h10004";

            tamplateHeroBtn.menu = this;
            if (button != null)
                button.onClick.AddListener(delegate () { tamplateHeroBtn.HeroSelectBtn(); });

            if(count < 2) {
                GameObject skeletonData = resource.heroPreview[tamplateHeroBtn.heroID].gameObject;
                GameObject preview = Instantiate(skeletonData, heroPortrait.transform);
                preview.transform.position = heroPortrait.transform.position;
                preview.SetActive((count == 0) ? true : false);
                preview.name = tamplateHeroBtn.heroID;
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

            heroCardObject.Find("Name").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = card.name;
            heroCardObject.Find("Cost").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = card.cost.ToString();
            cardCount++;
        }
        previewID = heroID;
    }



    public void SelectDeckBtn() {

    }

    public void StartEditBtn() {

    }
    
}

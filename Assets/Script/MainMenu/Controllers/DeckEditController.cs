using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using dataModules;
using TMPro;

public class DeckEditController : MonoBehaviour
{

    public string heroID;
    private GameObject heroInfo;
    private GameObject heroCardGroup;
    private GameObject heroProperty;

    private GameObject settingLayout;
    private GameObject ownCardLayout;
    private GameObject UnReleaseCardLayout;

    private GameObject deckNamePanel;

    private void Awake() {
        SetObject();
        SetHeroData();
    }


    public void NewDeck() {

    }

    public void LoadEditDeck() {

    }
    
    public void ConfirmButton() {

    }

    public void CancelButton() {

    }



    private void SetObject() {
        deckNamePanel = transform.Find("DeckNamePanel").gameObject;
        heroInfo = transform.Find("HeroStatus").gameObject;
        heroCardGroup = transform.Find("HeroCard").gameObject;
        heroProperty = transform.Find("HeroProperty").gameObject;

        settingLayout = transform.Find("SetDeckLayout").Find("Content").gameObject;
        ownCardLayout = transform.Find("CardPanel").Find("Viewport").Find("Content").Find("OwnCard").gameObject;
        UnReleaseCardLayout = transform.Find("CardPanel").Find("Viewport").Find("Content").Find("MissingCard").gameObject;

        transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(delegate () { ConfirmButton(); });
        transform.Find("CancelButton").GetComponent<Button>().onClick.AddListener(delegate () { CancelButton(); });
    }

    public void SetHeroData() {
        if (AccountManager.Instance == null) return;
        ResourceManager resource = AccountManager.Instance.resource;

        HeroInventory heroData = AccountManager.Instance.myHeroInventories["h10001"];
        int cardCount = 0;

        heroInfo.transform.Find("Streamer").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = heroData.name;

        foreach (HeroCard card in heroData.heroCards) {
            Transform heroCardObject = heroCardGroup.transform.GetChild(cardCount);

            heroCardObject.Find("NameTemplate").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = card.name;
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



    public void ReturnButton() {


    }

}


/*
public interface LoadEdit {
    void LoadCardData();
}


public class NewDeck : MonoBehaviour, LoadEdit {

    public void LoadCardData() {

    }
}

public class EditDeck : MonoBehaviour, LoadEdit {

    public void LoadCardData() {

    }
}

public class GetDeck : MonoBehaviour {

    private LoadEdit loadEdit;    

    public void Execute(LoadEdit deck) {
        loadEdit = deck;

        if (loadEdit == null)
            Logger.Log("에러!");
        else
            loadEdit.LoadCardData();
    }
}
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine;
using Spine.Unity;

public class MenuCardHandler : MonoBehaviour {
    public string cardID;
    //[SerializeField] MenuCardInfo menuCardInfo;
    [SerializeField] GameObject exitTrigger2;
    dataModules.CollectionCard cardData;
    GameObject skeleton;
    bool isHuman;
    public string CARDID {
        get { return cardID; }
        set { cardID = value; }
    }

    public void DrawCard(string id, bool isHuman) {
        this.isHuman = isHuman;
        cardID = id;
        cardData = AccountManager.Instance.allCardsDic[cardID];
        Transform cardObject;
        if (cardData.type == "unit") {
            transform.Find("MagicEditCard").gameObject.SetActive(false);
            transform.Find("ToolEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("UnitEditCard");
        }
        else if (cardData.type == "magic" && !cardData.isHeroCard) {
            transform.Find("UnitEditCard").gameObject.SetActive(false);
            transform.Find("ToolEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("MagicEditCard");
        }
        else if (cardData.type == "tool" && !cardData.isHeroCard) {
            transform.Find("UnitEditCard").gameObject.SetActive(false);
            transform.Find("MagicEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("ToolEditCard");
        }
        else
            cardObject = transform;
        cardObject.gameObject.SetActive(true);

        if (cardData.rarelity == "legend")
            cardObject.SetAsFirstSibling();
        Sprite portraitImage = null;
        if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardID))
            portraitImage = AccountManager.Instance.resource.cardPortraite[cardID] != null ? AccountManager.Instance.resource.cardPortraite[cardID] : AccountManager.Instance.resource.cardPortraite["ac10065"];
        else portraitImage = AccountManager.Instance.resource.cardPortraite["ac10065"];
        cardObject.Find("Portrait").GetComponent<Image>().sprite = portraitImage;
        if (!cardData.isHeroCard) {
            Logger.Log(cardData.type + "_" + cardData.rarelity);
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
        }
        else {
            string race;
            if (isHuman)
                race = "_human";
            else
                race = "_orc";
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race];
        }

        if (cardData.type == "unit") {
            //Logger.Log(cardData.name);
            cardObject.Find("Health/Text").GetComponent<Text>().text = cardData.hp.ToString();
            cardObject.Find("attack/Text").GetComponent<Text>().text = cardData.attack.ToString();
            if (cardData.attributes.Length == 0)
                cardObject.Find("SkillIcon").gameObject.SetActive(false);
            else {
                cardObject.Find("SkillIcon").gameObject.SetActive(true);
                if (cardData.attributes.Length == 1)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attributes[0].name];
                else if (cardData.attributes.Length > 1)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons["complex"];
            }
        }
        cardObject.Find("Cost/Text").GetComponent<Text>().text = cardData.cost.ToString();
        //cardObject.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.cardClasses[0]];
        if (cardData.isHeroCard) return;
        transform.Find("HaveNum").GetComponent<SkeletonGraphic>().Initialize(false);
        Spine.AnimationState aniState = transform.Find("HaveNum").GetComponent<SkeletonGraphic>().AnimationState;
        if (AccountManager.Instance.cardPackage.data.ContainsKey(cardID)) {
            aniState.SetAnimation(0, AccountManager.Instance.cardPackage.data[cardID].cardCount.ToString(), false);
            cardObject.Find("Disabled").gameObject.SetActive(false);
        }
        else {
            cardObject.Find("Disabled").gameObject.SetActive(true);
            if (cardData.type == "unit") {
                if (cardObject.Find("SkillIcon").gameObject.activeSelf) {
                    cardObject.Find("Disabled/HaveAbility").gameObject.SetActive(true);
                    cardObject.Find("Disabled/NonAbility").gameObject.SetActive(false);
                }
                else {
                    cardObject.Find("Disabled/HaveAbility").gameObject.SetActive(false);
                    cardObject.Find("Disabled/NonAbility").gameObject.SetActive(true);
                }
            }
            aniState.SetAnimation(0, "NOANI", false);
        }
        if(NewAlertManager.Instance.GetUnlockCondionsList().Exists(x => x.Contains("DICTIONARY_card_" + id)))
            transform.Find("NewCard").gameObject.SetActive(true); 
    }

    public void DrawCard(string id) {
        cardID = id;
        cardData = AccountManager.Instance.allCardsDic[cardID];
        Transform cardObject;
        if (cardData.type == "unit") {
            transform.Find("MagicEditCard").gameObject.SetActive(false);
            transform.Find("ToolEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("UnitEditCard");
        }
        else if (cardData.type == "magic" && !cardData.isHeroCard) {
            transform.Find("UnitEditCard").gameObject.SetActive(false);
            transform.Find("ToolEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("MagicEditCard");
        }
        else if (cardData.type == "tool" && !cardData.isHeroCard) {
            transform.Find("UnitEditCard").gameObject.SetActive(false);
            transform.Find("MagicEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("ToolEditCard");
        }
        else
            cardObject = transform;
        cardObject.gameObject.SetActive(true);
        if (cardData.rarelity == "legend")
            cardObject.SetAsFirstSibling();
        Sprite portraitImage = null;
        if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardID)) portraitImage = AccountManager.Instance.resource.cardPortraite[cardID] != null ? AccountManager.Instance.resource.cardPortraite[cardID] : AccountManager.Instance.resource.cardPortraite["ac10065"];
        else portraitImage = AccountManager.Instance.resource.cardPortraite["ac10065"];
        cardObject.Find("Portrait").GetComponent<Image>().sprite = portraitImage;
        if (!cardData.isHeroCard) {
            if(cardData.type != "tool") cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
        }
        else {
            string race;
            if (isHuman)
                race = "_human";
            else
                race = "_orc";
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race];
        }

        if (cardData.type == "unit") {
            //Logger.Log(cardData.name);
            cardObject.Find("Health/Text").GetComponent<Text>().text = cardData.hp.ToString();
            cardObject.Find("attack/Text").GetComponent<Text>().text = cardData.attack.ToString();
            if (cardData.attributes.Length == 0)
                cardObject.Find("SkillIcon").gameObject.SetActive(false);
            else {
                cardObject.Find("SkillIcon").gameObject.SetActive(true);
                if (cardData.attributes.Length == 1)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attributes[0].name];
                else if (cardData.attributes.Length > 0)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons["complex"];
            }
        }
        cardObject.Find("Cost/Text").GetComponent<Text>().text = cardData.cost.ToString();
        //cardObject.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.cardClasses[0]];
        //if (!cardData.isHeroCard)
        //    transform.Find("HaveNum").gameObject.SetActive(false);
        cardObject.Find("Disabled").gameObject.SetActive(false);
    }

    public void OpenCardInfo() {
        MenuCardInfo.cardInfoWindow.transform.parent.gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.gameObject.SetActive(true);
        if(transform.Find("NewCard") != null && transform.Find("NewCard").gameObject.activeSelf) {
            NewAlertManager
                .Instance
                .CheckRemovable(NewAlertManager.ButtonName.DICTIONARY, CARDID);
            transform.Find("NewCard").gameObject.SetActive(false);
        }
        MenuCardInfo.cardInfoWindow.SetCardInfo(cardData, isHuman, transform);
        if (transform.parent.parent.parent.name == "HeroInfo" && transform.parent.parent.name != "SkillWindow") {
            exitTrigger2.SetActive(true);
            EscapeKeyController.escapeKeyCtrl.AddEscape(MenuCardInfo.cardInfoWindow.CloseHeroesCardInfo);
        }
        else
            EscapeKeyController.escapeKeyCtrl.AddEscape(MenuCardInfo.cardInfoWindow.CloseInfo);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine;
using Spine.Unity;

public class MenuCardHandler : MonoBehaviour {
    public string cardID;
    [SerializeField] MenuCardInfo menuCardInfo;
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
            cardObject = transform.Find("UnitEditCard");
        }
        else if (cardData.type == "magic" && !cardData.isHeroCard) {
            transform.Find("UnitEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("MagicEditCard");
        }
        else
            cardObject = transform;
        cardObject.gameObject.SetActive(true);
        if (cardData.rarelity == "legend")
            cardObject.SetAsFirstSibling();
        Sprite portraitImage = null;
        if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardID)) portraitImage = AccountManager.Instance.resource.cardPortraite[cardID];
        else portraitImage = AccountManager.Instance.resource.cardPortraite["default"];
        cardObject.Find("Portrait").GetComponent<Image>().sprite = portraitImage;
        if (!cardData.isHeroCard) {
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
            cardObject.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["name_" + cardData.rarelity];
        }
        else {
            string race;
            if (isHuman)
                race = "_human";
            else
                race = "_orc";
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race];
            cardObject.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race + "_name"];
        }

        if (cardData.type == "unit") {
            //Logger.Log(cardData.name);
            cardObject.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.hp.ToString();
            cardObject.Find("attack/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.attack.ToString();
            if (cardData.attributes.Length == 0 && cardData.attackTypes.Length == 0)
                cardObject.Find("SkillIcon").gameObject.SetActive(false);
            else {
                cardObject.Find("SkillIcon").gameObject.SetActive(true);
                if (cardData.attributes.Length != 0)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attributes[0]];
                if (cardData.attackTypes.Length != 0)
                    if (AccountManager.Instance.resource.skillIcons.ContainsKey(cardData.attackTypes[0])) {
                        cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attackTypes[0]];
                    }
                if (cardData.attributes.Length != 0 && cardData.attackTypes.Length != 0)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons["complex"];
            }
        }
        cardObject.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.cost.ToString();
        //cardObject.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.cardClasses[0]];
        cardObject.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
        if (cardData.isHeroCard) return;
        transform.Find("HaveNum").GetComponent<SkeletonGraphic>().Initialize(false);
        Spine.AnimationState aniState = transform.Find("HaveNum").GetComponent<SkeletonGraphic>().AnimationState;
        if (AccountManager.Instance.cardPackage.data.ContainsKey(cardID)) {
            aniState.SetAnimation(0, AccountManager.Instance.cardPackage.data[cardID].cardCount.ToString(), false);
            cardObject.Find("Disabled").gameObject.SetActive(false);
            if(isHuman)
                transform.Find("NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.checkHumanCard.Contains(cardID));
            else
                transform.Find("NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.checkOrcCard.Contains(cardID));
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
    }


    public void DrawCard(string id) {
        cardID = id;
        cardData = AccountManager.Instance.allCardsDic[cardID];
        Transform cardObject;
        if (cardData.type == "unit") {
            transform.Find("MagicEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("UnitEditCard");
        }
        else if (cardData.type == "magic" && !cardData.isHeroCard) {
            transform.Find("UnitEditCard").gameObject.SetActive(false);
            cardObject = transform.Find("MagicEditCard");
        }
        else
            cardObject = transform;
        cardObject.gameObject.SetActive(true);
        if (cardData.rarelity == "legend")
            cardObject.SetAsFirstSibling();
        Sprite portraitImage = null;
        if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardID)) portraitImage = AccountManager.Instance.resource.cardPortraite[cardID];
        else portraitImage = AccountManager.Instance.resource.cardPortraite["default"];
        cardObject.Find("Portrait").GetComponent<Image>().sprite = portraitImage;
        if (!cardData.isHeroCard) {
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
            cardObject.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["name_" + cardData.rarelity];
        }
        else {
            string race;
            if (isHuman)
                race = "_human";
            else
                race = "_orc";
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race];
            cardObject.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race + "_name"];
        }

        if (cardData.type == "unit") {
            Logger.Log(cardData.name);
            cardObject.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.hp.ToString();
            cardObject.Find("attack/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.attack.ToString();
            if (cardData.attributes.Length == 0 && cardData.attackTypes.Length == 0)
                cardObject.Find("SkillIcon").gameObject.SetActive(false);
            else {
                cardObject.Find("SkillIcon").gameObject.SetActive(true);
                if (cardData.attributes.Length != 0)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attributes[0]];
                if (cardData.attackTypes.Length != 0)
                    if (AccountManager.Instance.resource.skillIcons.ContainsKey(cardData.attackTypes[0])) {
                        cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attackTypes[0]];
                    }
                if (cardData.attributes.Length != 0 && cardData.attackTypes.Length != 0)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons["complex"];
            }
        }
        cardObject.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.cost.ToString();
        //cardObject.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.cardClasses[0]];
        cardObject.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
        if (!cardData.isHeroCard)
            transform.Find("HaveNum").gameObject.SetActive(false);
        cardObject.Find("Disabled").gameObject.SetActive(false);
    }

    public void OpenCardInfo() {
        menuCardInfo.transform.parent.gameObject.SetActive(true);
        menuCardInfo.gameObject.SetActive(true);
        if(gameObject.name == "DictionaryCard" && transform.Find("NewCard").gameObject.activeSelf) {
            if (isHuman) {
                AccountManager.Instance.cardPackage.checkHumanCard.Remove(cardID);
                AccountManager.Instance.cardPackage.rarelityHumanCardCheck[AccountManager.Instance.cardPackage.data[cardID].rarelity].Remove(cardID);
            }
            else {
                AccountManager.Instance.cardPackage.checkOrcCard.Remove(cardID);
                AccountManager.Instance.cardPackage.rarelityOrcCardCheck[AccountManager.Instance.cardPackage.data[cardID].rarelity].Remove(cardID);
            }
            transform.Find("NewCard").gameObject.SetActive(false);
        }
        if (transform.parent.name == "Grid")
            menuCardInfo.SetCardInfo(cardData, isHuman, transform);
        else {
            menuCardInfo.SetCardInfo(cardData, isHuman);
            menuCardInfo.transform.Find("CreateCard").gameObject.SetActive(false);
        }
        if (transform.parent.parent.parent.name == "HeroInfo" && transform.parent.parent.name != "SkillWindow")
            exitTrigger2.SetActive(true);
    }
}

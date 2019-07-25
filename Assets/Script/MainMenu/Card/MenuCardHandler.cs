using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine;
using Spine.Unity;

public class MenuCardHandler : MonoBehaviour {
    private string cardID;
    [SerializeField] MenuCardInfo menuCardInfo;

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
        if (cardData.isHeroCard) return;
        if (AccountManager.Instance.cardPackage.data.ContainsKey(cardID)) {
            transform.Find("HaveNum").gameObject.SetActive(true);
            transform.Find("HaveNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + AccountManager.Instance.cardPackage.data[id].cardCount.ToString();
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
            transform.Find("HaveNum").gameObject.SetActive(false);
        }
    }

    public void OpenCardInfo() {
        menuCardInfo.SetCardInfo(cardData, isHuman);
        menuCardInfo.transform.parent.gameObject.SetActive(true);
    }
}

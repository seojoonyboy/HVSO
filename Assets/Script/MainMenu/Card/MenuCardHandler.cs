using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine;
using Spine.Unity;

public class MenuCardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private string cardID;
    [SerializeField] MenuCardInfo menuCardInfo;

    CardDataPackage cardDataPackage;
    CardData cardData;
    GameObject skeleton;
    bool isHuman;
    public string CARDID {
        get { return cardID; }
        set { cardID = value; }
    }

    public void DrawCard(string id, bool isHuman) {
        this.isHuman = isHuman;
        cardID = id;
        cardDataPackage = AccountManager.Instance.cardPackage;
        if (cardDataPackage.data.ContainsKey(cardID)) {
            cardData = cardDataPackage.data[cardID];
            if (cardData.rarelity == "legend")
                transform.SetAsFirstSibling();
            Sprite portraitImage = null;
            if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardID)) portraitImage = AccountManager.Instance.resource.cardPortraite[cardID];
            else portraitImage = AccountManager.Instance.resource.cardPortraite["default"];

            transform.Find("Portrait").GetComponent<Image>().sprite = portraitImage;
            if (!cardData.hero_chk) {
                transform.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
                transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["name_" + cardData.rarelity];
            }
            else {
                string race;
                if (isHuman)
                    race = "_human";
                else
                    race = "_orc";
                transform.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race];
                transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race + "_name"];
            }

            if (AccountManager.Instance.resource.cardSkeleton.ContainsKey("cardID")) skeleton = AccountManager.Instance.resource.cardSkeleton[cardID];
        }
        else
            Logger.Log("NoData");
        if (cardData.type == "unit") {
            Logger.Log(cardData.name);
            transform.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.hp.ToString();
            transform.Find("attack/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.attack.ToString();
            if (cardData.attributes.Length == 0 && cardData.attackTypes.Length == 0)
                transform.Find("SkillIcon").gameObject.SetActive(false);
            else {
                transform.Find("SkillIcon").gameObject.SetActive(true);
                if (cardData.attributes.Length != 0)
                    transform.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attributes[0]];
                if (cardData.attackTypes.Length != 0)
                    transform.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attackTypes[0]];
                if (cardData.attributes.Length != 0 && cardData.attackTypes.Length != 0)
                    transform.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons["complex"];
            }
        }
        else {
            transform.Find("Health").gameObject.SetActive(false);
            transform.Find("attack").gameObject.SetActive(false);
            transform.Find("SkillIcon").gameObject.SetActive(false);
        }
        transform.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.cost.ToString();
        transform.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.class_1];
        transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
    }

    public void OpenCardInfo() {
        menuCardInfo.SetCardInfo(cardData, isHuman);
        menuCardInfo.transform.parent.gameObject.SetActive(true);
    }


    float clickTime;

    public void OnBeginDrag(PointerEventData eventData) {
        clickTime = 0;
    }

    public void OnDrag(PointerEventData eventData) {
        clickTime += Time.deltaTime;
        if (clickTime > 1.0f) {
            OnEndDrag(null);
            return;
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        clickTime = 0;
        gameObject.SetActive(false);
    }
}

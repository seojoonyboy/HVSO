using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class EditCardHandler : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public string cardID;
    public DeckEditController deckEditController;

    [SerializeField] MenuCardInfo menuCardInfo;
    dataModules.CollectionCard cardData;
    public GameObject beforeObject;
    public Transform cardObject;

    public bool clicking = false;
    public bool pointerEnter = false;
    public float time = 0f;
    bool isHuman;
    public int myIndex;
    private int setNum;
    private int haveNum;

    public int SETNUM {
        get { return setNum; }
        set { setNum = value; }
    }

    public int HAVENUM {
        get { return haveNum; }
        set { haveNum = value; }
    }

    private void Update() {
        if (clicking == true && pointerEnter == true) {
            time += Time.deltaTime;

            if (time > 1f) {
                clicking = false;
                OpenCardInfo();
                time = 0;
            }
        }
    }

    public void SetHaveNum() {
        cardObject.Find("Disabled").gameObject.SetActive(false);
        transform.Find("HaveNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + haveNum.ToString();
    }

    public void SetSetNum() {
        if(setNum > 0)
            transform.Find("HaveNum").gameObject.SetActive(true);
        else
            transform.Find("HaveNum").gameObject.SetActive(false);
        transform.Find("HaveNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + setNum.ToString();
    }

    public void CardSet() {
        deckEditController.OnTouchCard(gameObject);
    }

    public void ExepctBtn() {
        deckEditController.ExpectFromDeck();
    }

    public void OnPointerDown(PointerEventData eventData) {
        clicking = true;
    }


    public void OnPointerClick(PointerEventData eventData) {
        if (clicking == false) return;

        clicking = false;
        CardSet();
        time = 0;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        pointerEnter = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        pointerEnter = false;
        time = 0;
    }

    public void OpenCardInfo() {
        menuCardInfo.SetCardInfo(cardData, isHuman);
        menuCardInfo.transform.parent.gameObject.SetActive(true);
    }


    public void DrawCard(string id, bool isHuman) {
        this.isHuman = isHuman;
        cardID = id;
        cardData = AccountManager.Instance.allCardsDic[cardID];
        if (cardData.type == "unit") 
            cardObject = transform.Find("UnitEditCard");
        else if (cardData.type == "magic" && !cardData.isHeroCard)
            cardObject = transform.Find("MagicEditCard");
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
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attackTypes[0]];
                if (cardData.attributes.Length != 0 && cardData.attackTypes.Length != 0)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons["complex"];
            }
        }
        cardObject.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.cost.ToString();
        //cardObject.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.cardClasses[0]];
        cardObject.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
        if (!cardData.isHeroCard) {
            if (haveNum > 0 || setNum > 0) {
                transform.Find("HaveNum").gameObject.SetActive(true);
                cardObject.Find("Disabled").gameObject.SetActive(false);
                if (haveNum > 0)
                    transform.Find("HaveNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + haveNum.ToString();
                if (setNum > 0)
                    transform.Find("HaveNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + setNum.ToString();
            }
            else {
                DisableCard(true);
                transform.Find("HaveNum").gameObject.SetActive(false);
            }
        }
    }

    public void DisableCard(bool disable) {
        cardObject.Find("Disabled").gameObject.SetActive(disable);
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
    }
}

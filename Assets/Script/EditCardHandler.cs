using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EditCardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
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
    private bool isHandCard = false;
    private bool disabled = false;
    private Vector3 startPos;
    private Vector3 startLocalPos;
    private Transform mouseObject;

    public static bool dragable = true;

    public int SETNUM {
        get { return setNum; }
        set { setNum = value; }
    }

    public int HAVENUM {
        get { return haveNum; }
        set { haveNum = value; }
    }

    private void Start() {
        if (transform.parent.name == "SettedDeck")
            isHandCard = true;
    }

    public void InitEditCard() {
        gameObject.SetActive(false);
        disabled = false;
        transform.Find("UnitEditCard").gameObject.SetActive(false);
        transform.Find("MagicEditCard").gameObject.SetActive(false);
        GetComponent<EditCardHandler>().SETNUM = 0;
    }

    public void OnBeginDrag(PointerEventData eventData) {        
        if (Input.touchCount > 1) return;
        if (disabled) return;
        if (!dragable) return;
        dragable = false;
        startPos = transform.position;
        startLocalPos = transform.localPosition;
        Vector3 mousePos = Input.mousePosition;
        if (isHandCard) {
            mouseObject = transform.parent.parent.Find("MousePos");
            mouseObject.position = new Vector3(mousePos.x, mouseObject.position.y, 0);
            transform.parent.SetParent(mouseObject);
            if (mousePos.y > startPos.y + 20)
                transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            else
                transform.position = new Vector3(mousePos.x, startPos.y, 0);
        }
        else
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);

    }

    public void OnDrag(PointerEventData eventData) {
        if (Input.touchCount > 1) return;
        if (disabled) return;
        Vector3 mousePos = Input.mousePosition;
        if (isHandCard) {
            mouseObject.position = new Vector3(mousePos.x, mouseObject.position.y, 0);
            if (mousePos.y > startPos.y + 20)
                transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            else
                transform.position = new Vector3(mousePos.x, startPos.y, 0);
        }
        else
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (disabled) return;
        StartCoroutine(EndDrag());
    }

    IEnumerator EndDrag() {
        if (isHandCard) {
            transform.parent.SetParent(mouseObject.parent);
            if (transform.localPosition.y > 280) {
                deckEditController.ExpectFromDeck(cardData.id, gameObject);
                if (SETNUM == 0) {
                    transform.SetAsLastSibling();
                    gameObject.SetActive(false);
                }
            }
            else {
                iTween.MoveTo(transform.parent.gameObject, iTween.Hash("x", 0, "islocal", true, "time", 0.2f));
                transform.localPosition = startLocalPos;
                yield return new WaitForSeconds(0.3f);
            }
        }
        else {
            if (transform.localPosition.y < 280) {
                deckEditController.ConfirmSetDeck(cardData.id, gameObject);
                transform.localPosition = startLocalPos;
            }
        }
        dragable = true;
    }

    public void SetHaveNum() {
        if (haveNum > 0) {
            DisableCard(false);
            transform.Find("HaveNum").gameObject.SetActive(true);
        }
        else {
            DisableCard(true);
            transform.Find("HaveNum").gameObject.SetActive(false);
        }
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
        disabled = disable;
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

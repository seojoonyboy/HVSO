using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class EditCardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public string cardID;
    public DeckEditController deckEditController;

    public MenuCardInfo menuCardInfo;
    public dataModules.CollectionCard cardData;
    public GameObject beforeObject;
    public Transform cardObject;
    public Transform beforeParent;

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

    static GameObject draggingObject;
    public string editBookRoot = "";
    public static bool onAnimation = false;
    public static bool dragable = true;
    public static bool dragging = false;

    public int SETNUM {
        get { return setNum; }
        set { setNum = value; }
    }

    public int HAVENUM {
        get { return haveNum; }
        set { haveNum = value; }
    }

    public void InitEditCard() {
        if (transform.parent.name == "SettedDeck")
            isHandCard = true;
        gameObject.SetActive(false);
        disabled = false;
        transform.Find("UnitEditCard").gameObject.SetActive(false);
        transform.Find("MagicEditCard").gameObject.SetActive(false);
        editBookRoot = "";
        GetComponent<EditCardHandler>().SETNUM = 0;
    }

    public void OnBeginDrag(PointerEventData eventData) {        
        if (Input.touchCount > 1) return;
        if (disabled) return;
        if (!dragable) return;
        draggingObject = gameObject;
        dragable = false;
        dragging = true;
        startPos = transform.position;
        startLocalPos = transform.localPosition;
        Vector3 mousePos = Input.mousePosition;
        if (isHandCard) {
            mouseObject = transform.parent.parent.Find("MousePos");
            mouseObject.position = new Vector3(mousePos.x, mouseObject.position.y, 0);
            transform.parent.SetParent(mouseObject);
            if (mousePos.y > startPos.y + 40)
                transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        }
        else {
            beforeParent = transform.parent;
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            transform.SetParent(deckEditController.setCardText.transform);
        }

    }

    public void OnDrag(PointerEventData eventData) {
        if (Input.touchCount > 1) return;
        if (disabled) return;
        if (draggingObject != gameObject) return;
        Vector3 mousePos = Input.mousePosition;
        if (isHandCard) {
            mouseObject.position = new Vector3(mousePos.x, mouseObject.position.y, 0);
            if (mousePos.y > startPos.y + 40)
                transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        }
        else
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (disabled) return;
        if (draggingObject != gameObject) return;
        EndDrag();
        dragging = false;
        draggingObject = null;
    }

    void EndDrag() {
        if (isHandCard) {
            transform.parent.SetParent(mouseObject.parent);
            transform.parent.localPosition = new Vector3(transform.parent.localPosition.x, -550, 0);
            if (transform.localPosition.y > 280) {
                deckEditController.ExpectFromDeck(cardData.id, gameObject);
                if (SETNUM == 0) {
                    transform.SetAsLastSibling();
                    gameObject.SetActive(false);
                }
            }
            else {
                transform.localPosition = startLocalPos;
            }
            SortHandPos();
        }
        else {
            transform.SetParent(beforeParent);
            if (transform.position.y < deckEditController.transform.Find("DeckNamePanel").position.y) {
                ShowAddedCardPos();
                deckEditController.ConfirmSetDeck(cardData.id, gameObject);
            }
            transform.localPosition = startLocalPos;
        }
        if(!onAnimation)
            dragable = true;
    }

    public void SortHandPos() {
        int handCardNum = deckEditController.setCardList.Count;
        if (handCardNum < 5)
            iTween.MoveTo(transform.parent.gameObject, iTween.Hash("x", 0, "islocal", true, "time", 0.2f));
        else {
            if (transform.parent.localPosition.x > 0)
                iTween.MoveTo(transform.parent.gameObject, iTween.Hash("x", 0, "islocal", true, "time", 0.2f));
            if(transform.parent.localPosition.x < -240 * (handCardNum - 4))
                iTween.MoveTo(transform.parent.gameObject, iTween.Hash("x", -240 * (handCardNum - 4), "islocal", true, "time", 0.2f));
        }
    }

    public void ShowAddedCardPos() {
        if (deckEditController.setCardList.ContainsKey(cardData.id)) {
            StartCoroutine(DuplicatedCardSet());
            return;
        }
        else {
            int handCardNum = deckEditController.setCardList.Count;
            if (handCardNum > 3) {
                iTween.MoveTo(deckEditController.settingLayout.gameObject, iTween.Hash("x", -240 * (handCardNum - 3), "islocal", true, "time", 0.2f));
            }
        }
    }

    IEnumerator DuplicatedCardSet() {
        onAnimation = true;
        Transform targetCard = deckEditController.setCardList[cardData.id].transform;
        mouseObject = targetCard.parent.parent.Find("MousePos");
        mouseObject.transform.position = targetCard.position;
        targetCard.parent.SetParent(mouseObject);
        if (targetCard.position.x != 0) {
            iTween.MoveTo(mouseObject.gameObject, iTween.Hash("x", 0, "islocal", true, "time", 0.2f));
            yield return new WaitForSeconds(0.25f);
        }
        targetCard.parent.SetParent(mouseObject.parent);
        onAnimation = false;
        dragable = true;
    }

    public void SetHaveNum() {
        if (haveNum > 0) {
            DisableCard(false);
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, haveNum.ToString(), false);
        }
        else {
            DisableCard(true);
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "NOANI", false);
        }
    }

    public void SetSetNum() {
        if (setNum > 0) {
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, setNum.ToString(), false);
        }
        else
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "NOANI", false);
    }

    public void CardSet() {
        deckEditController.OnTouchCard(gameObject);
    }


    public void OpenCardInfo() {
        if (dragging) return;
        menuCardInfo.SetCardInfo(cardData, isHuman);
        menuCardInfo.gameObject.SetActive(true);
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
            //cardObject.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["name_" + cardData.rarelity];
        }
        else {
            string race;
            if (isHuman)
                race = "_human";
            else
                race = "_orc";
            cardObject.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race];
            //cardObject.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race + "_name"];
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
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[cardData.attackTypes[0]];
                if (cardData.attributes.Length != 0 && cardData.attackTypes.Length != 0)
                    cardObject.Find("SkillIcon").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons["complex"];
            }
        }
        cardObject.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.cost.ToString();
        if (!cardData.isHeroCard) {
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().Initialize(false);
            Spine.AnimationState aniState = transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState;
            aniState.Update(0);
            if (haveNum > 0 || setNum > 0) {
                cardObject.Find("Disabled").gameObject.SetActive(false);
                if (haveNum > 0) {
                    //Debug.Log("!!" + transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState);
                    aniState.SetAnimation(0, haveNum.ToString(), false);
                }
                if (setNum > 0) {
                    //Debug.Log("!!" + transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState);
                    aniState.SetAnimation(0, setNum.ToString(), false);
                }
                    
            }
            else {
                DisableCard(true);
                transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "NOANI", false);
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

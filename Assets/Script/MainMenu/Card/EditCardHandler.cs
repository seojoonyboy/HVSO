using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class EditCardHandler : MonoBehaviour {
    public string cardID;
    public DeckEditController deckEditController;

    //public MenuCardInfo menuCardInfo;
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
    protected bool isHandCard = false;
    protected bool disabled = false;
    protected Vector3 startPos;
    protected Vector3 startLocalPos;
    protected Transform mouseObject;

    protected static GameObject draggingObject;
    public string editBookRoot = "";
    public static bool onAnimation = false;
    public static bool dragable = true;
    public static bool dragging = false;
    protected float mouseFirstXPos;
    protected float handFirstXPos;
    protected float clickTime;
    protected bool standby;

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
        GetComponent<EditCardHandler>().HAVENUM = 0;
    }


    //public void StartClick() {
    //    clickTime = Time.time;
    //    StartCoroutine(WaitForOpenInfo());
    //}

    //IEnumerator WaitForOpenInfo() {
    //    standby = true;
    //    while (standby) {
    //        yield return new WaitForSeconds(0.1f);
    //        if (dragging) break;
    //        if (onAnimation) break;
    //        if (!standby) break;
    //        if (Time.time - clickTime >= 0.2f) {
    //            dragable = false;
    //        }
    //        if (Time.time - clickTime >= 0.5f) {
    //            OpenCardInfo();
    //            standby = false;
    //            dragging = false;
    //            draggingObject = null;
    //        }
    //    }
    //    dragable = true;
    //}

    public void EndClick() {
        if (!dragable) return;
        if (dragging) return;
        if (onAnimation) return;
        dragging = false;
        draggingObject = null;
        standby = false;
        OpenCardInfo();
        //if (Time.time - clickTime < 0.5f) {
        //    SoundManager.Instance.PlaySound("button_3");
        //    if (isHandCard) {
        //        deckEditController.ExpectFromDeck(cardData.id, gameObject);
        //        if (SETNUM == 0) {
        //            transform.SetAsLastSibling();
        //            transform.localScale = Vector3.zero;
        //            StartCoroutine(SortHandPos());
        //        }
        //    }
        //    else {
        //        if (HAVENUM == 0) return;
        //        StartCoroutine(ShowAddedCardPos());
        //        deckEditController.ConfirmSetDeck(cardData.id, gameObject);
        //    }
        //}
        //else
        //    OpenCardInfo();
    }

    public IEnumerator SortHandPos() {
        onAnimation = true;
        int handCardNum = deckEditController.setCardList.Count;
        if (handCardNum < 5)
            transform.parent.localPosition = new Vector3(0, -550, 0);
        else {
            if (transform.parent.localPosition.x > 0)
                iTween.MoveTo(transform.parent.gameObject, iTween.Hash("x", 0, "y", -550, "islocal", true, "time", 0.2f));
            if (transform.parent.localPosition.x < -240 * (handCardNum - 4))
                iTween.MoveTo(transform.parent.gameObject, iTween.Hash("x", -240 * (handCardNum - 4), "y", -550, "islocal", true, "time", 0.2f));
        }
        yield return new WaitForSeconds(0.25f);
        onAnimation = false;
        dragable = true;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    public IEnumerator ShowAddedCardPos() {
        onAnimation = true;
        if (deckEditController.setCardList.ContainsKey(cardData.id)) {
            if (deckEditController.setCardList.Count > 4)
                StartCoroutine(DuplicatedCardSet());
            else {
                onAnimation = false;
                dragable = true;
            }
        }
        else {
            int handCardNum = deckEditController.setCardList.Count;
            if (handCardNum > 3) {
                iTween.MoveTo(deckEditController.settingLayout.gameObject, iTween.Hash("x", -240 * (handCardNum - 3), "y", -550, "islocal", true, "time", 0.2f));
                yield return new WaitForSeconds(0.25f);
            }
            onAnimation = false;
            dragable = true;
        }

    }

    IEnumerator DuplicatedCardSet() {
        onAnimation = true;
        Transform targetCard = deckEditController.setCardList[cardData.id].transform;
        Transform mask = targetCard.parent.parent;
        float leftEdge = mask.position.x + mask.GetComponent<RectTransform>().rect.xMin;
        float rightEdge = mask.position.x + mask.GetComponent<RectTransform>().rect.xMax;
        if (targetCard.Find("HaveNum").position.x < leftEdge || targetCard.Find("HaveNum").position.x > rightEdge) {
            mouseObject = targetCard.parent.parent.Find("MousePos");
            mouseObject.transform.position = targetCard.position;
            targetCard.parent.SetParent(mouseObject);
            if (targetCard.position.x != 0) {
                iTween.MoveTo(mouseObject.gameObject, iTween.Hash("x", 0, "islocal", true, "time", 0.2f));
                yield return new WaitForSeconds(0.25f);
            }
            targetCard.parent.SetParent(mouseObject.parent);
        }
        onAnimation = false;
        dragable = true;
    }

    public void SetHaveNum(bool except = false) {
        if (haveNum > 0) {
            DisableCard(false);
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, haveNum.ToString(), false);
            if (except) {
                Color spineColor = new Color();
                switch (cardData.rarelity) {
                    case "common":
                        spineColor = new Color(0.686f, 0.721f, 0.717f);
                        break;
                    case "uncommon":
                        spineColor = new Color(0.447f, 0.945f, 0.227f);
                        break;
                    case "rare":
                        spineColor = new Color(0.078f, 0.572f, 1f);
                        break;
                    case "superrare":
                        spineColor = new Color(0.772f, 0.062f, 0.788f);
                        break;
                    case "legend":
                        spineColor = new Color(1f, 0.494f, 0f);
                        break;
                }
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().color = spineColor;
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Initialize(true);
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Update(0);

                if (cardData.type == "unit")
                    transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
                else
                    transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Skeleton.SetSkin("2.magic");
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
            }
        }
        else {
            DisableCard(true);
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "NOANI", false);
        }
    }

    public void SetSetNum(bool add = false) {
        if (setNum > 0) {
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "bottom_" + setNum.ToString(), false);
            if (add) {
                Color spineColor = new Color();
                switch (cardData.rarelity) {
                    case "common":
                        spineColor = new Color(0.686f, 0.721f, 0.717f);
                        break;
                    case "uncommon":
                        spineColor = new Color(0.447f, 0.945f, 0.227f);
                        break;
                    case "rare":
                        spineColor = new Color(0.078f, 0.572f, 1f);
                        break;
                    case "superrare":
                        spineColor = new Color(0.772f, 0.062f, 0.788f);
                        break;
                    case "legend":
                        spineColor = new Color(1f, 0.494f, 0f);
                        break;
                }
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().color = spineColor;
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Initialize(true);
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Update(0);

                if (cardData.type == "unit")
                    transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.unit");
                else
                    transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().Skeleton.SetSkin("2.magic");
                transform.Find("CardAddEffect").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "animation", false);
            }
        }
        else
            transform.Find("HaveNum/Graphic").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "NOANI", false);
    }

    public void CardSet() {
        deckEditController.OnTouchCard(gameObject);
    }


    public void OpenCardInfo() {
        if (dragging) return;
        MenuCardInfo.cardInfoWindow.transform.parent.gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.transform.parent.Find("DeckEditExitTrigger").gameObject.SetActive(true);
        MenuCardInfo.cardInfoWindow.transform.parent.Find("ExitTrigger").gameObject.SetActive(false);
        MenuCardInfo.cardInfoWindow.SetCardInfo(cardData, isHuman, null);
        MenuCardInfo.cardInfoWindow.transform.Find("CreateCard").gameObject.SetActive(false);
        MenuCardInfo.cardInfoWindow.transform.Find("EditCardUI").gameObject.SetActive(true);
        if (isHandCard) {
            MenuCardInfo.cardInfoWindow.SetEditCardInfo(beforeObject.GetComponent<EditCardHandler>().HAVENUM, deckEditController.setCardNum);
            MenuCardInfo.cardInfoWindow.editCard = beforeObject;
        }
        else {
            MenuCardInfo.cardInfoWindow.SetEditCardInfo(haveNum, deckEditController.setCardNum);
            MenuCardInfo.cardInfoWindow.editCard = gameObject;
        }
        MenuCardInfo.cardInfoWindow.transform.Find("Flavor").gameObject.SetActive(false);
        MenuCardInfo.cardInfoWindow.transform.Find("Indestructible").gameObject.SetActive(false);
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
        //if (cardData.rarelity == "legend")
        //    cardObject.SetAsFirstSibling();
        Sprite portraitImage = null;
        if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardID)) portraitImage = AccountManager.Instance.resource.cardPortraite[cardID];
        else portraitImage = AccountManager.Instance.resource.cardPortraite["default"];
        cardObject.Find("Portrait").GetComponent<Image>().sprite = portraitImage;
        if (!cardData.isHeroCard) {
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
                    aniState.SetAnimation(0, haveNum.ToString(), false);
                }
                if (setNum > 0) {
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

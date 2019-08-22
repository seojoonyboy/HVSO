using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using System;
using SkillModules;
using Spine;
using Spine.Unity;

public partial class CardHandler : MonoBehaviour {
    public GameObject unit;
    public GameObject skeleton;
    protected CardListManager clm;
    protected bool blockButton = false;
    protected bool firstDraw = false;
    public bool changeSelected = false;
    protected bool isDropable = false;
    protected bool pointOnFeild = false;
    protected bool cardUsed = false;
    Animator cssAni;
    public string cardID;
    protected int _itemID;

    
    public Transform mouseLocalPos;
    public Transform mouseXPos;
    public Transform cardHand;
    public CardHandManager handManager;
    protected int myCardIndex;

    public int CARDINDEX {
        set { myCardIndex = value; }
    }
    public bool CARDUSED {
        set { cardUsed = value; }
    }

    public int itemID {
        get { return _itemID; }
        set {
            if (value < 0) Logger.Log("something wrong itemId");
            _itemID = value;
        }
    }

    protected bool highlighted = false;
    public Transform highlightedSlot;

    public CardData cardData;
    public CardDataPackage cardDataPackage;

    protected static GameObject itsDragging;

    public bool FIRSTDRAW {
        get { return firstDraw; }
        set { firstDraw = value; }
    }

    private void Start() {
        mouseXPos = transform.parent.parent.parent.Find("MouseXposition");
        cardHand = transform.root.Find("CardHand");
        handManager = cardHand.GetComponent<CardHandManager>();
        mouseLocalPos = transform.parent.parent.parent.Find("MouseLocalPosition");        
        clm = PlayMangement.instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
        gameObject.SetActive(false);
    }

    public virtual void DrawCard(string ID, int itemID = -1, bool first = false) {
        cardDataPackage = AccountManager.Instance.cardPackage;
        cardID = ID;

        //cardID = "ac10002";    //테스트 코드
        this.itemID = itemID;
        if (cardDataPackage.data.ContainsKey(cardID)) {
            cardData = cardDataPackage.data[cardID];
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
                if (PlayMangement.instance.player.isHuman)
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
            transform.Find("GlowEffect/HaveAbility").gameObject.SetActive(false);
            transform.Find("GlowEffect/NonAbility").gameObject.SetActive(false);
            transform.Find("Disabled/HaveAbility").gameObject.SetActive(false);
            transform.Find("Disabled/NonAbility").gameObject.SetActive(false);
        }
        else
            transform.Find("GlowEffect").GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1, 1);
        transform.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.cost.ToString();
        transform.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.class_1];
        transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;


        if (first) {
            if (gameObject.name == "UnitCard") {
                if (transform.Find("SkillIcon").gameObject.activeSelf) {
                    transform.Find("GlowEffect/HaveAbility").gameObject.SetActive(true);
                    transform.Find("GlowEffect/HaveAbility").GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1, 1);
                }
                else {
                    transform.Find("GlowEffect/NonAbility").gameObject.SetActive(true);
                    transform.Find("GlowEffect/NonAbility").GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1, 1);
                }
            }
            else {
                transform.Find("GlowEffect").gameObject.SetActive(true);
                transform.Find("GlowEffect").GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1, 1);
            }
            clm.AddMulliganCardInfo(cardData, cardID);
            firstDraw = true;
        }
        else {
            if (firstDraw) {
                transform.Find("ChangeButton").gameObject.SetActive(true);
                transform.Find("ChangeButton").GetChild(0).gameObject.SetActive(true);
            }
            else
                transform.Find("ChangeButton").gameObject.SetActive(false);
        }
        
    }

    public virtual void CheckHighlight() {
        if (!highlighted) {
            highlightedSlot = CheckSlot();
            if (highlightedSlot != null) {
                highlighted = true;
                CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            }
        }
        else {
            if (highlightedSlot != CheckSlot()) {
                highlighted = false;
                CardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
                highlightedSlot = null;
            }
        }
    }
    protected virtual void CheckLocation(bool off = false) {
        if (off) {
            pointOnFeild = false;
            if (cardData.type == "unit")
                CardInfoOnDrag.instance.ActivePreviewUnit(false);
            else
                CardInfoOnDrag.instance.ActiveCrossHair(false);
            return;
        }
        if (transform.position.y > -3.5) {
            if (!pointOnFeild) {
                pointOnFeild = true;
                transform.localScale = new Vector3(0, 0, 0);
                if (cardData.type == "unit")
                    CardInfoOnDrag.instance.ActivePreviewUnit(true);
                else {
                    if(cardData.skills[0].target.args[1] != "all")
                        CardInfoOnDrag.instance.ActiveCrossHair(true);
                    else
                        transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
        else {
            if (pointOnFeild) {
                pointOnFeild = false;
                transform.localScale = new Vector3(1, 1, 1);
                if (cardData.type == "unit")
                    CardInfoOnDrag.instance.ActivePreviewUnit(false);
                else {
                    CardInfoOnDrag.instance.ActiveCrossHair(false);
                }
            }
        }
    }

    protected Transform CheckSlot() {
        Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        origin = new Vector3(origin.x, origin.y + 0.3f, origin.z);
        Ray2D ray = new Ray2D(origin, Vector2.zero);

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.collider != null && hit.transform.gameObject.layer == 12) {
            return hit.transform;
        }

        return null;
    }

    protected Transform CheckMagicSlot() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LayerMask mask = (1 << LayerMask.NameToLayer("MagicTarget"));
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            new Vector2(mousePos.x, mousePos.y),
            Vector2.zero,
            Mathf.Infinity,
            mask
        );

        if (hits != null) {
            foreach (RaycastHit2D hit in hits) {
                return hit.transform;
            }
        }
        return null;
    }

    public void CheckMagicHighlight() {
        if (!highlighted) {
            highlightedSlot = CheckMagicSlot();
            if (highlightedSlot != null) {
                highlighted = true;
                CardInfoOnDrag.instance.crossHair.GetComponent<SkeletonGraphic>().Skeleton.SetSkin("3.green");
                if (cardData.skills[0].target.args[1] == "all")
                    transform.Find("GlowEffect").GetComponent<SkeletonGraphic>().color = new Color(0.639f, 0.925f, 0.105f);
                CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
            }
        }
        else {
            if (highlightedSlot != CheckMagicSlot()) {
                highlighted = false;
                CardInfoOnDrag.instance.crossHair.GetComponent<SkeletonGraphic>().Skeleton.SetSkin("1.yellow");
                if (cardData.skills[0].target.args[1] == "all")
                    transform.Find("GlowEffect").GetComponent<SkeletonGraphic>().color = new Color(1, 1, 1, 1);
                CardDropManager.Instance.HighLightMagicSlot(highlightedSlot, highlighted);
                highlightedSlot = null;
            }
        }
    }

    /// <summary>
    /// 마법 카드를 위한 현재 드래그 위치의 라인 검사
    /// </summary>
    /// <returns></returns>
    protected Transform CheckLine() {
        Vector3 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        origin = new Vector3(origin.x, origin.y, origin.z);
        Ray2D ray = new Ray2D(origin, Vector2.zero);

        LayerMask layerMask = 1 << LayerMask.NameToLayer("Line");
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, layerMask);

        if (hit.collider != null && hit.transform.gameObject.layer == 15) {
            Logger.Log(hit.collider.transform.parent.name);
            return hit.transform;
        }
        return null;
    }

    public void OpenCardInfoList() {
        if (heroCardActivate) return;
        if (PlayMangement.movingCard != null) return;
        if (PlayMangement.instance.isMulligan && transform.parent.name == "FirstDrawParent") {
            clm.OpenMulliganCardList(transform.GetSiblingIndex() - 5);
            return;
        }
        if (!blockButton) {
            clm.OpenCardInfo(transform.parent.GetSiblingIndex());
        }
    }

    public void RedrawButton() {
        CardHandManager handManager = FindObjectOfType<CardHandManager>();
        PlayMangement.instance.socketHandler.HandchangeCallback = handManager.RedrawCallback;
        PlayMangement.instance.socketHandler.ChangeCard(itemID);
    }

    public void DisableCard() {
        isDropable = false;
        if (gameObject.name == "UnitCard") {
            if (transform.Find("SkillIcon").gameObject.activeSelf) {
                transform.Find("GlowEffect/HaveAbility").gameObject.SetActive(false);
                transform.Find("Disabled/HaveAbility").gameObject.SetActive(true);
            }
            else {
                transform.Find("GlowEffect/NonAbility").gameObject.SetActive(false);
                transform.Find("Disabled/NonAbility").gameObject.SetActive(true);
            }
            if (!PlayMangement.instance.player.isHuman && PlayMangement.instance.currentTurn == "SECRET")
                transform.Find("Disabled/Orc").gameObject.SetActive(true);
            else
                transform.Find("Disabled/Orc").gameObject.SetActive(false);
        }
        else {
            transform.Find("GlowEffect").gameObject.SetActive(false);
            transform.Find("Disabled").gameObject.SetActive(true);
            if (!PlayMangement.instance.player.isHuman && PlayMangement.instance.currentTurn == "ORC")
                transform.Find("Disabled/Orc").gameObject.SetActive(true);
            else
                transform.Find("Disabled/Orc").gameObject.SetActive(false);
        }
    }

    public virtual void ActivateCard() {
        if (PlayMangement.instance.player.resource.Value - cardData.cost >= 0) {
            isDropable = true;
            if (cardData.cost <= PlayerController.activeCardMinCost)
                PlayerController.activeCardMinCost = cardData.cost;
            if (gameObject.name == "UnitCard") {
                if (transform.Find("SkillIcon").gameObject.activeSelf) {
                    transform.Find("GlowEffect/HaveAbility").gameObject.SetActive(true);
                    transform.Find("Disabled/HaveAbility").gameObject.SetActive(false);
                }
                else {
                    transform.Find("GlowEffect/NonAbility").gameObject.SetActive(true);
                    transform.Find("Disabled/NonAbility").gameObject.SetActive(false);
                }
            }
            else {
                transform.Find("GlowEffect").gameObject.SetActive(true);
                transform.Find("Disabled").gameObject.SetActive(false);
            }
        }
        else {
            DisableCard();
        }
    }

    public bool IsEnoughResource(int cost) {
        return PlayMangement.instance.player.resource.Value >= cost;
    }

    public void UserResource(int cost) {
        PlayMangement.instance.player.resource.Value -= cost;
    }

    protected bool isMyTurn(bool isMagic) {
        if(PlayMangement.instance.heroShieldActive) return false;
        bool isHuman = PlayMangement.instance.player.isHuman;
        string currentTurn = PlayMangement.instance.GetComponent<TurnMachine>().CurrentTurn();
        bool isHumanTurn = currentTurn.CompareTo("HUMAN") == 0;
        bool isOrcPreTurn = currentTurn.CompareTo("ORC") == 0;
        bool isOrcMagicTurn = currentTurn.CompareTo("SECRET") == 0;
        return isHuman ? isHumanTurn : isMagic ? isOrcMagicTurn : isOrcPreTurn;
    }

    protected void StartDragCard() {
        transform.localScale = new Vector3(1.15f, 1.15f, 1);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        mouseXPos.position = new Vector3(mousePos.x, 0, 0);
        cardHand.transform.SetParent(mouseXPos);
        mouseLocalPos.position = transform.position;
        EffectSystem.Instance.DecreaseFadeAlpha();
    }

    protected void OnDragCard() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        mouseXPos.position = new Vector3(mousePos.x, 0, 0);
        if (mousePos.y > -6.5f)
            transform.position = mousePos;
        else
            transform.localPosition = new Vector3(0, 0, 0);
        mouseLocalPos.position = transform.position;
    }
}

//영웅 카드
public partial class CardHandler : MonoBehaviour {

    protected GameObject heroCardInfo;
    public bool heroCardActivate = false;
    public void DrawHeroCard(SocketFormat.Card data) {
        DrawCard(data.id, data.itemId);
        heroCardInfo = clm.AddHeroCardInfo(cardData);
        gameObject.GetComponent<MagicDragHandler>().skillHandler = new SkillHandler();
        gameObject.GetComponent<MagicDragHandler>().skillHandler.Initialize(data.skills, gameObject, true);
        heroCardInfo.transform.SetParent(transform);
        heroCardInfo.SetActive(true);
        heroCardInfo.transform.rotation = transform.rotation;
        heroCardInfo.transform.position = transform.position;
        heroCardActivate = true;
    }

    public IEnumerator ActiveHeroCard() {
        while (heroCardActivate)
            yield return new WaitForSeconds(0.1f);
    }
}
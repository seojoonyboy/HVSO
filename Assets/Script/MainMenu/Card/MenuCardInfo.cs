using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BestHTTP;
using dataModules;
using Spine;
using Spine.Unity;
using System;

public partial class MenuCardInfo : MonoBehaviour {
    Translator translator;
    [SerializeField] Transform classDescModal;
    [SerializeField] DeckSettingManager deckSettingManager;

    string cardId;
    bool isHuman;
    bool cardCreate = false;
    float beforeCrystal;
    AccountManager accountManager;
    CollectionCard cardData;
    Transform cardObject;

    public UnityEvent OnMakeCardFinished = new UnityEvent();

    public static MenuCardInfo cardInfoWindow;
    public GameObject editCard;
    bool makeCard;


    private void Start() {
        accountManager = AccountManager.Instance;
        transform.Find("CreateSpine").GetComponent<SkeletonGraphic>().Initialize(false);
        transform.Find("CreateSpine").GetComponent<SkeletonGraphic>().Update(0);
        transform.Find("CreateSpine").GetComponent<SkeletonGraphic>().AnimationState.Complete += delegate { EndCardMaking(); };

        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_CREATE_CARD, CardModified);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_REMOVE_CARD, CardModified);

        cardInfoWindow = this;
        gameObject.SetActive(false);
        transform.parent.gameObject.SetActive(false);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_CREATE_CARD, CardModified);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_REMOVE_CARD, CardModified);
    }

    private void CardModified(Enum Event_Type, Component Sender, object Param) {
        accountManager.RequestUserInfo();
        accountManager.RequestInventories();
        accountManager.RequestMyDecks();
    }

    public virtual void SetCardInfo(CollectionCard data, bool isHuman, Transform cardObj, bool makeCard = false) {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON4);
        cardObject = cardObj;
        cardId = data.id;
        this.isHuman = isHuman;
        Transform info = transform;
        cardData = data;
        translator = AccountManager.Instance.GetComponent<Translator>();
        info.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["name_" + data.rarelity];
        info.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;

        if (data.skills.Length != 0) {
            info.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translator.DialogSetRichText(data.skills[0].desc);
        }
        else
            info.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = null;

        if (data.hp != null) {
            info.Find("Health/Text").GetComponent<Text>().text = data.hp.ToString();
            info.Find("Health").gameObject.SetActive(true);
        }
        else
            info.Find("Health").gameObject.SetActive(false);

        if (data.attack != null) {
            info.Find("Attack/Text").GetComponent<Text>().text = data.attack.ToString();
            info.Find("Attack").gameObject.SetActive(true);
        }
        else
            info.Find("Attack").gameObject.SetActive(false);

        info.Find("Cost/Text").GetComponent<Text>().text = data.cost.ToString();

        info.Find("Class_1").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[data.cardClasses[0]];


        for (int i = 0; i < 3; i++) {
            info.Find("Skill&BuffRow1").GetChild(i).gameObject.SetActive(false);
            EventTrigger skill1 = info.Find("Skill&BuffRow1").GetChild(i).GetComponent<EventTrigger>();
            skill1.triggers.RemoveRange(0, skill1.triggers.Count);

            info.Find("Skill&BuffRow2").GetChild(i).gameObject.SetActive(false);
            EventTrigger skill2 = info.Find("Skill&BuffRow2").GetChild(i).GetComponent<EventTrigger>();
            skill2.triggers.RemoveRange(0, skill2.triggers.Count);
        }

        info.Find("Flavor/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "";
        info.Find("UnitPortrait").gameObject.SetActive(false);
        info.Find("MagicPortrait").gameObject.SetActive(false);
        int skillnum = 0;
        if (data.type == "unit") {
            info.Find("UnitPortrait").gameObject.SetActive(true);
            if (AccountManager.Instance.resource.infoPortraite.ContainsKey(data.id)) {
                info.Find("UnitPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoPortraite[data.id];
            }
            if (data.attackTypes.Length != 0) {
                info.Find("Skill&BuffRow1").GetChild(skillnum).gameObject.SetActive(true);
                var image = AccountManager.Instance.resource.skillIcons[data.attackTypes[0]];
                info.Find("Skill&BuffRow1").GetChild(skillnum).Find("SkillIcon").GetComponent<Image>().sprite = image;
                info.Find("Skill&BuffRow1").GetChild(skillnum).Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text =translator.GetTranslatedSkillName(data.attackTypes[0]);
                EventTrigger.Entry onBtn = new EventTrigger.Entry();
                onBtn.eventID = EventTriggerType.PointerDown;
                onBtn.callback.AddListener((EventData) => OpenClassDescModal(data.attackTypes[0], image));
                info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(onBtn);
                EventTrigger.Entry offBtn = new EventTrigger.Entry();
                offBtn.eventID = EventTriggerType.PointerUp;
                offBtn.callback.AddListener((EventData) => CloseClassDescModal());
                info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(offBtn);
                skillnum++;
            }
            if (data.attributes.Length != 0) {
                info.Find("Skill&BuffRow1").GetChild(skillnum).gameObject.SetActive(true);
                var image = AccountManager.Instance.resource.skillIcons[data.attributes[0]];
                info.Find("Skill&BuffRow1").GetChild(skillnum).Find("SkillIcon").GetComponent<Image>().sprite = image;
                info.Find("Skill&BuffRow1").GetChild(skillnum).Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text= translator.GetTranslatedSkillName(data.attributes[0]);
                EventTrigger.Entry onBtn = new EventTrigger.Entry();
                onBtn.eventID = EventTriggerType.PointerDown;
                onBtn.callback.AddListener((EventData) => OpenClassDescModal(data.attributes[0], image));
                info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(onBtn);
                EventTrigger.Entry offBtn = new EventTrigger.Entry();
                offBtn.eventID = EventTriggerType.PointerUp;
                offBtn.callback.AddListener((EventData) => CloseClassDescModal());
                info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(offBtn);
                skillnum++;
            }

            List<string> categories = new List<string>();
            if (data.cardCategories[0] != null) categories.Add(data.cardCategories[0]);
            if (data.cardCategories.Length > 1 && data.cardCategories[1] != null) categories.Add(data.cardCategories[1]);
            var translatedCategories = translator.GetTranslatedUnitCtg(categories);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int cnt = 0;
            foreach (string ctg in translatedCategories) {
                cnt++;
                if (translatedCategories.Count != cnt) sb.Append(ctg + ", ");
                else sb.Append(ctg);
            }
            info.Find("Categories").gameObject.SetActive(true);
            info.Find("Categories/Text").GetComponent<TMPro.TextMeshProUGUI>().text = sb.ToString();
            info.Find("Flavor/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.flavorText;
            info.Find("Flavor/Text").position = info.Find("Skill&BuffRow2").position;
            info.Find("Flavor").gameObject.SetActive(true);
        }
        //마법 카드
        else {
            List<string> categories = new List<string>();
            categories.Add("magic");
            var translatedCategories = translator.GetTranslatedUnitCtg(categories);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int cnt = 0;
            foreach (string ctg in translatedCategories) {
                cnt++;
                if (translatedCategories.Count != cnt) sb.Append(ctg + ", ");
                else sb.Append(ctg);
            }
            info.Find("Categories/Text").GetComponent<TMPro.TextMeshProUGUI>().text = sb.ToString();

            info.Find("MagicPortrait").gameObject.SetActive(true);
            //info.Find("Categories").gameObject.SetActive(false);
            if (AccountManager.Instance.resource.cardPortraite.ContainsKey(data.id)) {
                info.Find("MagicPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[data.id];
                if (data.isHeroCard)
                    info.Find("MagicPortrait/Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_legend_human"];
                else
                    info.Find("MagicPortrait/Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["magic_" + data.rarelity];
            }
        }

        if (data.isHeroCard) {
            info.Find("CreateCard").gameObject.SetActive(false);
            info.Find("CreateSpine").gameObject.SetActive(false);
            transform.Find("Indestructible").gameObject.SetActive(false);
        }
        else {
            if (data.indestructible) {
                transform.Find("Indestructible").gameObject.SetActive(true);
                transform.Find("Indestructible/HaveNum").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "4", false);
                info.Find("CreateCard").gameObject.SetActive(false);
            }
            else {
                transform.Find("Indestructible").gameObject.SetActive(false);
                info.Find("CreateCard").gameObject.SetActive(true);
                int cardNum = 0;
                if (AccountManager.Instance.cardPackage.data.ContainsKey(data.id))
                    cardNum = AccountManager.Instance.cardPackage.data[data.id].cardCount;
                info.Find("CreateCard/HaveNum").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, cardNum.ToString(), false);
                int makeCardcost = 0;
                int breakCardcost = 0;
                switch (data.rarelity) {
                    case "common":
                        makeCardcost = 50;
                        breakCardcost = 5;
                        break;
                    case "uncommon":
                        makeCardcost = 150;
                        breakCardcost = 20;
                        break;
                    case "rare":
                        makeCardcost = 500;
                        breakCardcost = 80;
                        break;
                    case "superrare":
                        makeCardcost = 1000;
                        breakCardcost = 185;
                        break;
                    case "legend":
                        makeCardcost = 2000;
                        breakCardcost = 400;
                        break;
                }
                if (cardNum == 4)
                    info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
                else {
                    info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(false);
                    if (makeCardcost >= AccountManager.Instance.userResource.crystal)
                        info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
                }
                if (cardNum == 0)
                    info.Find("CreateCard/BreakBtn/Disabled").gameObject.SetActive(true);
                else
                    info.Find("CreateCard/BreakBtn/Disabled").gameObject.SetActive(false);
                info.Find("CreateCard/CrystalUseValue").GetComponent<TMPro.TextMeshProUGUI>().text = "-" + makeCardcost.ToString();
                info.Find("CreateCard/CrystalGetValue").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + breakCardcost.ToString();
                if (makeCard) {
                    if (data.rarelity == "common" && accountManager.userResource.crystal > beforeCrystal)
                        info.Find("CreateCard/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.manaCrystal.ToString();
                    else
                        StartCoroutine(AddCrystalAnimation());
                }
                else
                    info.Find("CreateCard/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.manaCrystal.ToString();
            }
        }
    }

    IEnumerator AddCrystalAnimation() {
        float changed = (float)accountManager.userResource.crystal;
        TMPro.TextMeshProUGUI crystalText = transform.Find("CreateCard/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>();
        float duration = 0.3f;
        float offset = (changed - beforeCrystal) / duration;
        if (changed > beforeCrystal) {
            while (beforeCrystal < changed) {
                beforeCrystal += offset * Time.deltaTime;
                crystalText.text = ((int)beforeCrystal).ToString();
                yield return null;
            }
        }
        else {
            while (beforeCrystal > changed) {
                beforeCrystal += offset * Time.deltaTime;
                crystalText.text = ((int)beforeCrystal).ToString();
                yield return null;
            }
        }
        crystalText.text = ((int)changed).ToString();
    }

    public void OpenClassDescModal(string className, Sprite image) {
        if (Input.touchCount > 1) return;
        Vector3 mousePos = Input.mousePosition;
        classDescModal.gameObject.SetActive(true);
        classDescModal.position = new Vector3(mousePos.x + 20, mousePos.y + 300f, 0);
        string[] set = translator.GetTranslatedSkillSet(className);
        SetClassDescModalData(set[0], set[1], image);
    }

    public void CloseClassDescModal() {
        SetClassDescModalData();
        classDescModal.gameObject.SetActive(false);
    }

    private void SetClassDescModalData(string className = "", string desc = "", Sprite sprite = null) {
        TMPro.TextMeshProUGUI TMP_header = classDescModal.Find("InnerModal/Header").GetComponent<TMPro.TextMeshProUGUI>();
        TMP_header.text = "<color=blue>"+className+"</color>" + "\n" + desc;
    }

    public void CloseInfo() {
        if (cardCreate) return;
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.parent.gameObject.SetActive(false);
        transform.gameObject.SetActive(false);
        //transform.parent.Find("DeckEditExitTrigger").gameObject.SetActive(false);
        //transform.parent.Find("ExitTrigger").gameObject.SetActive(true);
        transform.parent.Find("HeroInfo").gameObject.SetActive(false);
        transform.Find("CreateCard/BreakBtn/DisableInHand").gameObject.SetActive(false);
        editCard = null;
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseInfo);
    }

    public void CloseHeroesCardInfo() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.gameObject.SetActive(false);
        transform.parent.Find("ExitTrigger2").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseHeroesCardInfo);
    }


    public void MakeCard() {
        if (cardCreate) return;
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        cardCreate = true;
        makeCard = true;
        transform.Find("CreateSpine").gameObject.SetActive(true);
        transform.Find("CreateSpine").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "MAKING_" + cardData.rarelity, false);
        beforeCrystal = accountManager.userResource.crystal;
        accountManager.RequestCardMake(cardId);
    }

    public void BreakCard() {
        if (cardCreate) return;
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        cardCreate = true;
        makeCard = false;
        transform.Find("CreateSpine").gameObject.SetActive(true);
        transform.Find("CreateSpine").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DECOMPOSITION_" + cardData.rarelity, false);
        beforeCrystal = accountManager.userResource.crystal;

        accountManager.RequestCardBreak(cardId);

        string rarelity = accountManager.cardPackage.data[cardId].rarelity;
        if (accountManager.cardPackage.data[cardId].camp == "human")
            accountManager.cardPackage.rarelityHumanCardNum[rarelity].Remove(cardId);
        else
            accountManager.cardPackage.rarelityOrcCardNum[rarelity].Remove(cardId);
        accountManager.cardPackage.data.Remove(cardId);
    }

    void EndCardMaking() {
        if (cardObject.name == "DictionaryCard") {
            cardObject.GetComponent<MenuCardHandler>().DrawCard(cardId, isHuman);
            CardDictionaryManager.cardDictionaryManager.transform.Find("UIbar/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text
            = AccountManager.Instance.userResource.crystal.ToString();
            SetCardInfo(cardData, isHuman, cardObject, true);
        }
        else if(cardObject.name == "DictionaryCardVertical") {
            cardObject.GetComponent<MenuCardHandler>().DrawCard(cardId);
            SetCardInfo(cardData, isHuman, cardObject, true);
        }
        else {
            cardObject.GetComponent<EditCardHandler>().deckEditController.cardButtons.MakeCard(cardObject, makeCard);
            SetCardInfo(cardData, isHuman, cardObject, true);
            transform.Find("Flavor").gameObject.SetActive(false);
        }
        cardCreate = false;
        deckSettingManager.SetPlayerNewDecks();
    }
}

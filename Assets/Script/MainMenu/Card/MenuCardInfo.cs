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
using System.Text;

public partial class MenuCardInfo : MonoBehaviour {
    Fbl_Translator translator;
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
    public int bookHaveNum;
    public int haveNum;
    static public bool onTuto = false;

    private void Start() {
        accountManager = AccountManager.Instance;
        transform.Find("CreateCard/CreateSpine").GetComponent<SkeletonGraphic>().Initialize(true);
        transform.Find("CreateCard/CreateSpine").GetComponent<SkeletonGraphic>().Update(0);
        transform.Find("CreateCard/CreateSpine").GetComponent<SkeletonGraphic>().AnimationState.Complete += delegate { EndCardMaking(); };

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
        translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        info.Find("FrameImage/TierBack").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["name_" + data.rarelity];
        info.Find("FrameImage/TierRibbon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["ribbon_" + data.rarelity];
        info.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;

        if (data.skills != null) {
            info.Find("SkillInfo/Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = translator.DialogSetRichText(data.skills.desc);
        }
        else
            info.Find("SkillInfo/Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = null;

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

        info.Find("FrameImage/Human").gameObject.SetActive(data.camp == "human");
        info.Find("FrameImage/Orc").gameObject.SetActive(data.camp == "orc");
        info.Find("HaveNum").gameObject.SetActive(true);
        haveNum = 0;
        if (AccountManager.Instance.cardPackage.data.ContainsKey(data.id))
            haveNum = AccountManager.Instance.cardPackage.data[data.id].cardCount;
        info.Find("HaveNum").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "I_" + haveNum.ToString(), false);


        //for (int i = 0; i < 3; i++) {
        //    info.Find("Skill&BuffRow1").GetChild(i).gameObject.SetActive(false);
        //    EventTrigger skill1 = info.Find("Skill&BuffRow1").GetChild(i).GetComponent<EventTrigger>();
        //    skill1.triggers.RemoveRange(0, skill1.triggers.Count);

        //    info.Find("Skill&BuffRow2").GetChild(i).gameObject.SetActive(false);
        //    EventTrigger skill2 = info.Find("Skill&BuffRow2").GetChild(i).GetComponent<EventTrigger>();
        //    skill2.triggers.RemoveRange(0, skill2.triggers.Count);
        //}

        info.Find("Flavor/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "";
        //int skillnum = 0;
        if (AccountManager.Instance.resource.infoPortraite.ContainsKey(data.id)) {
            info.Find("FrameImage/UnitPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoPortraite[data.id];
            if (!accountManager.cardPackage.data.ContainsKey(data.id)) {
                info.Find("FrameImage/UnitPortrait").GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            }
            else {
                info.Find("FrameImage/UnitPortrait").GetComponent<Image>().color = Color.white;
            }
        }
        if (data.type == "unit") {
            StringBuilder status = new StringBuilder();
            //for(int i = 0; i < data.attackTypes.Length; i++) {
            //    status.Append(translator.GetTranslatedSkillName(data.attackTypes[i]));
            //    status.Append(',');
            //}
            for(int i = 0; i < data.attributes.Length; i++) {
                status.Append(translator.GetTranslatedSkillName(data.attributes[i].name));
                status.Append(',');
            }
            if(status.Length != 0) {
                TMPro.TextMeshProUGUI skillText = info.Find("SkillInfo/Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>();
                if(string.IsNullOrEmpty(skillText.text))
                    skillText.text = status.ToString().RemoveLast(1);
                else
                    skillText.text = status.ToString() + skillText.text;
            }
            else {
                TMPro.TextMeshProUGUI skillText = info.Find("SkillInfo/Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>();
                skillText.text = "능력이 없습니다.";
            }
            // if (data.attackTypes.Length != 0) {
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).gameObject.SetActive(true);
            //     var image = AccountManager.Instance.resource.skillIcons[data.attackTypes[0]];
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).Find("SkillIcon").GetComponent<Image>().sprite = image;
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = translator.GetTranslatedSkillName(data.attackTypes[0]);
            //     EventTrigger.Entry onBtn = new EventTrigger.Entry();
            //     onBtn.eventID = EventTriggerType.PointerDown;
            //     onBtn.callback.AddListener((EventData) => OpenClassDescModal(data.attackTypes[0], image));
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(onBtn);
            //     EventTrigger.Entry offBtn = new EventTrigger.Entry();
            //     offBtn.eventID = EventTriggerType.PointerUp;
            //     offBtn.callback.AddListener((EventData) => CloseClassDescModal());
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(offBtn);
            //     skillnum++;
            // }
            // if (data.attributes.Length != 0) {
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).gameObject.SetActive(true);
            //     var image = AccountManager.Instance.resource.skillIcons[data.attributes[0]];
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).Find("SkillIcon").GetComponent<Image>().sprite = image;
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = translator.GetTranslatedSkillName(data.attributes[0]);
            //     EventTrigger.Entry onBtn = new EventTrigger.Entry();
            //     onBtn.eventID = EventTriggerType.PointerDown;
            //     onBtn.callback.AddListener((EventData) => OpenClassDescModal(data.attributes[0], image));
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(onBtn);
            //     EventTrigger.Entry offBtn = new EventTrigger.Entry();
            //     offBtn.eventID = EventTriggerType.PointerUp;
            //     offBtn.callback.AddListener((EventData) => CloseClassDescModal());
            //     info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<EventTrigger>().triggers.Add(offBtn);
            //     skillnum++;
            // }

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
            info.Find("SkillInfo/Categories").gameObject.SetActive(true);
            info.Find("SkillInfo/Categories/Text").GetComponent<TMPro.TextMeshProUGUI>().text = sb.ToString();
            info.Find("Flavor/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.flavorText;
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
            info.Find("SkillInfo/Categories/Text").GetComponent<TMPro.TextMeshProUGUI>().text = sb.ToString();
        }

        info.Find("FrameImage/ClassFrame").gameObject.SetActive(!data.isHeroCard);
        info.Find("Class_1").gameObject.SetActive(!data.isHeroCard);
        info.Find("HeroClass").gameObject.SetActive(data.isHeroCard);
        info.Find("HaveNum").gameObject.SetActive(!data.isHeroCard);
        info.Find("Name/HeroName").gameObject.SetActive(data.isHeroCard);
        info.Find("FrameImage/ClassFrame").gameObject.SetActive(!data.isHeroCard);
        if (data.isHeroCard) {
            info.Find("CreateCard").gameObject.SetActive(false);
            info.Find("CreateBtn").GetComponent<Button>().interactable = false;
            info.Find("CreateCard/CreateSpine").gameObject.SetActive(false);
            info.Find("FrameImage/TierRibbon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["ribbon_hero"];
        }
        else {
            info.Find("Name/HeroName").gameObject.SetActive(false);
            bool ableToCreate = !data.indestructible;
            for (int i = 0; i < 3; i++) {
                info.Find("CreateCard").GetChild(i).gameObject.SetActive(ableToCreate);
            }
            info.Find("CreateCard/CreateDisabled").gameObject.SetActive(!ableToCreate);
            info.Find("CreateCard/CreateSpine").gameObject.SetActive(ableToCreate);

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
            if (haveNum == 4)
                info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
            else {
                info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(false);
                if (makeCardcost > AccountManager.Instance.userResource.crystal)
                    info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
            }
            if (haveNum == 0)
                info.Find("CreateCard/BreakBtn/Disabled").gameObject.SetActive(true);
            else
                info.Find("CreateCard/BreakBtn/Disabled").gameObject.SetActive(false);
            info.Find("CreateCard/MakeBtn/CrystalUseValue").GetComponent<TMPro.TextMeshProUGUI>().text = "-" + makeCardcost.ToString();
            info.Find("CreateCard/BreakBtn/CrystalGetValue").GetComponent<TMPro.TextMeshProUGUI>().text = "+" + breakCardcost.ToString();
            if (makeCard) {
                if (data.rarelity == "common" && accountManager.userResource.crystal > beforeCrystal)
                    info.Find("CreateCard/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.crystal.ToString();
                else
                    StartCoroutine(AddCrystalAnimation());
            }
            else
                info.Find("CreateCard/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.crystal.ToString();

        }
        if (onTuto)
            info.Find("CreateCard/BreakBtn/Disabled").gameObject.SetActive(true);
        if (!cardCreate)
            OpenSkillWindow();
    }

    public void OpenSkillWindow() {
        transform.Find("SkillInfo").gameObject.SetActive(true);
        transform.Find("Flavor").gameObject.SetActive(false);
        transform.Find("CreateCard").gameObject.SetActive(false);
        //transform.Find("CreateSpine").gameObject.SetActive(false);
        transform.Find("SkillBtn").GetComponent<Button>().interactable = false;
        transform.Find("FlavorBtn").GetComponent<Button>().interactable = true;
        transform.Find("CreateBtn").GetComponent<Button>().interactable = true;
    }

    public void OpenFlavor() {
        transform.Find("SkillInfo").gameObject.SetActive(false);
        transform.Find("Flavor").gameObject.SetActive(true);
        transform.Find("CreateCard").gameObject.SetActive(false);
        //transform.Find("CreateSpine").gameObject.SetActive(false);
        transform.Find("SkillBtn").GetComponent<Button>().interactable = true;
        transform.Find("FlavorBtn").GetComponent<Button>().interactable = false;
        transform.Find("CreateBtn").GetComponent<Button>().interactable = true;
    }

    public void OpenCreateCard() {
        transform.Find("SkillInfo").gameObject.SetActive(false);
        transform.Find("Flavor").gameObject.SetActive(false);
        transform.Find("CreateCard").gameObject.SetActive(true);
        //transform.Find("CreateSpine").gameObject.SetActive(true);
        transform.Find("SkillBtn").GetComponent<Button>().interactable = true;
        transform.Find("FlavorBtn").GetComponent<Button>().interactable = true;
        transform.Find("CreateBtn").GetComponent<Button>().interactable = false;
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
        TMP_header.text = "<color=#149AE9>" + className + "</color>" + "\n" + desc;
    }

    public void CloseInfo() {
        if (cardCreate) return;
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.parent.Find("HeroInfo").gameObject.SetActive(false);
        transform.Find("CreateCard/BreakBtn/DisableInHand").gameObject.SetActive(false);
        editCard = null;
        bookHaveNum = 0;
        transform.parent.gameObject.SetActive(false);
        transform.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseInfo);
        if (tutoHand != null) Destroy(tutoHand);
    }

    public void CloseHeroesCardInfo() {
        SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        transform.gameObject.SetActive(false);
        transform.parent.Find("ExitTrigger2").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseHeroesCardInfo);
    }


    public void MakeCard() {
        if (cardCreate) return;
        if (Input.touchCount > 1) return;
        Modal.instantiate("이 카드를 제작하시겠습니까?", Modal.Type.YESNO, () => {
            transform.Find("CreateBlock").gameObject.SetActive(true);
            SoundManager.Instance.PlaySound(UISfxSound.CARD_CREATE);
            if (bookHaveNum == 0)
                bookHaveNum++;
            cardCreate = true;
            makeCard = true;
            transform.Find("FrameImage/UnitPortrait").GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            transform.Find("CreateCard/BreakBtn/Disabled").gameObject.SetActive(true);
            transform.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
            transform.Find("CreateCard/CreateSpine").gameObject.SetActive(true);
            transform.Find("CreateCard/CreateSpine").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "MAKING", false);
            beforeCrystal = accountManager.userResource.crystal;
            accountManager.RequestCardMake(cardId);
        });
    }

    public void BreakCard() {
        if (cardCreate) return;
        if (Input.touchCount > 1) return;
        Modal.instantiate("이 카드를 분해하시겠습니까?", Modal.Type.YESNO, () => {
            transform.Find("CreateBlock").gameObject.SetActive(true);
            SoundManager.Instance.PlaySound(UISfxSound.CARD_BREAK);
            cardCreate = true;
            makeCard = false;
            if (bookHaveNum > 0)
                bookHaveNum--;
            transform.Find("FrameImage/UnitPortrait").GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            transform.Find("CreateCard/BreakBtn/Disabled").gameObject.SetActive(true);
            transform.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
            transform.Find("CreateCard/CreateSpine").gameObject.SetActive(true);
            transform.Find("CreateCard/CreateSpine").GetComponent<SkeletonGraphic>().Update(0);
            transform.Find("CreateCard/CreateSpine").GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DECOMPOSITION", false);
            beforeCrystal = accountManager.userResource.crystal;

            accountManager.RequestCardBreak(cardId);

            string rarelity = accountManager.cardPackage.data[cardId].rarelity;
            if (accountManager.cardPackage.data[cardId].camp == "human")
                accountManager.cardPackage.rarelityHumanCardNum[rarelity].Remove(cardId);
            else
                accountManager.cardPackage.rarelityOrcCardNum[rarelity].Remove(cardId);
            accountManager.cardPackage.data.Remove(cardId);
        });
    }

    void EndCardMaking() {
        if (cardObject.name.Contains("DictionaryCard")) {
            if(!cardObject.parent.parent.name.Contains("Reward"))
                cardObject.GetComponent<MenuCardHandler>().DrawCard(cardId, isHuman);
            CardDictionaryManager.cardDictionaryManager.transform.Find("UIbar/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text
            = AccountManager.Instance.userResource.crystal.ToString();
            SetCardInfo(cardData, isHuman, cardObject, true);
        }
        else {
            cardObject.GetComponent<EditCardHandler>().deckEditController.cardButtons.MakeCard(cardObject, makeCard);
            SetCardInfo(cardData, isHuman, cardObject, true);
            if (cardObject.parent.name != "HandDeckArea" && bookHaveNum == 0)
                transform.Find("CreateCard/BreakBtn/DisableInHand").gameObject.SetActive(true);
            transform.Find("Flavor").gameObject.SetActive(false);
        }
        if (haveNum > 0)
            transform.Find("FrameImage/UnitPortrait").GetComponent<Image>().color = Color.white;
        transform.Find("CreateBlock").gameObject.SetActive(false);
        cardCreate = false;
        deckSettingManager.SetPlayerNewDecks();
    }

    GameObject tutoHand;

    public void makeShowHand() {
        Transform creating = transform.Find("CreateBtn");
        Transform disableBreak = transform.Find("CreateCard/BreakBtn/Disabled");
        BlockerController.blocker.SetBlocker(creating.gameObject);
        //tutoHand.transform.SetParent(creating.parent.parent);
        //tutoHand.name = "tutorialHand";
        onTuto = true;
        creating.GetComponent<Button>().onClick.AddListener(makingShowHand);        
    }

    public void makingShowHand() {
        Transform creating = transform.Find("CreateBtn");
        Transform make = transform.Find("CreateCard/MakeBtn");
        BlockerController.blocker.SetBlocker(make.gameObject);
        creating.GetComponent<Button>().onClick.RemoveListener(makingShowHand);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class MenuCardInfo : MonoBehaviour {
    Translator translator;
    [SerializeField] Transform classDescModal;
    string cardId;

    public virtual void SetCardInfo(dataModules.CollectionCard data, bool isHuman) {
        cardId = data.id;
        Transform info = transform;
        translator = AccountManager.Instance.GetComponent<Translator>();
        info.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;

        if (data.skills.Length != 0) {
            info.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.skills[0].desc;
        }
        else
            info.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = null;

        if (data.hp != null) {
            info.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.hp.ToString();
            info.Find("Health").gameObject.SetActive(true);
        }
        else
            info.Find("Health").gameObject.SetActive(false);

        if (data.attack != null) {
            info.Find("Attack/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.attack.ToString();
            info.Find("Attack").gameObject.SetActive(true);
        }
        else
            info.Find("Attack").gameObject.SetActive(false);

        info.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.cost.ToString();

        info.Find("Class_1").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[data.cardClasses[0]];


        for (int i = 0; i < 5; i++) {
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
                info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<Image>().sprite = image;
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
                info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<Image>().sprite = image;
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
            info.Find("Categories/Text").GetComponent<TMPro.TextMeshProUGUI>().text = sb.ToString();
            info.Find("Flavor/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.flavorText;
            info.Find("Flavor/Text").position = info.Find("Skill&BuffRow1").position;
            if (info.Find("Skill&BuffRow1").GetChild(0).gameObject.activeSelf) {
                info.Find("Flavor/Text").position = info.Find("Skill&BuffRow2").position;
                if (info.Find("Skill&BuffRow2").GetChild(0).gameObject.activeSelf)
                    info.Find("Flavor/Text").localPosition = Vector3.zero;
            }
        }
        //마법 카드
        else {
            info.Find("MagicPortrait").gameObject.SetActive(true);
            if (AccountManager.Instance.resource.cardPortraite.ContainsKey(data.id)) {
                info.Find("MagicPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[data.id];
                if (data.isHeroCard)
                    info.Find("MagicPortrait/Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_legend_human"];
                else
                    info.Find("MagicPortrait/Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["magic_" + data.rarelity];
            }
        }

        if (data.isHeroCard)
            info.Find("CreateCard").gameObject.SetActive(false);

        else {
            info.Find("CreateCard").gameObject.SetActive(true);
            info.Find("CreateCard/Crystal/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.crystal.ToString();
            int cardNum = 0;
            if (AccountManager.Instance.cardPackage.data.ContainsKey(data.id))
                cardNum = AccountManager.Instance.cardPackage.data[data.id].cardCount;
            if (cardNum < 3)
                info.Find("CreateCard/HaveNum").GetComponent<TMPro.TextMeshProUGUI>().text = cardNum.ToString();
            else
                info.Find("CreateCard/HaveNum").GetComponent<TMPro.TextMeshProUGUI>().text = "MAX";
            if (cardNum == 4) {
                info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
                info.Find("CreateCard/CrystalUseValue").gameObject.SetActive(false);
            }
            else {
                info.Find("CreateCard/CrystalUseValue").gameObject.SetActive(true);
                int makeCardcost = 0;
                switch (data.rarelity) {
                    case "common":
                        makeCardcost = 50;
                        break;
                    case "uncommon":
                        makeCardcost = 150;
                        break;
                    case "rare":
                        makeCardcost = 500;
                        break;
                    case "superrare":
                        makeCardcost = 1000;
                        break;
                    case "legend":
                        makeCardcost = 2000;
                        break;
                }
                info.Find("CreateCard/CrystalUseValue").GetComponent<TMPro.TextMeshProUGUI>().text = "-" + makeCardcost.ToString();
                if (makeCardcost >= AccountManager.Instance.userResource.crystal)
                    info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(false);
                else
                    info.Find("CreateCard/MakeBtn/Disabled").gameObject.SetActive(true);
            }
        }
    }

    public void OpenClassDescModal(string className, Sprite image) {
        if (Input.touchCount > 1) return;
        Vector3 mousePos = Input.mousePosition;
        classDescModal.gameObject.SetActive(true);
        classDescModal.position = new Vector3(mousePos.x + 20, mousePos.y + 100f, 0);
        string[] set = translator.GetTranslatedSkillSet(className);
        SetClassDescModalData(set[0], set[1], image);
    }

    public void CloseClassDescModal() {
        SetClassDescModalData();
        classDescModal.gameObject.SetActive(false);
    }

    private void SetClassDescModalData(string className = "", string desc = "", Sprite sprite = null) {
        //Transform innerModal = classDescModal.Find("InnerModal");

        TMPro.TextMeshProUGUI TMP_header = classDescModal.Find("Header").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI TMP_desc = classDescModal.Find("Description").GetComponent<TMPro.TextMeshProUGUI>();

        TMP_header.text = className + ": " + desc;
        //TMP_desc.text = desc;
        //innerModal.Find("Portrait/Image").GetComponent<Image>().sprite = sprite;
    }

    public void CloseInfo() {
        transform.parent.gameObject.SetActive(false);
        transform.gameObject.SetActive(false);
        transform.parent.Find("HeroInfo").gameObject.SetActive(false);
    }

    public void CloseHeroesCardInfo() {
        transform.gameObject.SetActive(false);
        transform.parent.Find("ExitTrigger2").gameObject.SetActive(false);
    }


    public void MakeCard() {
        AccountManager.Instance.RequestCardMake(cardId);
    }
}

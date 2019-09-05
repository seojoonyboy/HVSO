using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class MenuCardInfo : MonoBehaviour
{
    Translator translator;
    [SerializeField] Transform classDescModal;

    //public void SetCardInfo(dataModules.CollectionCard data, bool isHuman) {
    //    if (!data.isHeroCard) {
    //        transform.Find("Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["frame_" + data.rarelity];
    //        transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["name_" + data.rarelity];
    //        transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
    //        transform.Find("Dialog").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["dialog_" + data.rarelity];
    //    }
    //    else {
    //        string race;
    //        if (isHuman)
    //            race = "human_";
    //        else
    //            race = "orc_";
    //        transform.Find("Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_frame_" + race + data.rarelity];
    //        transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_name_" + race + data.rarelity];
    //        transform.Find("Dialog").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_dialog_" + race + data.rarelity];
    //    }
    //    transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
    //    if (AccountManager.Instance.resource.infoPortraite.ContainsKey(data.id)) transform.Find("Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoPortraite[data.id];
    //    if (data.skills.Length != 0) {
    //        transform.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.skills[0].desc;
    //    }
    //    else
    //        transform.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = null;

    //    if (data.hp != null) {
    //        transform.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.hp.ToString();
    //        transform.Find("Health").gameObject.SetActive(true);
    //    }
    //    else
    //        transform.Find("Health").gameObject.SetActive(false);

    //    if (data.attack != null) {
    //        transform.Find("Attack/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.attack.ToString();
    //        transform.Find("Attack").gameObject.SetActive(true);
    //    }
    //    else
    //        transform.Find("Attack").gameObject.SetActive(false);

    //    transform.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.cost.ToString();
    //    transform.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + data.cardClasses[0]];
    //    transform.Find("Class/Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + data.cardClasses[0]];
    //    transform.Find("Class/Icon").GetComponent<Image>().SetNativeSize();

    //    transform.Find("SkillIcon1").gameObject.SetActive(false);
    //    transform.Find("SkillIcon2").gameObject.SetActive(false);
    //    if (data.type == "unit") {
    //        if (data.attackTypes.Length != 0) {
    //            transform.Find("SkillIcon1").gameObject.SetActive(true);
    //            transform.Find("SkillIcon1").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[data.attackTypes[0]];
    //        }
    //        if (data.attributes.Length != 0) {
    //            transform.Find("SkillIcon1").gameObject.SetActive(true);
    //            transform.Find("SkillIcon1").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[data.attributes[0]];
    //        }
    //        if (data.attackTypes.Length != 0 && data.attributes.Length != 0) {
    //            transform.Find("SkillIcon2").gameObject.SetActive(true);
    //            transform.Find("SkillIcon2").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[data.attackTypes[0]];
    //        }
    //    }
    //}

    public virtual void SetCardInfo(dataModules.CollectionCard data, bool isHuman) {
        Transform info = transform;
        translator = AccountManager.Instance.GetComponent<Translator>();
        //string race;
        //if (isHuman)
        //    race = "human";
        //else
        //    race = "orc";
        //info.Find("Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["frame_" + race];
        //info.Find("Dialog").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["dialog_" + race];
        info.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["name_" + data.rarelity];
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

        //info.Find("SkillIcon1").gameObject.SetActive(false);
        //info.Find("SkillIcon2").gameObject.SetActive(false);

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
        }
        //마법 카드
        else {
            info.Find("MagicPortrait").gameObject.SetActive(true);
            if (AccountManager.Instance.resource.cardPortraite.ContainsKey(data.id)) {
                info.Find("MagicPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[data.id];
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
}

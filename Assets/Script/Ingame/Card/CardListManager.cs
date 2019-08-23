using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;
using Spine;
using Spine.Unity;


public class CardListManager : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform contentParent;
    [SerializeField] protected Transform standbyInfo;
    [SerializeField] GameObject infoPrefab;
    [SerializeField] protected Transform mulliganInfoList;
    private Transform handCardInfo;
    Animator animator;
    Translator translator;
    [SerializeField] Transform classDescModal;

    public Transform StandbyInfo {
        get { return standbyInfo; }
    }

    public Transform HandCardInfo {
        get { return handCardInfo; }
    }

    void Awake() {
        translator = AccountManager.Instance.GetComponent<Translator>();
    }

    void Start()
    {
        transform.GetComponent<Image>().enabled = false;
        animator = transform.GetComponentInChildren<Animator>();
        handCardInfo = transform.Find("HandCardInfo");
    }

    public virtual void AddCardInfo(CardData data) {
        GameObject newcard = standbyInfo.GetChild(0).gameObject;
        newcard.SetActive(true);
        SetCardInfo(newcard, data);
        newcard.transform.SetParent(handCardInfo);
        newcard.SetActive(false);
    }

    public void AddHeroCardInfo(GameObject info) {
        info.transform.SetParent(handCardInfo);
        info.transform.position = Vector3.zero;
        info.transform.localScale = new Vector3(1, 1, 1);
        info.SetActive(false);
    }

    public void SetEnemyMagicCardInfo(CardData data) {
        GameObject newcard = standbyInfo.GetChild(0).gameObject;
        newcard.SetActive(true);
        SetCardInfo(newcard, data);
        //newcard.transform.localScale = new Vector3(1.4f, 1.4f, 1);
    }

    public virtual void AddMulliganCardInfo(CardData data, string id, int changeNum = 100) {
        GameObject newcard;
        if (changeNum == 100) {
            newcard = standbyInfo.GetChild(0).gameObject;
            newcard.transform.SetParent(mulliganInfoList);
        }
        else
            newcard = mulliganInfoList.GetChild(changeNum).gameObject;
        SetCardInfo(newcard, data);
        newcard.SetActive(false);
    }

    public GameObject AddHeroCardInfo(CardData data) {
        GameObject heroInfo = StandbyInfo.GetChild(0).gameObject;
        SetCardInfo(heroInfo, data);
        return heroInfo;

    }

    public void AddFeildUnitInfo(int cardIndex, int unitNum, CardData data= null) {
        GameObject unitInfo;
        if (data == null)
            unitInfo = handCardInfo.GetChild(cardIndex).gameObject;
        else {
            unitInfo = standbyInfo.GetChild(0).gameObject;
            unitInfo.SetActive(true);
            SetCardInfo(unitInfo, data);
        }

        unitInfo.name = unitNum.ToString() + "unit";
        unitInfo.transform.SetParent(transform.Find("FieldUnitInfo"));
        unitInfo.SetActive(false);
    }


    public void SendMulliganInfo() {
        int i = 0;
        while(i < 4) {
            mulliganInfoList.GetChild(0).SetParent(handCardInfo);
            i++;
        }
    }

    public void OpenMulliganCardList(int cardnum) {
        if (PlayMangement.movingCard == null) {
            mulliganInfoList.gameObject.SetActive(true);
            mulliganInfoList.GetChild(cardnum).gameObject.SetActive(true);

            //mulliganInfoList.GetChild(cardnum).localScale = new Vector3(1.4f, 1.4f, 1);
        }
    }

    public void CloseMulliganCardList() {
        mulliganInfoList.gameObject.SetActive(false);
        for (int i = 0; i < 4; i++) {
            mulliganInfoList.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void RemoveCardInfo(int index) {
        GameObject remove = handCardInfo.GetChild(index).gameObject;
        remove.transform.localScale = new Vector3(1, 1, 1);
        remove.transform.SetParent(standbyInfo);
        remove.SetActive(false);
    }

    public void RemoveUnitInfo(int index) {
        string objName = index.ToString() + "unit";
        Transform remove = transform.Find("FieldUnitInfo").Find(objName);
        remove.SetParent(standbyInfo);
        remove.name = "CardInfoWindow";
        remove.gameObject.SetActive(false);
    }

    public void OpenCardInfo(int cardnum, bool showMagic = false) {
        PlayMangement.instance.infoOn = true;
        if (!showMagic) {
            transform.GetComponent<Image>().enabled = true;
            //handCardInfo.GetChild(cardnum).transform.localScale = new Vector3(1.4f, 1.4f, 1);
        }
        else {
            handCardInfo.GetChild(cardnum).transform.localScale = new Vector3(0.8f, 0.8f, 1);
        }
        handCardInfo.GetChild(cardnum).gameObject.SetActive(true);
    }

    public void CloseCardInfo() {
        PlayMangement.instance.infoOn = false;
        transform.GetComponent<Image>().enabled = false;
        for(int i = 0; i < handCardInfo.childCount; i++)
            handCardInfo.GetChild(i).gameObject.SetActive(false);
    }

    public virtual void SetCardInfo(GameObject obj, CardData data) {
        Transform info = obj.transform;
        //string race;
        //if (PlayMangement.instance.player.isHuman)
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

        //if (data.category_1 != null) {
        //    info.Find("Category_1").GetComponent<Text>().text = data.category_1.ToString();
        //    info.Find("Category_1").gameObject.SetActive(true);
        //}
        //else
        //    info.Find("Category_1").gameObject.SetActive(false);

        //if (data.category_2 != null) {
        //    info.Find("Category_2").GetComponent<Text>().text = data.category_2.ToString();
        //    info.Find("Category_2").gameObject.SetActive(false);
        //}
        //else
        //    info.Find("Category_2").gameObject.SetActive(false);

        info.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.cost.ToString();

        info.Find("Class_1").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[data.class_1];

        for(int i = 0; i < 5; i++) {
            info.Find("Skill&BuffRow1").GetChild(i).gameObject.SetActive(false);
            EventTrigger skill1 = info.Find("Skill&BuffRow1").GetChild(i).GetComponent<EventTrigger>();
            skill1.triggers.RemoveRange(0, skill1.triggers.Count);

            info.Find("Skill&BuffRow2").GetChild(i).gameObject.SetActive(false);
            EventTrigger skill2 = info.Find("Skill&BuffRow2").GetChild(i).GetComponent<EventTrigger>();
            skill2.triggers.RemoveRange(0, skill2.triggers.Count);
        }

        info.Find("Flavor/Text").GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;

        info.Find("UnitPortrait").gameObject.SetActive(false);
        info.Find("MagicPortrait").gameObject.SetActive(false);
        info.Find("Categories/Text").GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;

        int skillnum = 0;
        if (data.type == "unit") {
            info.Find("UnitPortrait").gameObject.SetActive(true);
            if (AccountManager.Instance.resource.infoPortraite.ContainsKey(data.cardId)) {
                info.Find("UnitPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoPortraite[data.cardId];
            }
            if (data.attackTypes.Length != 0) {
                info.Find("Skill&BuffRow1").GetChild(skillnum).gameObject.SetActive(true);
                var image = AccountManager.Instance.resource.skillIcons[data.attackTypes[0]];
                info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<Image>().sprite = image;
                //info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<Button>().onClick.AddListener(() => {
                //    OpenClassDescModal(data.attackTypes[0], image);
                //});
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
                //info.Find("Skill&BuffRow1").GetChild(skillnum).GetComponent<Button>().onClick.AddListener(() => {
                //    OpenClassDescModal(data.attributes[0], image);
                //});
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
            if(data.category_1 != null) categories.Add(data.category_1);
            if(data.category_2 != null) categories.Add(data.category_2);
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
            if (AccountManager.Instance.resource.cardPortraite.ContainsKey(data.cardId)) {
                info.Find("MagicPortrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[data.cardId];
            }
        }
        //if (data.class_2 == null)
        //    obj.transform.GetChild(2).gameObject.SetActive(false);
        //else {
        //    obj.transform.GetChild(2).GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[data.class_2];
        //    obj.transform.GetChild(2).name = data.class_2;
        //}
    }

    public void OpenClassDescModal(string className, Sprite image) {
        if (Input.touchCount > 1) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        classDescModal.gameObject.SetActive(true);
        classDescModal.position = new Vector3(mousePos.x, mousePos.y + 1.3f, 0);
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
    

    public virtual void OpenUnitInfoWindow(Vector3 inputPos) {
        if (!PlayMangement.instance.infoOn && Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(inputPos);

            LayerMask mask = (1 << LayerMask.NameToLayer("UnitInfo"));
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                new Vector2(mousePos.x, mousePos.y),
                Vector2.zero,
                Mathf.Infinity,
                mask
            );

            if (hits != null) {
                foreach (RaycastHit2D hit in hits) {
                    GameObject selectedTarget = hit.collider.gameObject;

                    if (selectedTarget.GetComponentInParent<ambush>() && !selectedTarget.GetComponentInParent<PlaceMonster>().isPlayer) return;

                    string objName = selectedTarget.GetComponentInParent<PlaceMonster>().myUnitNum.ToString() + "unit";
                    transform.Find("FieldUnitInfo").gameObject.SetActive(true);
                    transform.Find("FieldUnitInfo").Find(objName).gameObject.SetActive(true);
                    //transform.Find("FieldUnitInfo").Find(objName).localScale = new Vector3(1.4f, 1.4f, 1);
                    PlayMangement.instance.infoOn = true;
                }
            }
        }
    }

    public virtual void CloseUnitInfoWindow() {
        transform.Find("FieldUnitInfo").gameObject.SetActive(false);
        for (int i = 0; i < transform.Find("FieldUnitInfo").childCount; i++) {
            transform.Find("FieldUnitInfo").GetChild(i).gameObject.SetActive(false);
        }
        PlayMangement.instance.infoOn = false;
    }

}

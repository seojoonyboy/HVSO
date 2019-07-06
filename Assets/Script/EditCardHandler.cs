using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class EditCardHandler : MonoBehaviour, IPointerDownHandler, IPointerClickHandler {
    public string cardID;
    public DeckEditController deckEditController;

    [SerializeField] MenuCardInfo menuCardInfo;
    dataModules.CollectionCard cardData;
    public GameObject beforeObject;

    public bool clicking = false;
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
        if (clicking == true) {
            time += Time.deltaTime;

            if (time > 1f) {
                clicking = false;
                OpenCardInfo();
                time = 0;
            }
        }
    }

    public void SetHaveNum() {
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

    public void OpenCardInfo() {
        menuCardInfo.SetCardInfo(cardData, isHuman);
        menuCardInfo.transform.parent.gameObject.SetActive(true);
    }


    public void DrawCard(string id, bool isHuman) {
        this.isHuman = isHuman;
        cardID = id;
        cardData = AccountManager.Instance.allCardsDic[cardID];
        if (cardData.rarelity == "legend")
            transform.SetAsFirstSibling();
        Sprite portraitImage = null;
        if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardID)) portraitImage = AccountManager.Instance.resource.cardPortraite[cardID];
        else portraitImage = AccountManager.Instance.resource.cardPortraite["default"];

        transform.Find("Portrait").GetComponent<Image>().sprite = portraitImage;
        if (!cardData.isHeroCard) {
            transform.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground[cardData.type + "_" + cardData.rarelity];
            transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["name_" + cardData.rarelity];
        }
        else {
            string race;
            if (isHuman)
                race = "_human";
            else
                race = "_orc";
            transform.Find("BackGround").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race];
            transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardBackground["hero_" + cardData.rarelity + race + "_name"];
        }

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
        }
        else {
            transform.Find("Health").gameObject.SetActive(false);
            transform.Find("attack").gameObject.SetActive(false);
            transform.Find("SkillIcon").gameObject.SetActive(false);
        }
        transform.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.cost.ToString();
        transform.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[cardData.cardClasses[0]];
        transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.name;
        if (haveNum > 0 || setNum > 0) {
            transform.Find("HaveNum").gameObject.SetActive(true);
            transform.Find("HaveNum/Value").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + haveNum.ToString();
        }
        else {
            transform.Find("Disabled").gameObject.SetActive(true);
            transform.Find("HaveNum").gameObject.SetActive(false);
        }
    }
}

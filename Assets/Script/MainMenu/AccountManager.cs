using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public partial class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }

    public delegate void Callback();
    private Callback callback = null;

    public string DEVICEID { get; private set; }
    public UserClassInput userData { get; private set; }
    public List<dataModules.CardInventory> myCards { get; private set; }
    public Dictionary<string, dataModules.HeroInventory> myHeroInventories { get; private set; }

    public List<Deck> myDecks = new List<Deck>();
    public CardDataPackage cardPackage;

    public ResourceManager resource;

    NetworkManager networkManager;
    GameObject loadingModal;

    private void Awake() {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
        DEVICEID = SystemInfo.deviceUniqueIdentifier;
        cardPackage = Resources.Load("CardDatas/CardDataPackage_01") as CardDataPackage;
        resource = transform.GetComponent<ResourceManager>();
    }

    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
    }

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("네트워크 오류가 발생하였습니다. 다시 시도해 주세요.", Modal.Type.CHECK);
        NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.NETWORK_EROR_OCCURED, this, errorCode);
    }

    public void SetDummyDeck() {
        if (myCards == null) return;

        var group =
            from card in myCards
            group card by card.camp into newGroup
            orderby newGroup.Key
            select newGroup;

        foreach (var _group in group) {
            Deck deck = new Deck();
            deck.type = _group.Key;

            myDecks.Add(deck);
        }

        try {
            myDecks[0].heroName = "수비대장 제로드";
            myDecks[0].deckName = "왕국 수비대";
            myDecks[1].heroName = "족장 크라쿠스";
            myDecks[1].deckName = "암흑주술 부족";
        }
        catch (ArgumentException ex) {
            Debug.LogError("사용자 덱이 정상적으로 세팅되지 않았습니다.");
        }
    }

    private void SetHeroInventories(List<dataModules.HeroInventory> data) {
        myHeroInventories = new Dictionary<string, dataModules.HeroInventory>();
        foreach (dataModules.HeroInventory inventory in data) {
            myHeroInventories[inventory.heroId] = inventory;
        }
    }

    public void SetCardData() {
        foreach (var card in myCards) {
            if (!cardPackage.data.ContainsKey(card.cardId)) {
                CardData data = new CardData();
                data.cardId = card.cardId;
                data.attackTypes = card.attackTypes;
                data.attributes = card.attributes;
                data.rarelity = card.rarelity;
                data.type = card.type;
                data.camp = card.camp;
                data.class_1 = card.cardClasses[0];
                if (card.cardClasses.Length == 2)
                    data.class_2 = card.cardClasses[1];
                if (card.cardCategories.Length > 0) {
                    data.category_1 = card.cardCategories[0];
                    if (card.cardCategories.Length == 2)
                        data.category_2 = card.cardCategories[1];
                }
                data.name = card.name;
                data.cost = card.cost;
                data.attack = card.attack;
                data.hp = card.hp;
                data.attackRange = card.attackRange;
                data.hero_chk = card.isHeroCard;
                data.skills = card.skills;
                cardPackage.data.Add(card.cardId, data);
            }
        }
        foreach (var cards in myHeroInventories) {
            foreach(var card in cards.Value.heroCards) {
                if (!cardPackage.data.ContainsKey(card.cardId)) {
                    CardData data = new CardData();
                    data.cardId = card.cardId;                          
                    data.attackTypes = card.attackTypes;
                    data.attributes = card.attributes;
                    data.rarelity = card.rarelity;
                    data.type = card.type;
                    data.camp = card.camp;
                    data.class_1 = card.cardClasses[0];
                    if (card.cardClasses.Length == 2)
                        data.class_2 = card.cardClasses[1];
                    if (card.cardCategories.Length > 0) {
                        data.category_1 = card.cardCategories[0];
                        if (card.cardCategories.Length == 2)
                            data.category_2 = card.cardCategories[1];
                    }
                    data.name = card.name;
                    data.cost = card.cost;
                    data.attack = card.attack;
                    data.hp = card.hp;
                    data.attackRange = card.attackRange;
                    data.hero_chk = card.isHeroCard;
                    data.skills = card.skills;
                    cardPackage.data.Add(card.cardId, data);
                }
            }
        }
        
    }


    public void SetCardPackage() {

    }

    public class UserClassInput {
        public string nickName;
        public string deviceId;

        public List<dataModules.CardInventory> cardInventories;
        public List<dataModules.HeroInventory> heroInventories;
    }

    public class Deck {
        public string heroName;
        public string deckName;
        public string type;
        public List<dataModules.CardInventory> cards = new List<dataModules.CardInventory>();
    }
}

/// <summary>
/// SignIn / SignUp 관련 처리
/// </summary>
public partial class AccountManager {
    public void SignUp(string inputText) {
        UserClassInput userInfo = new UserClassInput();
        userInfo.nickName = inputText;
        userInfo.deviceId = DEVICEID;

        string json = JsonUtility.ToJson(userInfo);
        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl)
            .Append("api/users");
        networkManager.request("PUT", url.ToString(), json, CallbackSignUp, false);
    }

    public void RequestUserInfo(NetworkManager.Callback callback, NetworkManager.CallbackRetryOccured retryOccured) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID);

        Debug.Log("Request User Info");
        networkManager.request("GET", url.ToString(), callback, retryOccured);
    }

    private void CallbackSignUp(HttpResponse response) {
        if (response.responseCode != 200) {
            Debug.Log(
                response.responseCode
                + "에러\n"
                + response.errorMessage);
        }
        else {
            SetSignInData(response);

            Modal.instantiate("회원가입이 완료되었습니다.", Modal.Type.CHECK, () => {
                SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
            });
        }
    }

    public void SetSignInData(HttpResponse response) {
        userData = dataModules.JsonReader.Read<UserClassInput>(response.data);

        myCards = userData.cardInventories;
        SetHeroInventories(userData.heroInventories);
        SetCardData();
    }
}

/// <summary>
/// Login 이후 CardsInventories 관련 처리
/// </summary>
public partial class AccountManager {
    public void RequestMyCardInventory() {
        RequestUserInfo(CallbackUserRequest, OnRetry);
        loadingModal = LoadingModal.instantiate();
        loadingModal.transform.Find("Panel/AdditionalMessage").GetComponent<Text>().text = "개인 정보를 불러오는중...";
    }

    private void CallbackUserRequest(HttpResponse response) {
        if (response.responseCode != 200) {
            Modal.instantiate("유저 정보를 불러오는데 실패하였습니다.", Modal.Type.CHECK);

            Destroy(loadingModal);
        }
        else {
            userData = dataModules.JsonReader.Read<UserClassInput>(response.data);

            myCards = userData.cardInventories;
            SetHeroInventories(userData.heroInventories);
            SetCardData();
            SetDummyDeck();

            Destroy(loadingModal);
            SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
        }
    }

    private void OnRetry(string msg) {
        loadingModal.transform.GetChild(0).GetComponent<UIModule.LoadingTextEffect>().AddAdditionalMsg(msg);
    }
}

/// <summary>
/// Login 이후 Deck 관련 처리
/// </summary>
public partial class AccountManager {
    public void RequestHumanDecks(NetworkManager.Callback callback, NetworkManager.CallbackRetryOccured retryOccured) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID)
            .Append("/decks/human");

        networkManager.request("GET", url.ToString(), callback, retryOccured);
    }

    public void RequestOrcDecks(NetworkManager.Callback callback, NetworkManager.CallbackRetryOccured retryOccured) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID)
            .Append("/decks/orc");

        networkManager.request("GET", url.ToString(), callback, retryOccured);
    }
}
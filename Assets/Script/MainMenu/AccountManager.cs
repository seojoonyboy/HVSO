using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using BestHTTP;
using Newtonsoft.Json;
using dataModules;

public partial class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }

    public delegate void Callback();
    private Callback callback = null;

    public string DEVICEID { get; private set; }
    public UserClassInput userData { get; private set; }
    public List<CardInventory> myCards { get; private set; }

    public HumanDecks humanDecks;
    public OrcDecks orcDecks;
    public List<CollectionCard> allCards { get; private set; }

    public Dictionary<string, HeroInventory> myHeroInventories { get; private set; }
    public CardDataPackage cardPackage;

    public ResourceManager resource;

    NetworkManager networkManager;
    GameObject loadingModal;

    string nickName;
    public string NickName {
        get { return nickName; }
    }

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

        //RequestDeckMake();
        //TestModifyDeck();
        //RequestDeckMake();
        //RequestDeckRemove(1);
        LoadAllCards();
    }

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("네트워크 오류가 발생하였습니다. 다시 시도해 주세요.", Modal.Type.CHECK);
        NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.NETWORK_EROR_OCCURED, this, errorCode);
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

        Logger.Log("Request User Info");
        networkManager.request("GET", url.ToString(), callback, retryOccured);
    }

    private void CallbackSignUp(HttpResponse response) {
        if (response.responseCode != 200) {
            Logger.Log(
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

        nickName = userData.nickName;
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
            //SetDummyDeck();

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

public partial class AccountManager {
    /// <summary>
    /// 덱 새로 생성 Server에 요청
    /// </summary>
    public void RequestDeckMake() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/users/")
            .Append(DEVICEID)
            .Append("/decks");

        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;

        NetworkManager.AddCustomDeckReqFormat format = new NetworkManager.AddCustomDeckReqFormat();
        format.name = "Deck114";
        format.heroId = "h10001";
        format.camp = "human";

        List<NetworkManager.DeckItem> items = new List<NetworkManager.DeckItem>();
        NetworkManager.DeckItem item = new NetworkManager.DeckItem();
        item.cardId = "ac10001";
        item.cardCount = 2;
        items.Add(item);
        format.items = items.ToArray();

        //var tmp = JsonConvert.SerializeObject(format);
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(format));
        networkManager.Request(request, OnReceived);
    }

    private void OnReceived(HTTPRequest originalRequest, HTTPResponse response) {
        var result = response.DataAsText;
        Logger.Log("On Received");
    }

    /// <summary>
    /// 덱 제거 요청
    /// </summary>
    /// <param name="deckId">Deck Id</param>
    public void RequestDeckRemove(int deckId) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/users/")
            .Append(DEVICEID)
            .Append("/decks/")
            .Append(deckId.ToString());

        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Delete;

        networkManager.Request(request, OnReceived);
    }

    /// <summary>
    /// 덱 수정 요청
    /// </summary>
    /// <param name="data">양식 작성</param>
    public void RequestDeckModify(NetworkManager.ModifyDeckReqFormat data, int deckId) {
        var pairs = data.parms;

        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/users/")
            .Append(DEVICEID)
            .Append("/decks/")
            .Append(deckId);

        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = HTTPMethods.Put;

        foreach (NetworkManager.ModifyDeckReqArgs pair in pairs) {
            switch (pair.fieldName) {
                case NetworkManager.ModifyDeckReqField.NAME:
                    request.AddField("name", (string)pair.value);
                    break;
                case NetworkManager.ModifyDeckReqField.ITEMS:
                    var items = (NetworkManager.DeckItem[])pair.value;
                    request.AddField("items", JsonConvert.SerializeObject(items));
                    break;
            }
        }
        networkManager.Request(request, OnReceived);
    }

    private void TestModifyDeck() {
        NetworkManager.ModifyDeckReqFormat form = new NetworkManager.ModifyDeckReqFormat();
        NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

        field.fieldName = NetworkManager.ModifyDeckReqField.NAME;
        field.value = "Modified";
        form.parms.Add(field);

        RequestDeckModify(form, 5);
    }

    private void LoadAllCards() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/cards");

        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(
            new Uri(sb.ToString())
        );

        request.MethodType = HTTPMethods.Get;
        networkManager.Request(request, OnReceivedLoadAllCards);
    }

    private void OnReceivedLoadAllCards(HTTPRequest originalRequest, HTTPResponse response) {
        var result = dataModules.JsonReader.Read<List<CollectionCard>>(response.DataAsText);
        Logger.Log("!!");
    }
}
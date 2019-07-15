using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using BestHTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public Dictionary<string, CollectionCard> allCardsDic { get; private set; }

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
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
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
                data.flavorText = card.flavorText;
                data.cardCount = card.cardCount;
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
        //networkManager.request("PUT", url.ToString(), json, CallbackSignUp, false);
    }

    /// <summary>
    /// SignIn 요청
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="retryOccured"></param>
    public void RequestUserInfo() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID);

        //url
        //    .Append(base_url)
        //    .Append("api/users/")
        //    .Append(DEVICEID)
        //    .Append("?slow=30");

        Logger.Log("Request User Info");
        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Get;
        networkManager.Request(request, OnReqUserInfo, "유저 정보를 불러오는중...");
    }

    private void OnReqUserInfo(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null && response.IsSuccess) {
            SetSignInData(response);
            OnSignInResultModal();
        }
        else {
            if(originalRequest.RedirectCount == networkManager.MAX_REDIRECTCOUNT) {
                Modal.instantiate("네트워크가 불안정합니다. 잠시 후 재접속해주세요.", Modal.Type.CHECK);
            }
            else {
                originalRequest.RedirectCount++;
                networkManager.Request(
                    originalRequest,
                    OnReqUserInfo,
                    "유저 정보를 불러오는중... 재요청(" + originalRequest.RedirectCount + "회)"
                );
            }
        }
    }

    public void OnSignInResultModal() {
        Destroy(loadingModal);
        Modal.instantiate("로그인이 되었습니다.", Modal.Type.CHECK, () => {
            SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
        });
    }

    public void OnSignUpModal() {
        Destroy(loadingModal);
        Modal.instantiate(
            "새로운 계정을 등록합니다.",
            "닉네임을 입력하세요.",
            null,
            Modal.Type.INSERT,
            SetUserReqData);
    }

    private void SetUserReqData(string inputText) {
        SignUp(inputText);
    }

    private void CallbackSignUp(HTTPResponse response) {
        if (response.StatusCode != 200) {
            Logger.Log(
                response.StatusCode
                + "에러\n"
                + response.DataAsText.ToString());
        }
        else {
            SetSignInData(response);

            Modal.instantiate("회원가입이 완료되었습니다.", Modal.Type.CHECK, () => {
                SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
            });
        }
    }

    public void SetSignInData(HTTPResponse response) {
        userData = dataModules.JsonReader.Read<UserClassInput>(response.DataAsText.ToString());

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
    public void RequestHumanDecks(OnRequestFinishedDelegate callback = null) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID)
            .Append("/decks/human");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Get;
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "휴먼 덱을 불러오는중...");
    }

    public void RequestOrcDecks(OnRequestFinishedDelegate callback = null) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID)
            .Append("/decks/orc");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Get;
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "오크 덱을 불러오는중...");
    }
}

public partial class AccountManager {
    /// <summary>
    /// 덱 새로 생성 Server에 요청
    /// </summary>
    public void RequestDeckMake(NetworkManager.AddCustomDeckReqFormat format, OnRequestFinishedDelegate callback = null) {
        if (string.IsNullOrEmpty(format.heroId)) {
            Logger.Log("해당 커스텀 덱의 HeroId를 지정해 주세요");
            return;
        }
        if (string.IsNullOrEmpty(format.name)) {
            Logger.Log("해당 커스텀 덱의 이름을 지정해 주세요");
            return;
        }

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
        format.items = format.items.ToArray();

        //var tmp = JsonConvert.SerializeObject(format);
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(format));
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "새로운 덱을 생성하는중...");
    }

    private void OnReceived(HTTPRequest originalRequest, HTTPResponse response) {
        var result = response.DataAsText;
        Logger.Log("On Received");
    }

    /// <summary>
    /// 덱 제거 요청
    /// </summary>
    /// <param name="deckId">Deck Id</param>
    public void RequestDeckRemove(int deckId, OnRequestFinishedDelegate callback = null) {
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
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "덱 삭제를 요청하는 중...");
    }

    /// <summary>
    /// 덱 수정 요청
    /// </summary>
    /// <param name="data">양식 작성</param>
    public void RequestDeckModify(NetworkManager.ModifyDeckReqFormat data, int deckId, OnRequestFinishedDelegate callback = null) {
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

        JObject json = new JObject();
        foreach (NetworkManager.ModifyDeckReqArgs pair in pairs) {
            switch (pair.fieldName) {
                case NetworkManager.ModifyDeckReqField.NAME:
                    json["name"] = new JValue(pair.value);
                    break;
                case NetworkManager.ModifyDeckReqField.ITEMS:
                    var items = (NetworkManager.DeckItem[])pair.value;
                    json.Add("items", new JRaw(JsonConvert.SerializeObject(items)));
                    break;
            }
        }
        request.RawData =  Encoding.UTF8.GetBytes(json.ToString());
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "덱 수정 요청을 전달하는중...");
    }

    private void TestModifyDeck() {
        NetworkManager.ModifyDeckReqFormat form = new NetworkManager.ModifyDeckReqFormat();
        NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

        field.fieldName = NetworkManager.ModifyDeckReqField.NAME;
        field.value = "Modified";
        form.parms.Add(field);

        RequestDeckModify(form, 5);
    }

    public void AddDummyCustomDeck() {
        NetworkManager.AddCustomDeckReqFormat formatData = new NetworkManager.AddCustomDeckReqFormat();
        formatData.name = "Test1000";
        formatData.heroId = "h10001";   //수비 대장 제로드
        formatData.camp = "human";
        List<NetworkManager.DeckItem> items = new List<NetworkManager.DeckItem>();
        var deck = humanDecks.basicDecks[0];
        foreach (Item item in deck.items) {
            NetworkManager.DeckItem _deckItem = new NetworkManager.DeckItem();
            _deckItem.cardCount = item.cardCount;
            _deckItem.cardId = item.cardId;
            items.Add(_deckItem);
        }
        formatData.items = items.ToArray();

        RequestDeckMake(formatData);
    }

    public void LoadAllCards() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/cards");

        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(
            new Uri(sb.ToString())
        );

        request.MethodType = HTTPMethods.Get;
        networkManager.Request(request, OnReceivedLoadAllCards, "모든 카드 정보를 불러오는중...");
    }

    private void OnReceivedLoadAllCards(HTTPRequest originalRequest, HTTPResponse response) {
        if(response != null && response.IsSuccess) {
            var result = dataModules.JsonReader.Read<List<CollectionCard>>(response.DataAsText);
            allCards = result;
            allCardsDic = allCards.ToDictionary(x => x.id, x => x);
        }
        else {
            if (originalRequest.RedirectCount == networkManager.MAX_REDIRECTCOUNT) {
                Modal.instantiate("네트워크가 불안정합니다. 잠시 후 재접속해주세요.", Modal.Type.CHECK);
            }
            else {
                originalRequest.RedirectCount++;
                networkManager.Request(
                    originalRequest,
                    OnReqUserInfo,
                    "모든 카드 정보를 불러오는중... 재요청(" + originalRequest.RedirectCount + "회)"
                );
            }
        }
    }
}
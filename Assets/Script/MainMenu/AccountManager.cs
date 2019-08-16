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
    public UserInfo userData { get; private set; }
    public CardInventory[] myCards { get; private set; }

    public List<Deck> humanDecks;
    public List<Deck> orcDecks;

    public List<Templates> humanTemplates;
    public List<Templates> orcTemplates;

    public List<CollectionCard> allCards { get; private set; }
    public Dictionary<string, CollectionCard> allCardsDic { get; private set; }

    public Dictionary<string, HeroInventory> myHeroInventories { get; private set; }
    public CardDataPackage cardPackage;

    public ResourceManager resource;

    NetworkManager networkManager;
    GameObject loadingModal;
    public string NickName { get; private set; }

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

    private void SetHeroInventories(HeroInventory[] data) {
        myHeroInventories = new Dictionary<string, HeroInventory>();
        foreach (HeroInventory inventory in data) {
            myHeroInventories[inventory.heroId] = inventory;
        }
    }

    public void SetCardData() {
        foreach (CardInventory card in myCards) {
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
        foreach (KeyValuePair<string, HeroInventory> cards in myHeroInventories) {
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

    /// <summary>
    /// 회원가입, 로그인시 유저 정보 처리를 위한 클래스
    /// </summary>
    public class UserInfo {
        public string[] baasicDeckUnlock;
        public string nickName;
        public string deviceId;
        public int pass;
        
        public CardInventory[] cardInventories;
        public HeroInventory[] heroInventories;
    }
}

/// <summary>
/// SignIn / SignUp 관련 처리
/// </summary>
public partial class AccountManager {
    public async void SignUp(string inputText) {
        OTPCode otp = new OTPCode();
        while(!otp.isDone) await System.Threading.Tasks.Task.Delay(100);

        UserInfo userInfo = new UserInfo();
        userInfo.nickName = inputText;
        userInfo.deviceId = DEVICEID;
        userInfo.pass = otp.computeTotp;

        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl)
            .Append("api/user");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userInfo));
        request.MethodType = HTTPMethods.Post;

        networkManager.Request(request, CallbackSignUp, "회원가입을 요청하는 중...");
    }

    /// <summary>
    /// 유저 정보 요청
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="retryOccured"></param>
    public void RequestUserInfo(OnRequestFinishedDelegate callback = null) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/");

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
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, callback, "유저 정보를 불러오는중...");
    }

    public void OnSignInResultModal() {
        Destroy(loadingModal);
        Modal.instantiate("로그인이 되었습니다.", Modal.Type.CHECK, () => {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        });
    }

    public void OnSignUpModal() {
        Destroy(loadingModal);
        Modal.instantiate(
            "새로운 계정을 등록합니다.",
            "닉네임을 입력하세요.",
            null,
            Modal.Type.INSERT,
            SignUp);
    }

    private void CallbackSignUp(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null && response.IsSuccess) {
            Logger.Log("회원가입 요청 완료");
            AuthUser();
            RequestUserInfo((req, res) => {
                if(response != null) {
                    if (response.IsSuccess) {
                        SetSignInData(res);
                    }
                }
            });
            Modal.instantiate("회원가입이 완료되었습니다.", Modal.Type.CHECK, () => {
                FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
            });
        }
        else {
            if(response.DataAsText.Contains("already exist")) {
                Modal.instantiate("이미 해당 기기의 ID가 존재합니다.", Modal.Type.CHECK);
            }
        }
    }
    
    public void SetSignInData(HTTPResponse response) {
        userData = dataModules.JsonReader.Read<UserInfo>(response.DataAsText);
        //TODO : 인벤토리는 별도로 작업을 해야함
        //myCards = userData.cardInventories;
        //SetHeroInventories(userData.heroInventories);
        //SetCardData();

        NickName = userData.nickName;
    }
}

public partial class AccountManager {
    public void RequestMyDecks(OnRequestFinishedDelegate callback = null) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/decks");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, callback, "내 덱을 불러오는중...");
    }
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
            .Append("api/decks");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        format.items = format.items.ToArray();

        //var tmp = JsonConvert.SerializeObject(format);
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(format));
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "새로운 덱을 생성하는중...");
    }

    private void OnReceived(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.IsSuccess) {
            if(response.StatusCode == 200 || response.StatusCode == 304) {
                var result = response.DataAsText;
            }
        }
    }

    /// <summary>
    /// 덱 제거 요청
    /// </summary>
    /// <param name="deckId">Deck Id</param>
    public void RequestDeckRemove(string deckId, OnRequestFinishedDelegate callback = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/decks/")
            .Append(deckId);

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Delete;
        request.AddHeader("authorization", TokenFormat);

        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "덱 삭제를 요청하는 중...");
    }

    /// <summary>
    /// 덱 수정 요청
    /// </summary>
    /// <param name="data">양식 작성</param>
    public void RequestDeckModify(NetworkManager.ModifyDeckReqFormat data, string deckId, OnRequestFinishedDelegate callback = null) {
        var pairs = data.parms;

        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/decks/")
            .Append(deckId);

        HTTPRequest request = new HTTPRequest(
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
                    var items = (DeckEditController.DeckItem[])pair.value;
                    json.Add("items", new JRaw(JsonConvert.SerializeObject(items)));
                    break;
            }
        }
        request.RawData =  Encoding.UTF8.GetBytes(json.ToString());
        request.AddHeader("authorization", TokenFormat);
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "덱 수정 요청을 전달하는중...");
    }

    public void RequestHumanTemplates(OnRequestFinishedDelegate callback = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/decks/templates/human");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "Human 템플릿을 불러오는중...");
    }

    public void RequestOrcTemplates(OnRequestFinishedDelegate callback = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/decks/templates/orc");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "Human 템플릿을 불러오는중...");
    }

    private void TestModifyDeck() {
        NetworkManager.ModifyDeckReqFormat form = new NetworkManager.ModifyDeckReqFormat();
        NetworkManager.ModifyDeckReqArgs field = new NetworkManager.ModifyDeckReqArgs();

        field.fieldName = NetworkManager.ModifyDeckReqField.NAME;
        field.value = "Modified";
        form.parms.Add(field);

        //RequestDeckModify(form, 5);
    }

    public void AddDummyCustomDeck() {
        //NetworkManager.AddCustomDeckReqFormat formatData = new NetworkManager.AddCustomDeckReqFormat();
        //formatData.name = "Test1000";
        //formatData.heroId = "h10001";   //수비 대장 제로드
        //formatData.camp = "human";
        //List<NetworkManager.DeckItem> items = new List<NetworkManager.DeckItem>();
        //var deck = humanDecks.basicDecks[0];
        //foreach (Item item in deck.items) {
        //    NetworkManager.DeckItem _deckItem = new NetworkManager.DeckItem();
        //    _deckItem.cardCount = item.cardCount;
        //    _deckItem.cardId = item.cardId;
        //    items.Add(_deckItem);
        //}
        //formatData.items = items.ToArray();

        //RequestDeckMake(formatData);
    }

    public void LoadAllCards() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/cards");

        HTTPRequest request = new HTTPRequest(
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
    }
}

public partial class AccountManager {
    string tokenId;
    public string TokenId {
        get => tokenId;
        set {
            tokenId = value;
            TokenFormat = string.Format("Bearer {0}", TokenId);
            //Logger.Log(TokenId);
        }
    }
    public string TokenFormat { get; private set; }

    public async void AuthUser(OnRequestFinishedDelegate callback = null) {
        OTPCode otp = new OTPCode();
        while(!otp.isDone) await System.Threading.Tasks.Task.Delay(100);
        StringBuilder url = new StringBuilder();

        url
            .Append(networkManager.baseUrl)
            .Append("api/user/auth");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        TokenForm form = new TokenForm(DEVICEID, otp.computeTotp);
        request.MethodType = HTTPMethods.Post;
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(form));
        if (callback == null) callback = AuthUserCallback;

        networkManager.Request(request, callback);
    }

    public void AuthUserCallback(HTTPRequest originalRequest, HTTPResponse response) {
        if (response.IsSuccess) SetUserToken(response);
        else Logger.LogError("Token을 요청하는 과정에서 문제가 발생하였음");
    }

    public void SetUserToken(HTTPResponse response) {
        var result = dataModules.JsonReader.Read<Token>(response.DataAsText);
        TokenId = result.token;
        //Logger.Log("Token 재발행");
    }

    public class TokenForm {
        public string deviceId;
        public int pass;

        public TokenForm(string deviceId, int pass) {
            this.deviceId = deviceId;
            this.pass = pass;
        }
    }
}

public partial class AccountManager {
    public int Exp { get; private set; }

    public void ExpInc(int amount) {
        ExpReq(amount, OnExpChanged);
    }

    private void ExpReq(int amount, OnRequestFinishedDelegate callback) {
        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl)
            .Append("api/user");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        //request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userInfo));
        //request.MethodType = HTTPMethods.Post;

        //networkManager.Request(request, callback, "");
    }

    private void OnExpChanged(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null && response.IsSuccess) {
            
        }
    }
}
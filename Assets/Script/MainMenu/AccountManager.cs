using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
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
    public CardInventory[] myCards;

    public List<Deck> humanDecks;
    public List<Deck> orcDecks;

    public List<Templates> humanTemplates;
    public List<Templates> orcTemplates;

    public List<CollectionCard> allCards { get; private set; }
    public Dictionary<string, CollectionCard> allCardsDic { get; private set; }

    public Dictionary<string, HeroInventory> myHeroInventories;
    public CardDataPackage cardPackage;    

    public ResourceManager resource;
    public UserResourceManager userResource;
    public RewardClass[] rewardList;
    public DictionaryInfo dicInfo;

    NetworkManager networkManager;
    GameObject loadingModal;
    public UnityEvent OnUserResourceRefresh = new UnityEvent();
    public UnityEvent OnCardLoadFinished = new UnityEvent();

    public string NickName { get; private set; }

    private void Awake() {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DontDestroyOnLoad(gameObject);
        DEVICEID = SystemInfo.deviceUniqueIdentifier;
        cardPackage = Resources.Load("CardDatas/CardDataPackage_01") as CardDataPackage;
        resource = transform.GetComponent<ResourceManager>();
        gameObject.AddComponent<Timer.TimerManager>();
    }

    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
        dicInfo = new DictionaryInfo();
    }

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("네트워크 오류가 발생하였습니다. 다시 시도해 주세요.", Modal.Type.CHECK);
        NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.NETWORK_EROR_OCCURED, this, errorCode);
    }

    public void SetHeroInventories(HeroInventory[] data) {
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
            else {
                cardPackage.data[card.cardId].cardCount = card.cardCount;
            }
        }
        foreach (KeyValuePair<string, HeroInventory> cards in myHeroInventories) {
            foreach (var card in cards.Value.heroCards) {
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
        OnCardLoadFinished.Invoke();
    }

    /// <summary>
    /// 회원가입, 로그인시 유저 정보 처리를 위한 클래스
    /// </summary>
    public class UserInfo {
        public uint exp;
        public uint nextLvExp;
        public uint lvExp;
        public uint lv;

        public int gold;
        public double supplyTimeRemain;
        public int supply;
        public int supplyBox;
        public int manaCrystal;
        public int preSupply;
        public int additionalPreSupply;

        public string nickName;
        public string deviceId;
        public int pass;
    }
}

/// <summary>
/// SignIn / SignUp 관련 처리
/// </summary>
public partial class AccountManager {
    Timer UserReqTimer;
    const int MAX_PRE_SUPPLY = 200;
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

    private void CallbackSignUp(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null && response.IsSuccess) {
            Logger.Log("회원가입 요청 완료");
            RequestUserInfo((req, res) => {
                if (response != null) {
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
            if (response.DataAsText.Contains("already exist")) {
                Modal.instantiate("이미 해당 기기의 ID가 존재합니다.", Modal.Type.CHECK);
            }
        }
    }

    public void SetSignInData(HTTPResponse response) {
        userData = dataModules.JsonReader.Read<UserInfo>(response.DataAsText);
        NickName = userData.nickName;

        userResource.SetResource(
            lv: userData.lv,
            exp: userData.exp,
            lvExp: userData.lvExp,
            nextLvExp: userData.nextLvExp,
            gold: userData.gold,
            crystal: userData.manaCrystal,
            supplyStoreTime: (int)userData.supplyTimeRemain,
            supplyStore: userData.preSupply,
            supply: userData.supply,
            supplyBox: userData.supplyBox,
            additionalPreSupply : userData.additionalPreSupply
        );
    }

    public void SetSignInData(HTTPRequest originalRequest, HTTPResponse response) {
        userData = dataModules.JsonReader.Read<UserInfo>(response.DataAsText);
        NickName = userData.nickName;

        userResource.SetResource(
            lv: userData.lv,
            exp: userData.exp,
            lvExp: userData.lvExp,
            nextLvExp: userData.nextLvExp,
            gold: userData.gold,
            crystal: userData.manaCrystal,
            supplyStoreTime: (int)userData.supplyTimeRemain,
            supplyStore: userData.preSupply,
            supply: userData.supply,
            supplyBox: userData.supplyBox,
            additionalPreSupply: userData.additionalPreSupply
        );
        OnUserResourceRefresh.Invoke();
    }

    #region supply 갱신 처리 관련 code
    public float GetRemainSupplySec() {
        //TODO : ReqUserInfo를 통한 값 가져와 return 시키기
        float sec = (float)(TimeSpan.FromMilliseconds(userData.supplyTimeRemain).TotalSeconds);
        return sec;
    }

    public void ReqInTimer(float interval) {
        Logger.Log("Times out");
        Timer.Cancel(UserReqTimer);
        UserReqTimer = Timer.Register(
            interval,
            () => {
                RequestUserInfo((req, res) => {
                    var sceneStartController = GetComponent<SceneStartController>();
                    if (res.StatusCode == 200 || res.StatusCode == 304) {
                        SetSignInData(res);
                        if (userData.preSupply >= MAX_PRE_SUPPLY) {
                            Logger.Log("Pre Supply가 가득찼습니다.");
                            return; //TODO : preSupply가 변동되면 다시 요청 필요 
                        }
                        ReqInTimer(GetRemainSupplySec());
                    }
                });
            });
    }
    #endregion
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
            if (response.StatusCode == 200 || response.StatusCode == 304) {
                var result = response.DataAsText;
            }
        }
    }

    /// <summary>
    /// 카드 제작 요청
    /// </summary>
    public void RequestCardMake(string cardId, OnRequestFinishedDelegate callback = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/user/createcard");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        request.RawData = Encoding.UTF8.GetBytes(string.Format("{{\"cardId\":\"{0}\"}}", cardId));
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "새로운 덱을 생성하는중...");
    }

    /// <summary>
    /// 카드 해체 요청
    /// </summary>
    public void RequestCardBreak(string cardId, OnRequestFinishedDelegate callback = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/user/grindcard");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        request.RawData = Encoding.UTF8.GetBytes(string.Format("{{\"cardId\":\"{0}\"}}", cardId));
        if (callback != null) request.Callback = callback;
        networkManager.Request(request, OnReceived, "새로운 덱을 생성하는중...");
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
        request.RawData = Encoding.UTF8.GetBytes(json.ToString());
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

    public void RequestInventories(OnRequestFinishedDelegate callback = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/user/inventories");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "인벤토리 정보를 불러오는 중...");
    }

    public void RefreshInventories(OnRequestFinishedDelegate callback = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/user/inventories");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "인벤토리 정보를 불러오는 중...");
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

    public void SetRewardInfo(RewardClass[] rewardList) {
        for (int i = 0; i < rewardList.Length; i++) {
            this.rewardList[i].item = rewardList[i].item;
            this.rewardList[i].amount = rewardList[i].amount;
            this.rewardList[i].type = rewardList[i].type;
        }
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
        if (response != null && response.IsSuccess) {
            var result = dataModules.JsonReader.Read<List<CollectionCard>>(response.DataAsText);
            allCards = result;
            allCardsDic = allCards.ToDictionary(x => x.id, x => x);
            //OnCardLoadFinished.Invoke();
        }
    }
    public void RequestRewardInfo(OnRequestFinishedDelegate callback = null) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/openbox");

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
        networkManager.Request(request, callback, "박스 정보를 불러오는중...");

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

    public void ChangeNicknameReq(string val = "", OnRequestFinishedDelegate callback = null) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/change_nickname");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        NickNamechangeFormat format = new NickNamechangeFormat();
        format.nickName = val;
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(format));
        networkManager.Request(request, callback, "박스 정보를 불러오는중...");
    }

    public class NickNamechangeFormat {
        public string nickName;
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
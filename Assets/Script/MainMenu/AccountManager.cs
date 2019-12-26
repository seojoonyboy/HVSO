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

    public List<Shop> shopItems;
    public List<Mail> mailList;
    public List<MailReward> mailRewardList; 

    public List<CollectionCard> allCards { get; private set; }
    public List<HeroInventory> allHeroes { get; private set; }

    public Dictionary<string, CollectionCard> allCardsDic { get; private set; }

    public Dictionary<string, HeroInventory> myHeroInventories;
    public List<NetworkManager.ClearedStageFormat> clearedStages;

    public CardDataPackage cardPackage;    

    public ResourceManager resource;
    public UserResourceManager userResource;
    public RewardClass[] rewardList;
    public DictionaryInfo dicInfo;


    NetworkManager networkManager;
    GameObject loadingModal;
    public UnityEvent OnUserResourceRefresh = new UnityEvent();
    public UnityEvent OnCardLoadFinished = new UnityEvent();
    public LeagueData scriptable_leagueData;
    public string prevSceneName;

    private string nickName;
    public string NickName {
        get {
            return nickName;
        }
        set {
            nickName = value;
        }
    }
    public Dictionary<string, Area> rankAreas;  //랭크 구간

    public int visitDeckNow = 0;

    private void Awake() {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DontDestroyOnLoad(gameObject);
        DEVICEID = SystemInfo.deviceUniqueIdentifier;
        cardPackage = Resources.Load("CardDatas/CardDataPackage_01") as CardDataPackage;
        resource = transform.GetComponent<ResourceManager>();
        gameObject.AddComponent<Timer.TimerManager>();
        
        PlayerPrefs.DeleteKey("ReconnectData");
    }

    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
        MakeAreaDict(); //랭크 구간 생성
    }

    #if UNITY_EDITOR
    void Update() {
        if(Input.GetKeyDown(KeyCode.F)) PlayerPrefs.DeleteKey("ReconnectData");
    }
    #endif

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

    public void SetCardPackage() {
        if (cardPackage.rarelityHumanCardNum == null && cardPackage.rarelityOrcCardNum == null) {
            cardPackage.rarelityHumanCardNum = new Dictionary<string, List<string>>();
            cardPackage.rarelityOrcCardNum = new Dictionary<string, List<string>>();
            cardPackage.rarelityHumanCardCheck = new Dictionary<string, List<string>>();
            cardPackage.rarelityOrcCardCheck = new Dictionary<string, List<string>>();
            cardPackage.rarelityHumanCardNum.Add("common", new List<string>());
            cardPackage.rarelityHumanCardNum.Add("uncommon", new List<string>());
            cardPackage.rarelityHumanCardNum.Add("rare", new List<string>());
            cardPackage.rarelityHumanCardNum.Add("superrare", new List<string>());
            cardPackage.rarelityHumanCardNum.Add("legend", new List<string>());
            cardPackage.rarelityOrcCardNum.Add("common", new List<string>());
            cardPackage.rarelityOrcCardNum.Add("uncommon", new List<string>());
            cardPackage.rarelityOrcCardNum.Add("rare", new List<string>());
            cardPackage.rarelityOrcCardNum.Add("superrare", new List<string>());
            cardPackage.rarelityOrcCardNum.Add("legend", new List<string>());
            cardPackage.rarelityHumanCardCheck.Add("common", new List<string>());
            cardPackage.rarelityHumanCardCheck.Add("uncommon", new List<string>());
            cardPackage.rarelityHumanCardCheck.Add("rare", new List<string>());
            cardPackage.rarelityHumanCardCheck.Add("superrare", new List<string>());
            cardPackage.rarelityHumanCardCheck.Add("legend", new List<string>());
            cardPackage.rarelityOrcCardCheck.Add("common", new List<string>());
            cardPackage.rarelityOrcCardCheck.Add("uncommon", new List<string>());
            cardPackage.rarelityOrcCardCheck.Add("rare", new List<string>());
            cardPackage.rarelityOrcCardCheck.Add("superrare", new List<string>());
            cardPackage.rarelityOrcCardCheck.Add("legend", new List<string>());
        }

        foreach (CardInventory card in myCards) {
            if (cardPackage.data.ContainsKey(card.cardId)) {
                if (card.camp == "human") {
                    if (!cardPackage.rarelityHumanCardNum[card.rarelity].Contains(card.cardId))
                        cardPackage.rarelityHumanCardNum[card.rarelity].Add(card.cardId);
                    if (cardPackage.checkHumanCard.Contains(card.cardId))
                        cardPackage.rarelityHumanCardCheck[card.rarelity].Add(card.cardId);
                }
                else {
                    if (!cardPackage.rarelityOrcCardNum[card.rarelity].Contains(card.cardId))
                        cardPackage.rarelityOrcCardNum[card.rarelity].Add(card.cardId);
                    if (cardPackage.checkOrcCard.Contains(card.cardId))
                        cardPackage.rarelityOrcCardCheck[card.rarelity].Add(card.cardId);
                }
            }
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
                if (card.cardClasses == null) Debug.Log(card.cardId);
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
                data.indestructible = card.indestructible;
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
                    data.indestructible = card.indestructible;
                    cardPackage.data.Add(card.cardId, data);
                }
            }
        }
        SetCardPackage();
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
        public double mainAdTimeRemain;
        public int supply;
        public int supplyBox;
        public int crystal;
        public int preSupply;
        public int supplyX2Coupon;
        public int mainAdCount;
        public string nickName;
        public string deviceId;
        public int pass;

        public List<etcInfo> etcInfo;
    }

    public class etcInfo {
        //public int id;
        //public int userId;
        public string key;
        public string value;
    }

    public bool IsTutorialCleared() {
        var data = userData.etcInfo.Find(x => x.key == "tutorialCleared");
        if (data == null) return false;
        return data.value == "true";
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
    public void RequestUserInfo() {
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
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                var sceneStartController = GetComponent<SceneStartController>();
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    SetSignInData(res);

                    if (userData.preSupply >= MAX_PRE_SUPPLY) {
                        Logger.Log("Pre Supply가 가득찼습니다.");
                    }
                    else {
                        ReqInTimer(GetRemainSupplySec());
                    }

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED,
                            null,
                            res
                        );
                }
            },
            "유저 정보를 불러오는중...");
    }

    public void RequestUserInfo(OnRequestFinishedDelegate callback) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "유저 정보를 불러오는중...");
    }

    public void OnSignInResultModal() {
        Destroy(loadingModal);
        AdsManager.Instance.Init();
        Modal.instantiate("로그인이 되었습니다.", Modal.Type.CHECK, () => {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        });
    }

    private void CallbackSignUp(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null && response.IsSuccess) {
            Logger.Log("회원가입 요청 완료");
            RequestUserInfo();

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
            crystal: userData.crystal,
            supplyStoreTime: (int)userData.supplyTimeRemain,
            mainAdTimeRemain: (int)userData.mainAdTimeRemain,
            mainAdCount: userData.mainAdCount,
            supplyStore: userData.preSupply,
            supply: userData.supply,
            supplyBox: userData.supplyBox,
            supplyX2Coupon: userData.supplyX2Coupon
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
            crystal: userData.crystal,
            supplyStoreTime: (int)userData.supplyTimeRemain,
            mainAdTimeRemain: (int)userData.mainAdTimeRemain,
            mainAdCount: userData.mainAdCount,
            supplyStore: userData.preSupply,
            supply: userData.supply,
            supplyBox: userData.supplyBox,
            supplyX2Coupon: userData.supplyX2Coupon
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
                RequestUserInfo();
            });
    }
    #endregion
}

public partial class AccountManager {
    public void RequestMyDecks() {
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
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<Decks>(res.DataAsText);
                    orcDecks = result.orc;
                    humanDecks = result.human;

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("내 덱 불러오기 실패");
            }
        }, "내 덱을 불러오는중...");
    }
    /// <summary>
    /// 덱 새로 생성 Server에 요청
    /// </summary>
    public void RequestDeckMake(NetworkManager.AddCustomDeckReqFormat format) {
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

        networkManager.Request(
            request, 
            (req, res) => {
                if (res.IsSuccess) {
                    if (res.StatusCode == 200 || res.StatusCode == 304) {
                        NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECK_CREATED, null, format.bannerImage);
                        RequestMyDecks();
                    }
                }
                else {
                    Logger.LogWarning("덱 정보 갱신 실패");
                }
            },
            "새로운 덱을 생성하는중...");
    }

    /// <summary>
    /// 영웅 티어업 요청
    /// </summary>
    public void RequestHeroTierUp(string heroId) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/user/herotier_up");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        request.RawData = Encoding.UTF8.GetBytes(string.Format("{{\"heroId\":\"{0}\"}}", heroId));
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_TIERUP_HERO,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("영웅 티어업 실패");
            }
        }, "영웅 티어업 하는중...");
    }

    /// <summary>
    /// 카드 제작 요청
    /// </summary>
    public void RequestCardMake(string cardId) {
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
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_CREATE_CARD,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("카드 제작 실패");
            }
        }, "새로운 카드 제작하는중...");
    }

    /// <summary>
    /// 카드 해체 요청
    /// </summary>
    public void RequestCardBreak(string cardId) {
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

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_REMOVE_CARD,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("카드 제거 실패");
            }
        }, "카드를 제거하는 중...");
    }


    /// <summary>
    /// 덱 제거 요청
    /// </summary>
    /// <param name="deckId">Deck Id</param>
    public void RequestDeckRemove(string deckId) {
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

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECK_REMOVED, null, res);
                    RequestMyDecks();
                }
            }
            else {
                Logger.LogWarning("덱 삭제 실패");
            }
        }, "덱 삭제를 요청하는 중...");
    }

    /// <summary>
    /// 덱 수정 요청
    /// </summary>
    /// <param name="data">양식 작성</param>
    public void RequestDeckModify(NetworkManager.ModifyDeckReqFormat data, string deckId) {
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

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECK_MODIFIED, null, res);
                    RequestMyDecks();
                }
            }
            else {
                Logger.LogWarning("덱 수정 실패");
            }
        }, "덱 수정 요청을 전달하는중...");
    }

    public void RequestHumanTemplates() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/decks/templates/human");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_HUMAN_TEMPLATES_UPDATED,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("템플릿 정보 가져오기 실패");
            }
        }, "Human 템플릿을 불러오는중...");
    }

    public void RequestOrcTemplates() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/decks/templates/orc");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ORC_TEMPLATES_UPDATED,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("템플릿 정보 가져오기 실패");
            }
        }, "orc 템플릿을 불러오는중...");
    }

    public void RequestInventories() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/user/inventories");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<MyCardsInfo>(res.DataAsText);

                    myCards = result.cardInventories;
                    SetHeroInventories(result.heroInventories);

                    SetCardData();
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("인벤토리 정보 가져오기 실패");
            }
        }, "인벤토리 정보를 불러오는 중...");
    }

    public void RequestMainAdReward() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/claim_reward?kind=ad&placementName=main&rewardName=presupply&rewardAmount=40");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_MAIN,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("광고 보상 받기 실패");
            }
        }, "광고 보상 불러오는 중...");
    }


    public void RequestShopItems() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/shop");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<List<Shop>>(res.DataAsText);
                    shopItems = result;

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_UPDATED,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("상점 불러오기 실패");
            }
        }, "상점을 불러오는중...");
    }

    public void RequestBuyItem(string itemId) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/shop/buy");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        request.RawData = Encoding.UTF8.GetBytes(string.Format("{{\"id\":\"{0}\"}}", itemId));

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_SHOP_ITEM_BUY,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("상품 구매 실패");
            }
        }, "상품 구매 중...");
    }

    public void RequestMailBox() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/posts");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<List<Mail>>(res.DataAsText);
                    mailList = result;

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_UPDATE,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("우편함 불러오기 실패");
            }
        }, "우편함 불러오는중...");
    }

    public void RequestReadMail(string mailId) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/posts/" + mailId);

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_READ,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("우편 읽기 실패");
            }
        }, "우편 불러오는중...");
    }

    public void RequestReceiveMail(string mailId) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/posts/receive/" + mailId);

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<List<MailReward>>(res.DataAsText);
                    mailRewardList = result;
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_RECEIVE,
                            null,
                            res
                        );
                }
            }
            else {
                Debug.Log(res.DataAsText);
                Logger.LogWarning("상품 구매 실패");
            }
        }, "상품 구매 중...");
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
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<List<CollectionCard>>(res.DataAsText);
                    allCards = result;
                    allCardsDic = allCards.ToDictionary(x => x.id, x => x);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_CARDS_UPDATED,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("모든 카드 정보 가져오기 실패");
            }
        }, "모든 카드 정보를 불러오는중...");
    }

    public void LoadAllHeroes() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/heroes");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );

        request.MethodType = HTTPMethods.Get;
        networkManager.Request(request, OnReceivedLoadAllHeroes, "모든 카드 정보를 불러오는중...");
    }

    private void OnReceivedLoadAllHeroes(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null && response.IsSuccess) {
            var result = dataModules.JsonReader.Read<List<HeroInventory>>(response.DataAsText);
            allHeroes = result;
            //OnCardLoadFinished.Invoke();
        }
    }
    public void RequestRewardInfo() {
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
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<RewardClass[]>(res.DataAsText);

                    rewardList = result;
                    SetRewardInfo(result);
                    RequestUserInfo();

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("박스 정보 가져오기 실패");
            }
        }, "박스 정보를 불러오는중...");

    }
}

public partial class AccountManager {
    public UnityAction tokenSetFinished;
    string tokenId;
    public string TokenId {
        get => tokenId;
        set {
            tokenId = value;
            TokenFormat = string.Format("Bearer {0}", TokenId);
            Logger.Log(TokenFormat);
            tokenSetFinished.Invoke();
        }
    }
    public string TokenFormat { get; private set; }

    public class TokenForm {
        public string deviceId;
        public int pass;

        public TokenForm(string deviceId, int pass) {
            this.deviceId = deviceId;
            this.pass = pass;
        }
    }

    public void ChangeNicknameReq(string val = "", UnityAction callback = null) {
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
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    needChangeNickName = false;
                    NickName = val;

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_NICKNAME_UPDATED,
                            null,
                            res
                        );
                    if(callback != null) callback();
                }
            }
            else {
                Logger.LogWarning("닉네임 변경하기 실패");
            }
        }, "닉네임을 변경하는 중...");
    }

    public class NickNamechangeFormat {
        public string nickName;
    }
}

public partial class AccountManager {
    /// <summary>
    /// 튜토리얼 보상 요청
    /// </summary>
    /// <param name="callback"></param>
    public void RequestIngameTutorialReward(OnRequestFinishedDelegate callback, string camp) {
        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl);
        url.Append("api/user/claim_reward?");
        string parm = null;
        if(camp == "orc") {
            parm = "kind=orcDefaultDeck";
        }
        else if(camp == "human") {
            parm = "kind=humanDefaultDeck";
        }

        if (string.IsNullOrEmpty(parm)) return;

        url.Append(parm);
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, callback, "보상 받기를 기다리는 중...");
    }

    public void RequestClearedStoryList(OnRequestFinishedDelegate callback) {
        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl);
        url.Append("api/user/stage_cleared");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, callback, "");
    }

    public void RequestClearedStoryList() {
        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl);
        url.Append("api/user/stage_cleared");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    clearedStages = dataModules.JsonReader
                        .Read<List<NetworkManager.ClearedStageFormat>>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_CLEARED_STAGE_UPDATED,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("클리어한 스테이지 정보 불러오기 실패");
            }
        }, "스테이지 진척도 불러오는 중...");
    }

    public void RequestTutorialBox(OnRequestFinishedDelegate callback) {
        StringBuilder url = new StringBuilder();

        url.Append(networkManager.baseUrl);
        url.Append("api/user/claim_reward?kind=tutorialBox");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(
            request, 
            (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    Logger.Log(res.DataAsText);
                    if (res.DataAsText.Contains("already")) {
                        Logger.Log("이미 보상 받았음");
                    }
                    else {
                        var result = dataModules.JsonReader.Read<RewardClass[]>(res.DataAsText);

                        rewardList = result;
                        SetRewardInfo(result);

                        NoneIngameSceneEventHandler.Instance.PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_OPENBOX,
                            null,
                            res
                        );
                    }
                }
                callback(req, res);
            },
            "보상 내역 확인중..."
            );
    }

    public void RequestTutorialPreSettings() {
        RequestUserInfo((req, res) => {
            if (res.IsSuccess) {
                SetSignInData(res);
                NoneIngameSceneEventHandler
                    .Instance
                    .PostNotification(
                        NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED,
                        null,
                        res
                    );
                
                RequestClearedStoryList((_req, _res) => {
                    if (_res.IsSuccess) {
                        clearedStages = dataModules.JsonReader
                        .Read<List<NetworkManager.ClearedStageFormat>>(_res.DataAsText);

                        NoneIngameSceneEventHandler
                            .Instance
                            .PostNotification(
                                NoneIngameSceneEventHandler.EVENT_TYPE.API_CLEARED_STAGE_UPDATED,
                                null,
                                _res
                            );

                        NoneIngameSceneEventHandler
                            .Instance
                            .PostNotification(
                                NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_PRESETTING_COMPLETE,
                                null
                            );
                    }
                    else {
                        Debug.LogError(_res.IsSuccess);
                        Debug.LogError(_res.DataAsText);
                        Logger.LogWarning("요청 실패로 튜토리얼 진행 문제 발생");
                    }
                });
            }
            else {
                Logger.LogWarning("요청 실패로 튜토리얼 진행 문제 발생");
            }
        });
    }

    public bool needChangeNickName = false;
}

public partial class AccountManager {
    [Serializable]
    public class LeagueInfo {
        public int modifiedRatingPoint;
        public RankDetail rankDetail;

        public int? id;
        public int userId;
        //public int leagueId;
        public int ratingPoint;
        public int winningStreak;
        public int losingStreak;
        public string rankingBattleState;
        public bool[] rankingBattleCount;
        public int? ratingPointTop;
        public List<Reward> rewards;

        public LeagueInfo DeepCopy(LeagueInfo originData) {
            LeagueInfo leagueInfo = new LeagueInfo();
            leagueInfo.modifiedRatingPoint = originData.modifiedRatingPoint;
            leagueInfo.ratingPoint = originData.ratingPoint;
            leagueInfo.ratingPointTop = originData.ratingPointTop;
            //leagueInfo.rankingBattleCount = originData.rankingBattleCount;
            leagueInfo.rankingBattleCount = new bool[originData.rankingBattleCount.Length];
            for(int i=0; i<originData.rankingBattleCount.Length; i++) {
                rankingBattleCount[i] = originData.rankingBattleCount[i];
            }
            leagueInfo.rankingBattleState = originData.rankingBattleState;
            leagueInfo.rewards = originData.rewards;

            leagueInfo.rankDetail = new RankDetail();
            leagueInfo.rankDetail.majorRankName = originData.rankDetail.majorRankName;
            leagueInfo.rankDetail.minorRankName = originData.rankDetail.minorRankName;
            leagueInfo.rankDetail.pointLessThen = originData.rankDetail.pointLessThen;
            leagueInfo.rankDetail.pointOverThen = originData.rankDetail.pointOverThen;
            

            if(originData.rankDetail.rankDownBattleCount != null) {
                leagueInfo.rankDetail.rankDownBattleCount = new RankUpCondition(originData.rankDetail.rankDownBattleCount.needTo, originData.rankDetail.rankDownBattleCount.battles);
            }

            if(originData.rankDetail.rankUpBattleCount != null) {
                leagueInfo.rankDetail.rankUpBattleCount = new RankUpCondition(originData.rankDetail.rankUpBattleCount.needTo, originData.rankDetail.rankUpBattleCount.battles);
            }

            return leagueInfo;
        }
    }

    [Serializable]
    public class RankDetail {
        public RankUpCondition rankUpBattleCount;
        public RankUpCondition rankDownBattleCount;

        public string majorRankName;
        public string minorRankName;
        public int pointOverThen;
        public int pointLessThen;
    }

    [Serializable]
    public class RankUpCondition {
        public int needTo;
        public int battles;

        public RankUpCondition(int needTo, int battles) {
            this.needTo = needTo;
            this.battles = battles;
        }
    }

    public class Reward {
        public bool claimed;
        public bool canClaim;
        public RewardInfo reward;
        public int id;
        public int point;
    }

    public class RewardInfo {
        public string kind;
        public string amount;
    }

    /// <summary>
    /// 플레이어 리그 정보 요청
    /// </summary>
    public void RequestLeagueInfo() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/league_info");

        Logger.Log("Request league_info");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                var sceneStartController = GetComponent<SceneStartController>();
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var leagueInfo = dataModules.JsonReader.Read<LeagueInfo>(res.DataAsText);

                    if(prevSceneName == "Login") {
                        Logger.Log("이전 씬이 Ingame이 아닌 경우");
                        scriptable_leagueData.leagueInfo = leagueInfo;
                        scriptable_leagueData.prevLeagueInfo = leagueInfo.DeepCopy(leagueInfo);
                    }

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED,
                            null,
                            leagueInfo
                        );
                }
            },
            "리그 정보를 불러오는중...");
    }

    public void RequestLeagueReward(OnRequestFinishedDelegate callback, int rewardId) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/claim_reward")
            .Append("?kind=rating&rewardId=")
            .Append(rewardId.ToString());

        Logger.Log("RequestLeagueReward");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                callback(req, res);

                RequestLeagueInfo();
            },
            "리그 보상 요청...");
    }

    public List<RankTableRow> rankTable = new List<RankTableRow>();

    public void RequestRankTable() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/info/rank_table");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                var sceneStartController = GetComponent<SceneStartController>();
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    rankTable = dataModules.JsonReader.Read<List<RankTableRow>>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_RANK_TABLE_RECEIVED,
                            null,
                            rankTable
                        );
                }
            },
            "등급 테이블을 불러오는중...");
    }

    private void MakeAreaDict() {
        dicInfo = new DictionaryInfo();
        rankAreas = new Dictionary<string, Area>();
        rankAreas.Add("무명 병사", new Area(0, 149));
        rankAreas.Add("오합지졸 우두머리", new Area(150, 299));
        rankAreas.Add("소규모 무력집단", new Area(300, 449));
        rankAreas.Add("지역 지도자", new Area(450, 599));
        rankAreas.Add("자경대 대장", new Area(600, 799));
        rankAreas.Add("초급 용병단장", new Area(800, 999));
        rankAreas.Add("베테랑 용병단장", new Area(1000, 1199));
        rankAreas.Add("최정예 용병단장", new Area(1200, 1399));
        rankAreas.Add("정규군 지휘관", new Area(1400, 1699));
        rankAreas.Add("군단장", new Area(1700, 1999));
        rankAreas.Add("대장군", new Area(2000, 2299));
        rankAreas.Add("총사령관", new Area(2300, 2599));
        rankAreas.Add("찬란한 영웅", new Area(2600, 2999));
    }

    public Area GetTargetRankArea(string keyword) {
        if (rankAreas.ContainsKey(keyword)) {
            return rankAreas[keyword];
        }
        return null;
    }

    public class Area {
        public int Min;
        public int Max;

        public Area(int Min, int Max) {
            this.Max = Max;
            this.Min = Min;
        }
    }

    public class RankTableRow {
        //public RankUpCondition rankUpBattleCount;
        //public RankUpCondition rankDownBattleCount;
        public string majorRankName;
        public string minorRankName;
        public int? pointOverThen;
        public int? pointLessThen;

        public int id;
    }
}

public partial class AccountManager {
    public List<QuestInfo> questInfos;

    public class QuestInfo {
        public int id;
        public string name;
        public bool cleared;
        public List<QuestUnlockInfo> after;
    }

    public class QuestUnlockInfo {
        public string method;
        public string[] args;
    }

    /// <summary>
    /// 메뉴 해금/잠금 여부 목록
    /// </summary>
    public void RequestTutorialUnlockInfos() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/tutorial_info");

        Logger.Log("Request tutorial_info");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    questInfos = dataModules.JsonReader.Read<List<QuestInfo>>(res.DataAsText);
                    
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_INFOS_UPDATED,
                            null,
                            questInfos
                        );

                    RequestUserInfo();
                }
            },
            "튜토리얼 정보를 불러오는중...");
    }

    /// <summary>
    /// 튜토리얼 완료 처리 요청
    /// </summary>
    /// <param name="id"></param>
    public void RequestUnlockInTutorial(int id) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/tutorial_cleared");

        url.Append("/" + id);
        Logger.Log("Request POST tutorial_info");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));

        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request,
            (req, res) => {
                if (res.IsSuccess) {
                    if(res.StatusCode == 200 || res.StatusCode == 304) {
                        RequestTutorialUnlockInfos();
                        //RequestQuestInfo();
                    }
                }
            }, 
            "튜토리얼 완료 처리중..."
            );
    }

    public void RequestQuestClearReward(int id, GameObject obj) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/quest/get_reward");

        url.Append("/" + id);
        Logger.Log("RequestQuestClearReward");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));

        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request,
            (req, res) => {
                Logger.Log(res.DataAsText);
                if (res.IsSuccess) {
                    if (res.StatusCode == 200 || res.StatusCode == 304) {
                        RequestMailBox();

                        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REWARD_RECEIVED,
                            null,
                            obj
                        );
                    }
                }
            },
            "튜토리얼 보상 요청중..."
            );
    }
}

public partial class AccountManager {
    public void RequestQuestInfo() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/quest");

        Logger.Log("Request Quest Info");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var QuestData = dataModules.JsonReader.Read<Quest.QuestData[]>(res.DataAsText);
                    
                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED,
                            null,
                            QuestData
                        );

                    RequestUserInfo();
                }
            },
            "튜토리얼 정보를 불러오는중...");
    }

    public void RequestQuestProgress(int questId) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/quest/progress/")
            .Append(questId);

        Logger.Log("Request Quest Info");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    // var QuestData = dataModules.JsonReader.Read<Quest.QuestData[]>(res.DataAsText);
                    
                    // NoneIngameSceneEventHandler
                    //     .Instance
                    //     .PostNotification(
                    //         NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED,
                    //         null,
                    //         QuestData
                    //     );

                    // RequestUserInfo();
                }
            },
            "튜토리얼 정보를 불러오는중...");
    }

    public void SkipStoryRequest(string camp, int stageNumber) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/test_helper/tutorial/")
            .Append(camp)
            .Append("/")
            .Append(stageNumber);

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        PlayerPrefs.SetString("SelectedBattleButton", "STORY");
        PlayerPrefs.SetInt("isFirst", 0);

        MenuLockController.MenuNameData lockMenuList = new MenuLockController.MenuNameData();
        MenuLockController.MenuNameData unlockMenuList = new MenuLockController.MenuNameData();

        lockMenuList.menuNameList = new List<string>();
        unlockMenuList.menuNameList = new List<string>();

        lockMenuList.menuNameList.Add("League");
        //lockMenuList.menuNameList.Add("Story");
        lockMenuList.menuNameList.Add("DeckEdit");
        lockMenuList.menuNameList.Add("Dictionary");
        lockMenuList.menuNameList.Add("Shop");
        lockMenuList.menuNameList.Add("RewardBox");
        lockMenuList.menuNameList.Add("Mode");
        lockMenuList.menuNameList.Add("HumanBaseDeckAiBattleBtn");
        lockMenuList.menuNameList.Add("HumanBaseDeckDeleteBtn");
        lockMenuList.menuNameList.Add("OrcBaseDeckAiBattleBtn");
        lockMenuList.menuNameList.Add("OrcBaseDeckDeleteBtn");

        unlockMenuList.menuNameList.Add("Story");

        PlayerPrefs.SetString("lockMenuList", JsonConvert.SerializeObject(lockMenuList));
        PlayerPrefs.SetString("unlockMenuList", JsonConvert.SerializeObject(unlockMenuList));

        if (stageNumber == 1) {
            if(camp == "human") {
                RequestIngameTutorialReward((req, res) => {
                    Modal.instantiate("휴먼 기본부대 획득", Modal.Type.CHECK);
                }, camp);
            }
            else {
                RequestIngameTutorialReward((req, res) => {
                    Modal.instantiate("오크 기본부대 획득", Modal.Type.CHECK);
                }, camp);
            }
        }

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    Modal.instantiate("튜토리얼 " + camp + ", " + stageNumber + " 스킵 완료", Modal.Type.CHECK);
                }
                else {
                    Modal.instantiate("튜토리얼 skip 요청 오류", Modal.Type.CHECK);
                    Logger.LogError(res.DataAsText);
                }
            },
            "튜토리얼 스킵 요청중...");
    }

    public void RequestChangeMMRForTest(int mmr) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        if (mmr < 0) mmr = 0;
        else if (mmr > 3000) mmr = 3000;

        url
            .Append(base_url)
            .Append("api/test_helper/league_info/")
            .Append(mmr);

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    Modal.instantiate("MMR 변경 완료", Modal.Type.CHECK);
                }
                else {
                    Modal.instantiate("MMR 변경 요청 오류", Modal.Type.CHECK);
                    Logger.LogError(res.DataAsText);
                }
                
            },
            "MMR 변경 요청중...");
    }
}
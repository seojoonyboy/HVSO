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
using UnityEngine.SceneManagement;
using Quest;
using TMPro;
using JsonReader = dataModules.JsonReader;

public partial class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }

    public delegate void Callback();
    private Callback callback = null;

    public string DEVICEID { get; private set; }
    public UserInfo userData { get; private set; }
    public UserStatistics userStatistics;
    public CardInventory[] myCards;

    public List<Deck> humanDecks;
    public List<Deck> orcDecks;

    public List<Templates> humanTemplates;
    public List<Templates> orcTemplates;

    public List<Shop> shopItems;
    public List<Mail> mailList;
    public int mailNum;
    public List<MailReward> mailRewardList;

    public Queue<MainWindowEffectManager.Effect> mainSceneEffects = new Queue<MainWindowEffectManager.Effect>();
    
    public List<CollectionCard> allCards {
        get => _allCards;
        private set => _allCards = value;
    }

    public List<HeroInventory> allHeroes { get; private set; }

    public Dictionary<string, CollectionCard> allCardsDic { get; private set; }

    public Dictionary<string, HeroInventory> myHeroInventories;
    public List<NetworkManager.ClearedStageFormat> clearedStages;

    public CardDataPackage cardPackage;    

    public ResourceManager resource;
    public UserResourceManager userResource;
    public RewardClass[] rewardList;
    public RewardClass boxAdReward;
    public DictionaryInfo dicInfo;
    public AdReward[] shopAdsList;
    public AdRewardRequestResult adRewardResult;
    public AttendanceResult attendanceResult;
    public AttendanceReward attendanceBoard;
    public BuyBoxInfo buyBoxInfo;
    public BattleRank battleRank;
    public TotalRank totalRank;


    NetworkManager networkManager;
    GameObject loadingModal;
    public UnityEvent OnUserResourceRefresh = new UnityEvent();
    public UnityEvent OnCardLoadFinished = new UnityEvent();
    public LeagueData scriptable_leagueData;
    public string prevSceneName;
    public bool canLoadDailyQuest = false;
    public bool needToReturnBattleReadyScene = false;

    public bool startSpread = false;
    public int beforeBox;
    public int beforeSupply;

    private string nickName;
    public string NickName {
        get {
            return nickName;
        }
        set {
            nickName = value;
        }
    }
    public Dictionary<string, Area> rankAreas;  //?????? ??????

    public int visitDeckNow = 0;
    string languageSetting;
    private List<CollectionCard> _allCards;

    private void Awake() {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Input.multiTouchEnabled = false;
        DontDestroyOnLoad(gameObject);
        DEVICEID = SystemInfo.deviceUniqueIdentifier;
        cardPackage = Resources.Load("CardDatas/CardDataPackage_01") as CardDataPackage;
        resource = transform.GetComponent<ResourceManager>();
        gameObject.AddComponent<Timer.TimerManager>();
        dicInfo = new DictionaryInfo();

        //TOOD : Server??? ?????? Setting?????? ??????

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("Language", string.Empty))) languageSetting = Application.systemLanguage.ToString();
        else languageSetting = PlayerPrefs.GetString("Language");

        SetLanguageSetting(languageSetting);
        //????????? ??????
        //PlayerPrefs.SetInt("IsQuestLoaded", 0);
    }

    public void SetLanguageSetting(string language) {
        PlayerPrefs.SetString("Language", language);
        languageSetting = language;
    }

    void Start() {
        networkManager = NetworkManager.Instance;
    }

    public string GetLanguageSetting() {
        return languageSetting;
    }

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("???????????? ????????? ?????????????????????. ?????? ????????? ?????????.", Modal.Type.CHECK);
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
            cardPackage.checkHumanHero = new List<string>();
            cardPackage.checkOrcHero = new List<string>();
            cardPackage.checkHumanCard = new List<string>();
            cardPackage.checkOrcCard = new List<string>();
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

        //SetNewCardsByRarlty();
    }

    public void SetNewCardsByRarlity() {
        foreach (CardInventory card in myCards) {
            if (cardPackage.data.ContainsKey(card.cardId)) {
                if (card.camp == "human") {
                    if (!cardPackage.rarelityHumanCardNum[card.rarelity].Contains(card.cardId))
                        cardPackage.rarelityHumanCardNum[card.rarelity].Add(card.cardId);
                    if (NewAlertManager.Instance.GetUnlockCondionsList().Exists(x => x.Contains("DICTIONARY_card_" + card.cardId))) {
                        if(!cardPackage.rarelityHumanCardCheck[card.rarelity].Contains(card.cardId))
                            cardPackage.rarelityHumanCardCheck[card.rarelity].Add(card.cardId);
                    }
                    else {
                        if(cardPackage.rarelityHumanCardCheck[card.rarelity].Contains(card.cardId))
                            cardPackage.rarelityHumanCardCheck[card.rarelity].Remove(card.cardId);
                    }
                }
                else {
                    if (!cardPackage.rarelityOrcCardNum[card.rarelity].Contains(card.cardId))
                        cardPackage.rarelityOrcCardNum[card.rarelity].Add(card.cardId);
                    if (NewAlertManager.Instance.GetUnlockCondionsList().Exists(x => x.Contains("DICTIONARY_card_" + card.cardId))) {
                        if (!cardPackage.rarelityOrcCardCheck[card.rarelity].Contains(card.cardId))
                            cardPackage.rarelityOrcCardCheck[card.rarelity].Add(card.cardId);
                    }
                    else {
                        if (cardPackage.rarelityOrcCardCheck[card.rarelity].Contains(card.cardId))
                            cardPackage.rarelityOrcCardCheck[card.rarelity].Remove(card.cardId);
                    }
                }
            }
        }
    }

    public void SetNewHeroInfos() {
        foreach (HeroInventory inventory in myHeroInventories.Values) {
            string id = inventory.heroId;
            if (inventory.tier >= 3) {
                NewAlertManager.Instance.CheckRemovable(NewAlertManager.ButtonName.DICTIONARY, inventory.heroId, true);
                if (inventory.camp == "human") {
                    if (cardPackage.checkHumanHero.Contains(id))
                        cardPackage.checkHumanHero.Remove(id);
                }
                else {
                    if (cardPackage.checkOrcHero.Contains(id))
                        cardPackage.checkOrcHero.Remove(id);
                }
                continue;
            }
            if (inventory.piece >= inventory.nextTier.piece) {
                NewAlertManager.Instance.SetUpButtonToUnlockCondition(NewAlertManager.ButtonName.DICTIONARY, id, true);
                NewAlertManager.Instance.SetUpButtonToAlert(NewAlertManager.Instance.referenceToInit[NewAlertManager.ButtonName.DICTIONARY], NewAlertManager.ButtonName.DICTIONARY);
                if (inventory.camp == "human") {
                    if (!cardPackage.checkHumanHero.Contains(id))
                        cardPackage.checkHumanHero.Add(id);
                }
                else {
                    if (!cardPackage.checkOrcHero.Contains(id))
                        cardPackage.checkOrcHero.Add(id);
                }
            }
            else {
                NewAlertManager.Instance.CheckRemovable(NewAlertManager.ButtonName.DICTIONARY, inventory.heroId, true);
                if (inventory.camp == "human") {
                    if (cardPackage.checkHumanHero.Contains(id))
                        cardPackage.checkHumanHero.Remove(id);
                }
                else {
                    if (cardPackage.checkOrcHero.Contains(id))
                        cardPackage.checkOrcHero.Remove(id);
                }
            }
        }
        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_HERO_REFRESDHED,
                            null,
                            questDatas
                        );
    }

    public void SetCardData() {
        foreach (CardInventory card in myCards) {
            if (!cardPackage.data.ContainsKey(card.cardId)) {
                CardData data = new CardData();
                data.cardId = card.cardId;
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
    /// ????????????, ???????????? ?????? ?????? ????????? ?????? ?????????
    /// </summary>
    public class UserInfo {
        public uint exp;
        public uint nextLvExp;
        public uint lvExp;
        public uint lv;

        public int _exp;
        public int _supply;
        public int maxDeckCount;
        public int? id;
        public string suid;
        public string serverId;

        public int gold;
        public int _goldPaid;
        public int _goldFree;
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
/// SignIn / SignUp ?????? ??????
/// </summary>
public partial class AccountManager {
    Timer UserReqTimer;
    const int MAX_PRE_SUPPLY = 200;
    /// <summary>
    /// ?????? ?????? ??????
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="retryOccured"></param>
    public void RequestUserInfo() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/");
        
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                var sceneStartController = GetComponent<SceneStartController>();
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    SetSignInData(res);

                    if (userData.preSupply >= MAX_PRE_SUPPLY) {
                        Logger.Log("Pre Supply??? ??????????????????.");
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
                else {
                    OccurNetworkErrorModal("?????????", res.Message, "????????? ?????? ??????????????????.");
                }
            },
            "?????? ????????? ???????????????...");
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

        networkManager.Request(request, callback, "?????? ????????? ???????????????...");
    }

    public void RequestUserStatistics() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/statistics");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {                
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    userStatistics = dataModules.JsonReader.Read<UserStatistics>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_STATISTICS,
                            null,
                            res
                        );
                }
                else {
                    Logger.LogWarning("?????? ?????? ?????? ?????? ?????? : " + res.Message.ToString());
                }
            },
            "?????? ?????? ???????????????...");
    }



    public void OnSignInResultModal() {
        StartCoroutine(ProceedSignInResult());
    }

    IEnumerator ProceedSignInResult() {
        Destroy(loadingModal);
        AdsManager.Instance.Init();

        if (PlayerPrefs.GetInt("isFirst", -1) == 1) {
            RequestLocaleSetting(true);
        }
        
        yield return new WaitForSeconds(1.0f);
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void RequestLocaleSetting(bool isFirst, string lang = null, OnRequestFinishedDelegate callback = null) {
        //POST /api/user/set_locale/:locale
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/set_locale/");

        if (isFirst) {
            url
                .Append(Application.systemLanguage.ToString().ToLower());
        }
        else {
            url
                .Append(lang.ToLower());
        }
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        if (callback == null) {
            networkManager.Request(
                request,
                (req, res) => {
                    if (res.IsSuccess) {
                        Logger.Log("!!");
                    }
                    else {
                        Logger.LogWarning("?????? ?????? ?????? ?????? ?????? ?????? ?????? : " + res.Message.ToString());
                    }
                },
                "?????? ?????? ??????..."
            );
        }
        else {
            networkManager.Request(
                request,
                callback,
                "?????? ?????? ??????..."
            );
        }
    }
    
    private void CallbackSignUp(HTTPRequest originalRequest, HTTPResponse response) {
        if (response != null && response.IsSuccess) {
            Logger.Log("???????????? ?????? ??????");
            RequestUserInfo();

            Modal.instantiate("??????????????? ?????????????????????.", Modal.Type.CHECK, () => {
                FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
            });
        }
        else {
            if (response.DataAsText.Contains("already exist")) {
                Modal.instantiate("?????? ?????? ????????? ID??? ???????????????.", Modal.Type.CHECK);
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

    #region supply ?????? ?????? ?????? code
    public float GetRemainSupplySec() {
        //TODO : ReqUserInfo??? ?????? ??? ????????? return ?????????
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
                Logger.LogWarning("??? ??? ???????????? ??????");
            }
        }, "??? ?????? ???????????????...");
    }
    /// <summary>
    /// ??? ?????? ?????? Server??? ??????
    /// </summary>
    public void RequestDeckMake(NetworkManager.AddCustomDeckReqFormat format) {
        if (string.IsNullOrEmpty(format.heroId)) {
            Logger.Log("?????? ????????? ?????? HeroId??? ????????? ?????????");
            return;
        }
        if (string.IsNullOrEmpty(format.name)) {
            Logger.Log("?????? ????????? ?????? ????????? ????????? ?????????");
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
                    Fbl_Translator translator = GetComponent<Fbl_Translator>();
                    string errorMsg = string.Empty;//.Contains("curse") ? translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_unablename") : null;
                    if (res.DataAsText.Contains("false")) {
                        errorMsg = "???????????? ?????? ??????????????????";//translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_unablename");
                    }                    
                    if (!string.IsNullOrEmpty(errorMsg)) Modal.instantiate(errorMsg, Modal.Type.CHECK);
                    Logger.LogWarning("??? ?????? ?????? ??????");
                }
            },
            "????????? ?????? ???????????????...");
    }

    /// <summary>
    /// ?????? ????????? ??????
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
                            new object[] { res, heroId }
                        );
                }
            }
            else {
                Logger.LogWarning("?????? ????????? ??????");
            }
        }, "?????? ????????? ?????????...");
    }

    /// <summary>
    /// ?????? ?????? ??????
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
                Logger.LogWarning("?????? ?????? ??????");
            }
        }, "????????? ?????? ???????????????...");
    }

    /// <summary>
    /// ?????? ?????? ??????
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
                Logger.LogWarning("?????? ?????? ??????");
            }
        }, "????????? ???????????? ???...");
    }


    /// <summary>
    /// ??? ?????? ??????
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
                Logger.LogWarning("??? ?????? ??????");
            }
        }, "??? ????????? ???????????? ???...");
    }

    /// <summary>
    /// ??? ?????? ??????
    /// </summary>
    /// <param name="data">?????? ??????</param>
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
                Fbl_Translator translator= GetComponent<Fbl_Translator>();
                string errorMsg = string.Empty;//.Contains("curse") ? translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_unablename") : null;
                if (res.DataAsText.Contains("false")) {
                    errorMsg = "???????????? ?????? ??????????????????";//translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_unablename");
                }
                if(!string.IsNullOrEmpty(errorMsg)) Modal.instantiate(errorMsg, Modal.Type.CHECK);
                Logger.LogWarning("??? ?????? ??????");
            }
        }, "??? ?????? ????????? ???????????????...");
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
                    var result = dataModules.JsonReader.Read<List<Templates>>(res.DataAsText);
                    humanTemplates = result;

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
                Logger.LogWarning("????????? ?????? ???????????? ??????");
            }
        }, "Human ???????????? ???????????????...");
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
                    var result = dataModules.JsonReader.Read<List<Templates>>(res.DataAsText);
                    orcTemplates = result;

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
                Logger.LogWarning("????????? ?????? ???????????? ??????");
            }
        }, "orc ???????????? ???????????????...");
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

                    foreach(CardInventory cardInventory in result.cardInventories) {
                        CardInventory inventory = new CardInventory();
                        string cardId = cardInventory.cardId;
                        var selectedCardInfo = allCards.Find(x => x.id == cardId);
                        if(selectedCardInfo != null) {
                            cardInventory.PasteData(selectedCardInfo);
                        }
                    }

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
                Logger.LogWarning("???????????? ?????? ???????????? ??????");
            }
        }, "???????????? ????????? ???????????? ???...");
    }

    public void RequestMainAdReward(IronSourcePlacement placement, UnityAction callback = null) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append(string.Format("api/user/claim_reward?kind=ad&placementName={0}&rewardName={1}&rewardAmount={2}", 
            placement.getPlacementName(), placement.getRewardName(), placement.getRewardAmount()));

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    if(placement.getPlacementName() == "shop") {
                        var result = dataModules.JsonReader.Read<AdRewardRequestResult>(res.DataAsText);
                        adRewardResult = result;
                        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_SHOP,
                            null,
                            res
                        );
                    }
                    else if(placement.getPlacementName() == "main") {
                        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_MAIN,
                            null,
                            res
                        );
                    }
                    else if (placement.getPlacementName() == "chest") {
                        var result = dataModules.JsonReader.Read<RewardClass[]>(res.DataAsText);
                        boxAdReward = result[0];
                        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST,
                            null,
                            res
                        );
                    }
                    else if (placement.getPlacementName() == "shop_chest") {
                        var result = dataModules.JsonReader.Read<AdRewardRequestResult>(res.DataAsText);
                        adRewardResult = result;
                        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ADREWARD_CHEST_ONLY,
                            null,
                            res
                        );
                    }
                }
            }
            else {
                Logger.LogWarning("?????? ?????? ?????? ??????" + res.Message.ToString());
            }
            callback?.Invoke();
        }, "?????? ?????? ???????????? ???...");
    }


    public void RequestShopAds() {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/shop/ad_rewards");

        HTTPRequest request = new HTTPRequest(new Uri(sb.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<AdReward[]>(res.DataAsText);
                    shopAdsList = result;

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_AD_SHOPLIST,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("?????? ?????? ?????? ???????????? ??????");
            }
        }, "?????? ?????? ????????? ???????????? ???...");
    }

    public RefreshRemain adBoxRefreshRemain;

    public void RequestAdBoxTime() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/shop/ad_reward_chest");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    adBoxRefreshRemain = dataModules.JsonReader.Read<RefreshRemain>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_AD_BOX_TIMEREMAIN,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("?????? ???????????? ?????? ???????????? ??????");
            }
        }, "?????? ???????????? ????????? ???????????????...");
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
                Logger.LogWarning("?????? ???????????? ??????");
            }
        }, "????????? ???????????????...");
    }

    public void RequestBuyItem(string itemId, Haegin.PurchasedInfo purchasedInfo = null) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/shop/buy");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        JObject json = new JObject();
        json["id"] = new JValue(itemId);
        if(purchasedInfo != null) {
            JObject transaction = new JObject();

            #if UNITY_EDITOR
            transaction.Add("store", new JValue("unity"));
            #elif UNITY_IOS
            transaction.Add("store", new JValue("apple"));
            #elif UNITY_ANDROID
            transaction.Add("store", new JValue("google"));
            #endif
            #if !UNITY_EDITOR
            transaction.Add("productId", new JValue(purchasedInfo.ProductId));
            transaction.Add("transactionId", new JValue(purchasedInfo.TransactionId));
            #endif
            json["transaction"] = transaction;
        }

        request.RawData = Encoding.UTF8.GetBytes(json.ToString());

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    buyBoxInfo = dataModules.JsonReader.Read<BuyBoxInfo>(res.DataAsText);
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
                OccurNetworkErrorModal("?????? ?????? ??????", res.Message);
                Logger.LogWarning("?????? ?????? ??????");
            }
        }, "?????? ?????? ???...");
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
                    mailNum = result.Count;

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
                Logger.LogWarning("????????? ???????????? ??????");
            }
        }, "????????? ???????????????...");
    }


    public void RequestMailBoxNum() {
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
                    mailNum = result.Count;

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_UPDATE_SOFT,
                            null,
                            res
                        );
                }
            }
            else {
                Logger.LogWarning("????????? ???????????? ??????");
            }
        }, "????????? ???????????????...");
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
                Logger.LogWarning("?????? ?????? ??????");
            }
        }, "?????? ???????????????...");
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
        
        int __mailId = -1;
        int.TryParse(mailId, out __mailId);

        var selectedItem = mailList.Find(x => x.id == __mailId);
        if(__mailId != -1) mailList.Remove(selectedItem);
        else Logger.LogWarning(mailId + "??? AccountManager??? MailList?????? ?????? ??? ????????????");
        
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
                Logger.LogWarning("?????? ?????? ??????");
            }
        }, "?????? ?????? ???...");
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
        //formatData.heroId = "h10001";   //?????? ?????? ?????????
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
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());

        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/cards");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );

        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = dataModules.JsonReader.Read<List<CollectionCard>>(res.DataAsText);
                    allCards = result;
                    allCardsDic = allCards.ToDictionary(x => x.id, x => x);

                    var nullCardClassesCards = allCards.FindAll(x => x.cardClasses == null);
                    foreach (var variable in nullCardClassesCards) {
                        Logger.LogWarning($"{variable.id}??? ?????? ???????????? ??????????????????.");
                    }
                    
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
                Logger.LogWarning("?????? ?????? ?????? ???????????? ??????");
            }
        }, "?????? ?????? ????????? ???????????????...");
    }

    public void LoadAllHeroes() {
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());

        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/heroes");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );

        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        
        networkManager.Request(request, OnReceivedLoadAllHeroes, "?????? ?????? ????????? ???????????????...");
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
                Logger.LogWarning("?????? ?????? ???????????? ??????");
            }
        }, "?????? ????????? ???????????????...");

    }

    public void RequestScenarioReward(int scenarioNum) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url.Append(base_url)
            .Append("api/user/claim_reward/")
            .Append("?kind=tutorialBox&boxNum=")
            .Append(scenarioNum.ToString());

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    string temp = res.DataAsText;

                    if (temp.Contains("claimed") || temp.Contains("error")) {
                        Logger.Log("????????????!");
                        PlayMangement.instance.resultManager.rewards = null;
                        return;
                    }


                    var result = dataModules.JsonReader.Read<RewardClass[]>(res.DataAsText);
                    rewardList = result;
                    

                    SetRewardInfo(result);
                    RequestUserInfo();
                    PlayMangement.instance.resultManager.rewards = result;    
                }
            }
            else {
                Logger.LogWarning("?????? ?????? ???????????? ??????");
            }
        }, "?????? ????????? ???????????????...");
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
    
    public void ChangeNicknameReq(string newNickName, bool ticketHave) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/change_nickname");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);
        NickNamechangeFormat format = new NickNamechangeFormat();
        format.nickName = newNickName;
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(format));
        Fbl_Translator translator= GetComponent<Fbl_Translator>();
        
        //????????? ??????
        networkManager.Request(request, (req, res) => {
            if (res.IsSuccess) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var errorFormat = JsonReader.Read<ErrorFormat>(res.DataAsText);
                    string text = String.Empty;
                
                    if (errorFormat != null && !string.IsNullOrEmpty(errorFormat.error)) {
                        switch (errorFormat.error) {
                            case "blank_only":
                            case "blank_exist":
                            case "emoji":
                            case "special":
                                text = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_unablename");
                                break;
                            case "curse":
                                text = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_unableword");
                                break;
                            default:
                                Logger.Log("Error -142 : " + errorFormat.error);
                                text = "Error : " + errorFormat.error;
                                break;
                        }
                        Modal.instantiate(text, Modal.Type.CHECK, () => { });
                        return;
                    }
                    
                    string header = translator.GetLocalizedText("UIPopup", "ui_popup_myinfo_questnamechange");
                    header = header.Replace("{a}", newNickName);
                    
                    Modal.instantiate(header, Modal.Type.YESNO, () => {
                        //?????? ?????? ??????
                        HTTPRequest finalReq = new HTTPRequest(
                            new Uri(url.ToString())
                        );
                    
                        finalReq.MethodType = HTTPMethods.Post;
                        finalReq.AddHeader("authorization", TokenFormat);
                        format = new NickNamechangeFormat();
                        format.nickName = newNickName;
                        finalReq.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(format));
                    
                        networkManager.Request(finalReq, (_req, _res) => {
                            if (_res.IsSuccess) {
                                if (_res.StatusCode == 200 || _res.StatusCode == 304) {
                                    NoneIngameSceneEventHandler
                                        .Instance
                                        .PostNotification(
                                            NoneIngameSceneEventHandler.EVENT_TYPE.API_NICKNAME_UPDATED,
                                            null,
                                            res
                                        );

                                    string header1 = translator.GetLocalizedText("UIPopup",
                                        "ui_popup_myinfo_namechangecompl");
                                    header1 = header1.Replace("{a}", newNickName);
                                    
                                    Modal.instantiate(header1, Modal.Type.CHECK, () => { });
                                    
                                    RequestUserInfo();
                                }
                                else {
                                    Modal.instantiate("Error -141", Modal.Type.CHECK, () => { });
                                }
                            }
                        });
                    });
                }
            }
            else {
                Modal.instantiate("Error -142", Modal.Type.CHECK, () => { });
            }
        }, "Request Nickname Change...");
    }

    public class ErrorFormat {
        public string result;
        public string error;
    }

    public class NickNamechangeFormat {
        public string nickName;
    }
}

public partial class AccountManager {
    /// <summary>
    /// ???????????? ?????? ??????
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
        networkManager.Request(request, callback, "?????? ????????? ???????????? ???...");
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
                Logger.LogWarning("???????????? ???????????? ?????? ???????????? ??????");
            }
        }, "???????????? ????????? ???????????? ???...");
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
                        Logger.Log("?????? ?????? ?????????");
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
            "?????? ?????? ?????????..."
            );
    }

    public void RequestTutorialPreSettings() {
        RequestUserInfo((req, res) => {
            if (res.IsSuccess) {
                SetSignInData(res);
                
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
                        Logger.LogWarning("?????? ????????? ???????????? ?????? ?????? ??????");
                    }
                });
            }
            else {
                Logger.LogWarning("?????? ????????? ???????????? ?????? ?????? ??????");
            }
        });
    }
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
            
            leagueInfo.id = originData.id;
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

            leagueInfo.rankDetail = new RankDetail(originData.rankDetail);

            return leagueInfo;
        }
    }

    [Serializable]
    public class RankDetail {
        public RankUpCondition rankUpBattleCount;
        public RankUpCondition rankDownBattleCount;
        public int id;
        [SerializeField] private string _majorRankName;
        public string majorRankName{
            set { _majorRankName = value;}
            get { return AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("Tier", _majorRankName);}
        }
        [SerializeField] private string _minorRankName;
        public string minorRankName {
            set { _minorRankName = value;}
            get { return AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("Tier", _minorRankName);}
        }
        public int pointOverThen;
        public int pointLessThen;

        public RankDetail(RankDetail copy) {
            id = copy.id;
            _majorRankName = copy._majorRankName;
            _minorRankName = copy._minorRankName;
            pointOverThen = copy.pointOverThen;
            pointLessThen = copy.pointLessThen;
            
            if(copy.rankDownBattleCount != null) {
                rankDownBattleCount = new RankUpCondition(copy.rankDownBattleCount.needTo, copy.rankDownBattleCount.battles);
            }

            if(copy.rankUpBattleCount != null) {
                rankUpBattleCount = new RankUpCondition(copy.rankUpBattleCount.needTo, copy.rankUpBattleCount.battles);
            }
        }

        public RankDetail(){}
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
    /// ???????????? ?????? ?????? ??????
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

        var prevLeagueId = PlayerPrefs.GetInt("PrevLeagueId");
        networkManager.Request(
            request, (req, res) => {
                var sceneStartController = GetComponent<SceneStartController>();
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var leagueInfo = dataModules.JsonReader.Read<LeagueInfo>(res.DataAsText);

                    if (prevSceneName == "Login") {
                        Logger.Log("?????? ?????? Ingame??? ?????? ??????");
                        scriptable_leagueData.leagueInfo = leagueInfo;
                        scriptable_leagueData.prevLeagueInfo = leagueInfo.DeepCopy(leagueInfo);

                        if (prevLeagueId != leagueInfo.id) {
                            Logger.Log("<color=yellow>?????? ????????????!!!</color>");
                            ReuqestLeagueEndReward();
                        }
                    }
                    else { 
                        //????????? ?????????
                        if (prevLeagueId != leagueInfo.id) {
                            Logger.Log("<color=yellow>?????? ????????????!!!</color>");
                            ReuqestLeagueEndReward();
                        }
                        
                        scriptable_leagueData.leagueInfo = leagueInfo;
                    }

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_INFO_UPDATED,
                            null,
                            leagueInfo
                        );
                    if (leagueInfo.id != null) PlayerPrefs.SetInt("PrevLeagueId", leagueInfo.id.Value);
                }
            },
            "?????? ????????? ???????????????...");
    }

    public void RequestLeagueReward(OnRequestFinishedDelegate callback, int rewardId) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/claim_reward")
            .Append("?kind=rating&rewardId=")
            .Append(rewardId.ToString());
        
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                callback(req, res);

                RequestLeagueInfo();
            },
            "?????? ?????? ??????...");
    }
    private Timer leagueEndTimer = null;
    
    public void ReuqestLeagueEndReward() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/claim_reward?kind=leagueEnd");
        
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        
        networkManager.Request(
            request, (req, res) => {
                if (res.IsSuccess) {
                    if (res.DataAsText.Contains("already get")) {
                        Logger.Log("<color=yellow>already get reward</color>");
                    }
                    else if (res.DataAsText.Contains("no before")) {
                        Logger.Log("<color=yellow>have no before league</color>");
                    }
                    else {
                        var resFormat = JsonReader.Read<ClaimRewardResFormat>(res.DataAsText);
                        //?????? ????????? ?????? ??????
                        if (MainWindowModalEffectManager.Instance == null) {
                            PlayerPrefs.SetString("SoftResetData", resFormat.ToString());
                        }
                        //?????? ????????? ??????
                        else {
                            NoneIngameSceneEventHandler
                                .Instance
                                .PostNotification(
                                    NoneIngameSceneEventHandler.EVENT_TYPE.API_LEAGUE_CHANGED,
                                    null,
                                    resFormat
                                );
                        }
                    }
                }
            }, "");
    }

    public class ClaimRewardResFormat {
        public LeagueInfo leagueInfoCurrent;
        public LeagueInfo leagueInfoBefore;
        public List<ClaimRewardResFormatReward> rewards;
    }

    public class ClaimRewardResFormatReward {
        public string kind;
        public int amount;
    }

    private Timer dayChangedTimer = null;
    public void SetDayChangedTimer() {
        dayChangedTimer?.Cancel();
        
        var utcNow = DateTime.UtcNow;
        DateTime tommorow = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day + 1, 0, 0, 0);
        TimeSpan diff = tommorow - utcNow;
        var totalSeconds = diff.TotalSeconds + 1;
        
        dayChangedTimer = Timer.Register((float)totalSeconds, () => {
            var stateHandler = MainSceneStateHandler.Instance;
            if(stateHandler != null) stateHandler.ChangeState("DailyQuestLoaded", false);
            
            //?????? ??????????????? ??????
            if (MenuSceneController.menuSceneController != null) {
                GetDailyQuest((req, res) => {
                    MenuSceneController.menuSceneController.CheckDailyQuest();
                });
                ReuqestLeagueEndReward();
            }
            SetDayChangedTimer();
        });
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
            "?????? ???????????? ???????????????...");
    }

    public void RequestBattleRank() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/statistics/battle_rank");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    battleRank = dataModules.JsonReader.Read<BattleRank>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_BATTLERANK_RECEIVED,
                            null,
                            rankTable
                        );
                }
            },
            "?????? ???????????? ???????????????...");
    }

    public void RequestRankTable(OnRequestFinishedDelegate callback) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/info/rank_table");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "?????? ???????????? ???????????????...");
    }

    public void RequestTotalRank() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/statistics/total_rank");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    totalRank = dataModules.JsonReader.Read<TotalRank>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_TOTALRANK_RECEIVED,
                            null,
                            rankTable
                        );
                }
            },
            "?????? ???????????? ???????????????...");
    }


    public void RequestAttendance() {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/attendance");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    attendanceResult = dataModules.JsonReader.Read<AttendanceResult>(res.DataAsText);
                    if (!attendanceResult.attendChk) {
                        NoneIngameSceneEventHandler
                            .Instance
                            .PostNotification(
                                NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_ALREADY,
                                null,
                                res
                            );
                    }
                    else {
                        NoneIngameSceneEventHandler
                            .Instance
                            .PostNotification(
                                NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_SUCCESS,
                                null,
                                res
                            );
                    }
                }
            },
            "???????????? ???????????????...");
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
    /// ?????? ??????/?????? ?????? ??????
    /// </summary>
    public void RequestTutorialUnlockInfos(bool isInitRequest = false) {
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
                    var newQuestInfos = dataModules.JsonReader.Read<List<QuestInfo>>(res.DataAsText);
                    var prevQuestInfo = questInfos;
                    List<string> unlockList = new List<string>();
                    List<string> lockList = new List<string>();

                    foreach (QuestInfo questInfo in newQuestInfos) {
                        QuestUnlockInfo innerData = questInfo.after.Find(x => x.method == "unlock_menu");
                        if (innerData != null) {
                            if (questInfo.cleared) {
                                foreach (var arg in innerData.args) {
                                    unlockList.Add(arg);
                                }
                            }
                            else {
                                foreach (var arg in innerData.args) {
                                    lockList.Add(arg);
                                }
                            }

                        }
                    }

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_TUTORIAL_INFOS_UPDATED,
                            null,
                            new object[] { isInitRequest, unlockList, lockList }
                        );

                    questInfos = newQuestInfos;
                }
            },
            "???????????? ????????? ???????????????...");
    }

    /// <summary>
    /// ???????????? ?????? ?????? ??????
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
                        RequestQuestInfo();
                    }
                }
            }, 
            "???????????? ?????? ?????????..."
            );
    }

    public void RequestUnlockInTutorial(int id, OnRequestFinishedDelegate callback) {
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

        networkManager.Request(request, callback, "?????? ????????? Clear ??????");
    }

    public void RequestQuestClearReward(int id, GameObject obj) {
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());
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
                        //RequestMailBoxNum();

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
            "???????????? ?????? ?????????..."
            );
    }
}

public partial class AccountManager {
    public List<QuestData> questDatas;
    public QuestData rerolledQuest;
    public RefreshRemain refreshTime;
    public GameObject refreshObj;

    public List<AchievementData> achievementDatas;
    public AchievementData updatedAchievement;

    public void RequestQuestInfo(OnRequestFinishedDelegate callback = null) {
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/quest/");

        Logger.Log("Request Quest Info");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        if (callback == null) {
            networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    questDatas = dataModules.JsonReader.Read<List<QuestData>>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED,
                            null,
                            questDatas
                        );
                }
            },
            "???????????? ????????? ???????????????...");
        }
        else {
            networkManager.Request(request, callback, "????????? ????????? ???????????? ???...");
        }
    }

    public void GetDailyQuest(OnRequestFinishedDelegate callback) {
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/quest/get_daily_quest");

        Logger.Log("Request Quest Info");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "?????? ????????? ????????? ???????????????...");
    }

    public void RequestQuestRefreshTime(OnRequestFinishedDelegate callback = null) {
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/quest/refresh");

        Logger.Log("Request Quest Refresh Time");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        if (callback == null) {
            networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    refreshTime = dataModules.JsonReader.Read<RefreshRemain>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REFRESH_TIME_UPDATED,
                            null,
                            questDatas
                        );
                }
            },
            "????????? ?????? ?????? ???????????? ???...");
        }
        else {
            networkManager.Request(request, callback, "????????? ?????? ?????? ???????????? ???...");
        }
    }

    public void RequestQuestRefresh(int id, GameObject obj) {
        refreshObj = obj;
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/quest/refresh/" + id);

        Logger.Log("RequestRefreshQuest");

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));

        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request,
            (req, res) => {
                Logger.Log(res.DataAsText);
                if (res.IsSuccess) {
                    if (res.StatusCode == 200 || res.StatusCode == 304) {
                        rerolledQuest = dataModules.JsonReader.Read<QuestData>(res.DataAsText);

                        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REFRESHED,
                            null,
                            res
                        );
                    }
                }
            },
            "????????? ?????????..."
            );
    }


    public void RequestAchievementInfo(OnRequestFinishedDelegate callback = null) {
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/achievement");

        Logger.Log("Request Quest Info");
        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        if (callback == null) {
            networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    achievementDatas = dataModules.JsonReader.Read<List<AchievementData>>(res.DataAsText);

                    NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ACHIEVEMENT_UPDATED,
                            null,
                            achievementDatas
                        );
                }
            },
            "?????? ????????? ???????????????...");
        }
        else {
            networkManager.Request(request, callback, "?????? ????????? ???????????? ???...");
        }
    }

    public void RequestAchievementClearReward(string id) {
        var language = PlayerPrefs.GetString("Language", GetLanguageSetting());
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/achievement/get_reward");

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
                        //RequestMailBoxNum();
                        updatedAchievement = dataModules.JsonReader.Read<AchievementData>(res.DataAsText);

                        NoneIngameSceneEventHandler
                        .Instance
                        .PostNotification(
                            NoneIngameSceneEventHandler.EVENT_TYPE.API_ACHIEVEMENT_REWARD_RECEIVED,
                            null
                        );
                    }
                }
            },
            "???????????? ?????? ?????????..."
            );
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
        PlayerPrefs.SetInt("isFirst", 0);

        if (stageNumber == 1) {
            if(camp == "human") {
                RequestIngameTutorialReward((req, res) => {
                    Modal.instantiate("?????? ???????????? ??????", Modal.Type.CHECK);
                }, camp);
            }
            else {
                RequestIngameTutorialReward((req, res) => {
                    Modal.instantiate("?????? ???????????? ??????", Modal.Type.CHECK);
                }, camp);
            }
        }

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    Modal.instantiate("???????????? " + camp + ", " + stageNumber + " ?????? ??????", Modal.Type.CHECK);
                }
                else {
                    Modal.instantiate("???????????? skip ?????? ??????", Modal.Type.CHECK);
                    Logger.LogError(res.DataAsText);
                }
            },
            "???????????? ?????? ?????????...");
    }

    public void RequestPickServer(OnRequestFinishedDelegate callback) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("lobby/pick_server");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        
        networkManager.Request(request, callback, "pick server...");
    }

    /// <summary>
    /// ????????? Progress ??????
    /// </summary>
    /// <param name="qid">quest id</param>
    /// <param name="progress">????????? ??????</param>
    /// <param name="callback">callback</param>
    public void RequestChangeQuestProgress(int qid, int progress, OnRequestFinishedDelegate callback) {
        StringBuilder sb = new StringBuilder();
        sb
            .Append(networkManager.baseUrl)
            .Append("api/test_helper/quest/progress");

        HTTPRequest request = new HTTPRequest(
            new Uri(sb.ToString())
        );
        request.MethodType = BestHTTP.HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        NetworkManager.ChangeProgressReqFormat format = new NetworkManager.ChangeProgressReqFormat();
        format.qid = qid;
        format.progress = progress;
        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(format));

        networkManager.Request(request, callback, "????????? Progress ?????? ?????????...");
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
                    Modal.instantiate("MMR ?????? ??????", Modal.Type.CHECK);
                }
                else {
                    Modal.instantiate("MMR ?????? ?????? ??????", Modal.Type.CHECK);
                    Logger.LogError(res.DataAsText);
                }
                
            },
            "MMR ?????? ?????????...");
    }

    public void RequestChangeMMRForTest(int mmr, int rankId) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        if (mmr < 0) mmr = 0;
        else if (mmr > 3000) mmr = 3000;

        url
            .Append(base_url)
            .Append("api/test_helper/league_info/")
            .Append(mmr)
            .Append("/")
            .Append(rankId);

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(
            request, (req, res) => {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    Modal.instantiate("MMR ?????? ??????", Modal.Type.CHECK);
                }
                else {
                    Modal.instantiate("MMR ?????? ?????? ??????", Modal.Type.CHECK);
                    Logger.LogError(res.DataAsText);
                }

            },
            "MMR ?????? ?????????...");
    }

    public void RequestThreeWinReward(OnRequestFinishedDelegate callback) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/user/claim_reward?kind=leagueWin");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "3??? ?????? ?????????");
    }

    public void RequestAllCardCheat(OnRequestFinishedDelegate callback) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/test_helper/give_all_card");

        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);

        networkManager.Request(request, callback, "?????? ?????? ?????????");
    }

    public void OccurNetworkErrorModal(string requestType, string originalMessage, string additionalMessage = null) {
        StringBuilder sb = new StringBuilder();
        sb.Append(requestType);
        sb.Append(" ?????? ?????? ???????????? ?????? ??????\n");
        sb.Append($"[{originalMessage}]");
        
        if (!string.IsNullOrEmpty(additionalMessage)) {
            sb.Append("\n" + additionalMessage);
        }
        Modal.instantiate(sb.ToString(), Modal.Type.CHECK);
    }

    public void RequestCheatExp(OnRequestFinishedDelegate callback, int exp) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/test_helper/user_give_exp");
        
        HTTPRequest request = new HTTPRequest(
            new Uri(url.ToString())
        );
        request.MethodType = HTTPMethods.Post;
        request.AddHeader("authorization", TokenFormat);
        request.RawData = Encoding.UTF8.GetBytes(string.Format("{{\"exp\":{0}}}", exp));
        
        networkManager.Request(request, callback, "?????? ????????? ?????? ??????");
    }
}
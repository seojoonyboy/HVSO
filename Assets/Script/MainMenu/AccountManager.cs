using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

public class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }

    public delegate void Callback();
    private Callback callback = null;

    public string DEVICEID { get; private set; }
    public UserClassInput userData { get; private set; }
    public List<dataModules.CardInventory> myCards { get; private set; }
    public List<Deck> myDecks = new List<Deck>();
    public CardDataPackage cardPackage;

    NetworkManager networkManager;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        DEVICEID = SystemInfo.deviceUniqueIdentifier;
        cardPackage = Resources.Load("CardDatas/CardDataPackage_01") as CardDataPackage;
    }

    // Start is called before the first frame update
    void Start() {
        networkManager = NetworkManager.Instance;
    }

    // Update is called once per frame
    void Update() {

    }

    public void isUserExist() {
        RequestUserInfo();
    }

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

    private void RequestUserInfo(bool needPopup = true) {
        StringBuilder url = new StringBuilder();
        string base_url = networkManager.baseUrl;

        url
            .Append(base_url)
            .Append("api/users/")
            .Append(DEVICEID);

        Debug.Log(url.ToString());
        if (needPopup) {
            networkManager.request("GET", url.ToString(), CallbackUserRequestWithPopup, false);
        }
        else {
            networkManager.request("GET", url.ToString(), CallbackUserRequest, false);
        }
    }

    private void CallbackUserRequestWithPopup(HttpResponse response) {
        if (response.responseCode != 200) {
            if (!response.request.isNetworkError) {
                transform
                .GetChild(0)
                .GetComponent<LoginController>()
                .OnSignUpModal();
            }
            else {
                OccurErrorModal(response.responseCode);
            }
        }
        else {
            transform
                .GetChild(0)
                .GetComponent<LoginController>()
                .OnSignInModal();

            userData = dataModules.JsonReader.Read<UserClassInput>(response.data);
        }
    }

    private void OccurErrorModal(long errorCode) {
        Modal.instantiate("네트워크 오류가 발생하였습니다. 다시 시도해 주세요.", Modal.Type.CHECK);
        NoneIngameSceneEventHandler.Instance.PostNotification(NoneIngameSceneEventHandler.EVENT_TYPE.NETWORK_EROR_OCCURED, this, errorCode);
    }

    private void CallbackUserRequest(HttpResponse response) {
        if (response.responseCode != 200) { }
        else {
            userData = dataModules.JsonReader.Read<UserClassInput>(response.data);
            myCards = userData.cardInventories;
        }
    }

    private void CallbackSignUp(HttpResponse response) {
        if (response.responseCode != 200) {
            Debug.Log(
                response.responseCode
                + "에러\n"
                + response.errorMessage);
        }
        else {
            userData = dataModules.JsonReader.Read<UserClassInput>(response.data);

            Modal.instantiate("회원가입이 완료되었습니다.", Modal.Type.CHECK, () => {
                SceneManager.Instance.LoadScene(SceneManager.Scene.MAIN_SCENE);
            });
        }
    }

    public void RequestMyCardInventory() {
        RequestUserInfo(false);
    }

    public class UserClassInput {
        public string nickName;
        public string deviceId;

        public List<dataModules.CardInventory> cardInventories;
    }

    public void SetDummyDeck() {
        if (myCards == null) return;

        var group =
            from card in myCards
            group card by card.camp into newGroup
            orderby newGroup.Key
            select newGroup;

        foreach(var _group in group) {
            Deck deck = new Deck();
            deck.type = _group.Key;

            foreach(var card in _group) {
                deck.cards.Add(card);
                cardPackage.data.Add(card.cardId, SetCardData(card));
                Debug.Log("");
            }
            myDecks.Add(deck);
        }

        try {
            myDecks[0].heroName = "수비대장 제로드";
            myDecks[1].heroName = "족장 크라쿠스";
        }
        catch(ArgumentException ex) {
            Debug.LogError("사용자 덱이 정상적으로 세팅되지 않았습니다.");
        }
    }

    public CardData SetCardData(dataModules.CardInventory card) {
        CardData data = new CardData();
        data.rarelity = card.rarelity;
        data.type = card.type;
        data.class_1 = card.cardClasses[0];
        if(card.cardClasses.Length == 2)
            data.class_2 = card.cardClasses[1];
        data.category_1 = card.cardCategories[0];
        if (card.cardCategories.Length == 2)
            data.category_2 = card.cardCategories[1];
        data.name = card.name;
        data.cost = card.cost;
        data.attack = card.attack;
        data.hp = card.hp;
        data.hero_chk = card.isHeroCard;

        return data;
}

    public class Deck {
        public string heroName;
        public string type;
        public List<dataModules.CardInventory> cards = new List<dataModules.CardInventory>();
    }
}

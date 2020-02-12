using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using dataModules;
using System;
using System.Threading.Tasks;

namespace Quest {
    public class QuestManager : MonoBehaviour
    {
        public GameObject QuestCanvas;
        public Transform content;
        [SerializeField] HUDController HUDController;
        [SerializeField] GameObject newIcon;
        [SerializeField] GameObject glowEffect;
        [SerializeField] Transform header;
        public MenuSceneController tutoDialog;

        public GameObject handSpinePrefab;
        private List<QuestContentController> quests;
        private Tutorials[] tutorialJson;

        public void OpenQuestCanvas() {
            HUDController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
            HUDController.SetBackButton(OnBackBtnClicked);

            EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackBtnClicked);
            QuestCanvas.SetActive(true);
            SwitchPanel(0);
            //RemoveHandIcon();//퀘스트창 열때마다 손가락 있는지 없는지 확인하는 비효율적인 부분.
            //showNewIcon(false);
        }

        public void SwitchPanel(int page) {
            for (int i = 0; i < 3; i++) {
                if (i == 1) continue;
                QuestCanvas.transform.Find("InnerCanvas/MainPanel").GetChild(i).gameObject.SetActive(i == page);
                header.GetChild(i).GetComponent<Button>().interactable = !(i == page);
            }
        }

        void OnBackBtnClicked() {
            SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
            EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackBtnClicked);
            HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);
            QuestCanvas.SetActive(false);
        }

        private void Awake() {
            quests = new List<QuestContentController>();
            content.GetComponentsInChildren<QuestContentController>(true, quests);
            ReadFile();

            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, ShowQuest);
        }

        void Start() {
            AccountManager.Instance.RequestQuestInfo();
            //OpenQuestCanvas();
        }

        private void ReadFile() {
            string data = ((TextAsset)Resources.Load("TutorialDatas/questData")).text;
            tutorialJson = JsonReader.Read<Tutorials[]>(data);
        }

        private void OnDestroy() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, ShowQuest);
        }

        public void AddQuest(QuestData data) {
            QuestContentController quest = quests.Find(x=>!x.gameObject.activeSelf);

            
            quest.GetComponent<RectTransform>().sizeDelta = new Vector2(
                quest.fakeItem.GetComponent<RectTransform>().sizeDelta.x,
                quest.fakeItem.GetComponent<RectTransform>().sizeDelta.y
            );

            quest.GetComponent<Image>().enabled = true;
            foreach (Transform tf in quest.transform) {
                tf.gameObject.SetActive(true);
            }

            quest.data = data;
            quest.manager = this;
            quest.gameObject.SetActive(true);
            if(data.tutorials == null) return;
            quest.ActiveTutorial();
            if(data.cleared) return;
            showNewIcon(true);
        }

        public void showNewIcon(bool yesno) {
            newIcon.SetActive(yesno);
            glowEffect.SetActive(yesno);
        }

        public void ResetQuest() {
            quests.ForEach(x=>x.gameObject.SetActive(false));
        }

        private void ShowQuest(Enum type, Component Sender, object Param) {
            AccountManager accountManager = AccountManager.Instance;
            var questDatas = accountManager.questDatas;
            foreach(QuestData questData in questDatas) {
                AddQuest(questData);
            }
        }

        /// <summary>
        /// 0-2 튜토리얼 깼을 때 PlayerPrefs로 FirstTutorialClear의 여부에 따라 임의의 퀘스트 튜토리얼을 추가함 (1일때 퀘스트 시작, 2일때 퀘스트 클리어)
        /// </summary>
        public void TutorialNoQuestShow() {
            bool needStart = (PlayerPrefs.GetInt("FirstTutorialClear", 0) != 0);
            if(!needStart) return;
            QuestContentController noQuest = gameObject.AddComponent<QuestContentController>();
            noQuest.enabled = false;
            noQuest.data = new QuestData();
            noQuest.data.tutorials = tutorialJson[0].tutorials;
            noQuest.manager = this;
            noQuest.data.cleared = (PlayerPrefs.GetInt("FirstTutorialClear", 0) == 2);
            noQuest.ActiveTutorial();
        }
        /// <summary>
        /// 퀘스트 버튼에 손가락 표시
        /// </summary>
        public void ShowHandIcon() {
            Instantiate(handSpinePrefab, transform, false).name = "tutorialHand";
        }

        /// <summary>
        /// 퀘스트 버튼에 손가락 제거
        /// </summary>
        private void RemoveHandIcon() {
            Transform hand = transform.Find("tutorialHand");
            if(hand == null) return;
            Destroy(hand.gameObject);
        }

        public TutorialSerializeList tutorialSerializeList;
        /// <summary>
        /// 튜토리얼을 진행하기 위해 Inspector 창에 필요한 GameObject 또는 Component를 사용하기 위해 선언된 변수들
        /// </summary>
        [Serializable] public class TutorialSerializeList {
            public GameObject playButton;   //플레이 버튼 느낌표
            public GameObject HumanFlagIcon; //휴먼 스토리 깃발, 느낌표 추가를 위해 
            public GameObject OrcFlagIcon;  //오크 스토리 깃발, 느낌표 추가를 위해
            public GameObject newMail;  //메일함, 느낌표 추가를 위해
            public ScenarioManager scenarioManager; //시나리오매니저, 0-2 표기 손가락 표기를 위해
            public DeckSettingManager deckSettingManager; //덱세팅매니저, 덱세팅에 카드 넣고 빼는걸 관리하기 위해
            public Button BattleButton; //Mode의 배틀 버튼, 손가락 표시겸 눌렀을 때 대사 뜨기 위해
            public Button backButton; //백 버튼, 손가락 표시를 위해
            public Button mailAllGetButton; //메일 전부 받기 버튼, 원래는 버튼 클릭 막기 위해인데 사용을 안하네?
            public MailBoxManager mailBoxManager; //메일박스매니저, 메일들을 살펴봐 특정 메일을 관리하기 위해
            public MenuLockController menuLockController; //메뉴락컨트롤러, 잠금해제 할 때
            public HorizontalScrollSnap horizontalScrollSnap; //스크롤스냅(덱리스트부분), 덱리스트에 손가락 표시 하기 위해
            public GameObject newCardMenu;  //하단 메뉴 4번째 카드메뉴의 느낌표
            public Button ScrollDeckButton; //하단 메뉴 2번째 덱편집 버튼
            public GameObject newDeckMenu;  //하단 메뉴 2번째 덱편집의 느낌표
            public GameObject newBattleMenu;//Mode의 느낌표
            public Button modeSelect;   //Mode 버튼
            public GameObject modeGlow; //Mode의 반짝임 
        }

    }

    [Serializable] public class QuestData {
        public int id;
        public int progress;
        public QuestDetail questDetail;
        public bool cleared = false;
        public bool rewardGet = false;
        public TutorialShowList[] tutorials;
    }

    public class QuestDetail {
        public string id;
        public string type;
        public string name;
        public string desc;
        public int progMax;
        public Reward[] rewards;
    }

    [Serializable]
    public class Reward {
        public string kind;
        public int amount;
    }
    /// <summary>
    /// Resource의 questData json을 Deserialize화 받는 클래스
    /// </summary>
    public class Tutorials {
        public string id;
        public TutorialShowList[] tutorials;
    }
    /// <summary>
    /// 개별의 튜토리얼들의 데이터들
    /// </summary>
    public class TutorialShowList {
        public string method;   //튜토리얼 함수이름
        public string[] args;   //튜토리얼 매개변수
        public bool isShowing = false;  //이미 보여주고 있는 단계인지 중복 확인용
    }
}
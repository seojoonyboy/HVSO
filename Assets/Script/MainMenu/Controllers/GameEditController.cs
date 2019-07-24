using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using dataModules;
using UnityEngine.UI;
using Bolt;

/// <summary>
/// 테스트용 인게임을 위한 스크립트
/// </summary>
namespace IngameEditor {
    public class GameEditController : SerializedMonoBehaviour {
        [TableMatrix(SquareCells = true)] public GameObject[,] enemySlots = new GameObject[5, 2];
        [TableMatrix(SquareCells = true)] public GameObject[,] playerSlots = new GameObject[5, 2];

        List<CollectionCard> allCards = new List<CollectionCard>();
        List<CollectionCard> allUnitCards = new List<CollectionCard>();
        List<CollectionCard> humanUnitCards = new List<CollectionCard>();
        List<CollectionCard> orcUnitCards = new List<CollectionCard>();
        List<CollectionCard> humanCards = new List<CollectionCard>();
        List<CollectionCard> orcCards = new List<CollectionCard>();

        GameObject selectedSlot = null;
        GameObject selectedHandSlot = null;

        string selectedRace = "";

        [SerializeField] GameObject[] unitEditModals;
        [SerializeField] GameObject unitSelectModal;
        [SerializeField] GameObject handSelectModal;
        [SerializeField] Transform unitSelectModalContent;
        [SerializeField] Transform handSelectModalContent;
        [SerializeField] Transform handSelectPlayerGrid;
        [SerializeField] Transform handSelectEnemyGrid;

        [SerializeField] GameObject unitBox;
        [SerializeField] GameObject[] raceButtons;

        [SerializeField]
        Slider
            enemyHeroHpSlider,
            playerHeroHpSlider,
            enemyHeroShieldSlider,
            playerShieldSlider;

        public StartState startState;
        void Start() {
            allCards = AccountManager.Instance.allCards;
            allUnitCards = allCards.FindAll(x => x.type != "magic");

            humanUnitCards = allUnitCards.FindAll(x => x.camp == "human");
            orcUnitCards = allUnitCards.FindAll(x => x.camp == "orc");

            humanCards = allCards.FindAll(x => x.camp == "human");
            orcCards = allCards.FindAll(x => x.camp == "orc");

            startState = new StartState();
            
            Initialize();
            OnSelectRace(0);
        }

        public void Initialize() {
            GameObject slots = null;
            //TODO Slot 초기화
            switch (selectedRace) {
                default:
                case "human":
                    slots = unitEditModals[0];
                    break;
                case "orc":
                    slots = unitEditModals[1];
                    break;
            }

            var list = slots.GetComponentsInChildren<CardData>();
            foreach(CardData cardData in list) {
                cardData.Clear();
            }
        }

        public void OnSelectRace(int index) {
            GameObject selectedButton, unselectedButton;
            string selectedRace = null;
            switch (index) {
                default:
                case 0:
                    unitEditModals[0].SetActive(true);
                    unitEditModals[1].SetActive(false);

                    selectedButton = raceButtons[0];
                    unselectedButton = raceButtons[1];
                    selectedRace = "human";
                    break;

                case 1:
                    unitEditModals[1].SetActive(true);
                    unitEditModals[0].SetActive(false);

                    selectedButton = raceButtons[1];
                    unselectedButton = raceButtons[0];
                    selectedRace = "orc";
                    break;
            }

            selectedButton.transform.Find("Selected").gameObject.SetActive(true);
            selectedButton.transform.Find("Unselected").gameObject.SetActive(false);
            unselectedButton.transform.Find("Unselected").gameObject.SetActive(true);
            unselectedButton.transform.Find("Selected").gameObject.SetActive(false);
            this.selectedRace = selectedRace;
        }

        /// <summary>
        /// 슬롯 클릭시
        /// </summary>
        public void OnSlotClick(GameObject clickedSlot) {
            selectedSlot = clickedSlot;
            OnUnitSelectModal();
        }
        
        public void OnUnitSelectModal() {
            unitSelectModal.SetActive(true);
            MakeUnitList();
        }

        void MakeUnitList() {
            foreach(Transform tf in unitSelectModalContent) {
                Destroy(tf.gameObject);
            }

            List<CollectionCard> selectedPool = new List<CollectionCard>();
            string slotCamp = selectedSlot.transform.parent.parent.name;

            switch (selectedRace) {
                case "human":
                    if (slotCamp == "Player") {
                        selectedPool = humanUnitCards;
                    }
                    else {
                        selectedPool = orcUnitCards;
                    }
                    break;
                case "orc":
                    if (slotCamp == "Player") {
                        selectedPool = orcUnitCards;
                    }
                    else {
                        selectedPool = humanUnitCards;
                    }
                    break;
                default:
                    selectedPool = null;
                    break;
            }

            if (selectedPool == null) return;
            foreach(CollectionCard card in selectedPool) {
                GameObject unit = Instantiate(unitBox, unitSelectModalContent);
                unit.GetComponent<CardData>().data = card;
                unit.SetActive(true);
                unit.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = card.name;
                if (AccountManager.Instance.resource.cardPortraite.ContainsKey(card.id)) {
                    unit.transform.Find("Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[card.id];
                }

                unit.GetComponent<Button>().onClick.AddListener(() => {
                    OnSelectUnit(unit.GetComponent<CardData>());
                    OffUnitSelectModal();
                });
            }
        }

        public void OffUnitSelectModal() {
            unitSelectModal.SetActive(false);
        }

        public void OnSelectUnit(CardData selectedData) {
            selectedSlot.transform.Find("Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[selectedData.data.id];
            selectedSlot.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = selectedData.data.name;
            selectedSlot.GetComponent<CardData>().data = selectedData.data;
        }

        public void OnHandCardSettingModal(GameObject selectedSlot) {
            foreach (Transform tf in handSelectModalContent) {
                Destroy(tf.gameObject);
            }

            handSelectModal.SetActive(true);
            selectedHandSlot = selectedSlot;

            string slotCamp = selectedHandSlot.transform.parent.name;
            List<CollectionCard> selectedPools = new List<CollectionCard>();
            switch (selectedRace) {
                default:
                case "human":
                    if(slotCamp == "PlayerGrid") {
                        selectedPools = humanCards;
                    }
                    else {
                        selectedPools = orcCards;
                    }
                    break;
                case "orc":
                    if (slotCamp == "PlayerGrid") {
                        selectedPools = orcCards;
                    }
                    else {
                        selectedPools = humanCards;
                    }
                    break;
            }

            foreach (CollectionCard card in selectedPools) {
                GameObject unit = Instantiate(unitBox, handSelectModalContent);
                unit.GetComponent<CardData>().data = card;
                unit.SetActive(true);
                unit.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = card.name;
                if (AccountManager.Instance.resource.cardPortraite.ContainsKey(card.id)) {
                    unit.transform.Find("Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[card.id];
                }

                unit.GetComponent<Button>().onClick.AddListener(() => {
                    OnSelectHandCard(unit.GetComponent<CardData>());
                    handSelectModal.SetActive(false);
                });
            }
        }

        public void OnSelectHandCard(CardData cardData) {
            selectedHandSlot.GetComponent<CardData>().data = cardData.data;
            if (AccountManager.Instance.resource.cardPortraite.ContainsKey(cardData.data.id)) {
                selectedHandSlot.transform.Find("Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.cardPortraite[cardData.data.id];
            }

            selectedHandSlot.transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = cardData.data.name;
        }

        public void ResetHandSetting() {
            var grid = handSelectModal.transform.Find("Grid");
            var cardDatas = grid.GetComponentsInChildren<CardData>();
            foreach(CardData cardData in cardDatas) {
                cardData.Clear();
            }
        }

        public void SavePlacedUnitsId() {
            GameObject selectedPanel = null;
            switch (selectedRace) {
                default:
                case "human":
                    selectedPanel = unitEditModals[0];
                    break;
                case "orc":
                    selectedPanel = unitEditModals[1];
                    break;
            }
            Transform enemyRear = selectedPanel.transform.Find("Enemy/Rear");
            Transform enemyFront = selectedPanel.transform.Find("Enemy/Front");

            Transform playerRear = selectedPanel.transform.Find("Player/Rear");
            Transform playerFront = selectedPanel.transform.Find("Player/Front");
            
            for(int i=0; i<5; i++) {
                if (!string.IsNullOrEmpty(enemyRear.GetChild(i).GetComponent<CardData>().data.name)) {
                    var data = enemyRear.GetChild(i).GetComponent<CardData>().data;
                    string id = data.id;

                    if (data.camp == "human") startState.map.lines[i].human[0] = id;
                    else if (data.camp == "orc") startState.map.lines[i].orc[0] = id;
                }

                if (!string.IsNullOrEmpty(enemyFront.GetChild(i).GetComponent<CardData>().data.name)){
                    var data = enemyFront.GetChild(i).GetComponent<CardData>().data;
                    string id = data.id;

                    if (data.camp == "human") startState.map.lines[i].human[1] = id;
                    else if (data.camp == "orc") startState.map.lines[i].orc[1] = id;
                }

                if (!string.IsNullOrEmpty(playerRear.GetChild(i).GetComponent<CardData>().data.name)) {
                    var data = playerRear.GetChild(i).GetComponent<CardData>().data;
                    string id = data.id;

                    if (data.camp == "human") startState.map.lines[i].human[0] = id;
                    else if (data.camp == "orc") startState.map.lines[i].orc[0] = id;
                }

                if (!string.IsNullOrEmpty(playerFront.GetChild(i).GetComponent<CardData>().data.name)) {
                    var data = playerFront.GetChild(i).GetComponent<CardData>().data;
                    string id = data.id;

                    if (data.camp == "human") startState.map.lines[i].human[1] = id;
                    else if (data.camp == "orc") startState.map.lines[i].orc[1] = id;
                }
            }
        }

        public void SaveHandCardsId() {
            foreach(Transform tf in handSelectPlayerGrid) {
                CardData data = tf.GetComponent<CardData>();
                if (!string.IsNullOrEmpty(data.data.name)) {
                    if(data.data.camp == "human") {
                        if (data.data.isHeroCard) {
                            startState.players.human.deck.heroCards.Add(data.data.id);
                        }
                        else {
                            startState.players.human.deck.handCards.Add(data.data.id);
                        }
                    }
                    else if(data.data.camp == "orc") {
                        if (data.data.isHeroCard) {
                            startState.players.orc.deck.heroCards.Add(data.data.id);
                        }
                        else {
                            startState.players.orc.deck.handCards.Add(data.data.id);
                        }
                    }
                }
            }

            foreach (Transform tf in handSelectEnemyGrid) {
                CardData data = tf.GetComponent<CardData>();
                if (!string.IsNullOrEmpty(data.data.name)) {
                    if (data.data.camp == "human") {
                        if (data.data.isHeroCard) {
                            startState.players.human.deck.heroCards.Add(data.data.id);
                        }
                        else {
                            startState.players.human.deck.handCards.Add(data.data.id);
                        }
                    }
                    else if (data.data.camp == "orc") {
                        if (data.data.isHeroCard) {
                            startState.players.orc.deck.heroCards.Add(data.data.id);
                        }
                        else {
                            startState.players.orc.deck.handCards.Add(data.data.id);
                        }
                    }
                }
            }
        }

        public void SaveHeroInfo() {
            if(selectedRace == "human") {
                startState.players.human.hero.currentHp = (int)playerHeroHpSlider.value;
                startState.players.human.hero.shieldGuage = (int)playerShieldSlider.value;

                startState.players.orc.hero.currentHp = (int)enemyHeroHpSlider.value;
                startState.players.orc.hero.shieldGuage = (int)enemyHeroShieldSlider.value;
            }
            else if(selectedRace == "orc") {
                startState.players.orc.hero.currentHp = (int)playerHeroHpSlider.value;
                startState.players.orc.hero.shieldGuage = (int)playerShieldSlider.value;

                startState.players.human.hero.currentHp = (int)enemyHeroHpSlider.value;
                startState.players.human.hero.shieldGuage = (int)enemyHeroShieldSlider.value;
            }
            else {
                Logger.Log("Something is wrong");
            }

            Variables.Saved.Set("SelectedRace", selectedRace);
            Variables.Saved.Set("SelectedBattleType", "test");
            Variables.Saved.Set("Editor_startState", startState);

            //SceneManager.Instance.LoadScene(SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }

        public struct Pos {
            public int col;
            public int row;

            public Pos(int col, int row) {
                this.col = col;
                this.row = row;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;


public class AttendanceManager : MonoBehaviour
{
    [SerializeField] MenuSceneController menuSceneController;
    [SerializeField] SkeletonGraphic getSpine;
    [SerializeField] SkeletonGraphic nextSlotSpine;
    bool comeback = false;
    bool welcome = false;
    bool onLaunchCheck = false;

    // Start is called before the first frame update
    private void Awake() {
        MainSceneStateHandler stateHandler = MainSceneStateHandler.Instance;
        if (stateHandler.GetState("IsTutorialFinished")) { AccountManager.Instance.RequestAttendance(); }
        else CloseAttendanceBoard();

        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_SUCCESS, AttendSuccess);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_ALREADY, AlreadyAttended);

    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_SUCCESS, AttendSuccess);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_ALREADY, AlreadyAttended);

    }

    public void OpenAttendanceBoard() {
        if (!onLaunchCheck) 
            onLaunchCheck = true;
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;
        transform.Find("MonthlyBoard").gameObject.SetActive(true);
        transform.Find("WeeklyBoard").gameObject.SetActive(false);
        AccountManager.Instance.RequestAttendance();

        MainSceneStateHandler.Instance.ChangeState("NeedToCallAttendanceBoard", false);
    }

    public void CloseAttendanceBoard() {
        gameObject.SetActive(false);
        AccountManager.Instance.RequestMailBoxNum();
        transform.localScale = Vector3.zero;
    }

    private void AttendSuccess(Enum Event_Type, Component Sender, object Param) {
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;
        SetMonthlyBoard();
    }

    private void AlreadyAttended(Enum Event_Type, Component Sender, object Param) {
        if (!onLaunchCheck) {
            onLaunchCheck = true;
            CloseAttendanceBoard();
        }
        else {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            transform.Find("MonthlyBoard").gameObject.SetActive(true);
            transform.Find("WeeklyBoard").gameObject.SetActive(false);
            SetMonthlyBoardChecked();
        }
    }

    public void InitBoard(Transform slotList) {
        getSpine.gameObject.SetActive(false);
        nextSlotSpine.transform.localScale = new Vector3(0, 0, 0);
        for (int i = 0; i < slotList.childCount; i++) {
            slotList.GetChild(i).Find("Block").gameObject.SetActive(false);
        }
    }

    void SetMonthlyBoard() {
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.RemoveAllListeners();
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.AddListener(() => SetWeaklyBoard());
        Transform slotList = transform.Find("MonthlyBoard/DayList");
        InitBoard(slotList);
        dataModules.AttendanceResult boardInfo = AccountManager.Instance.attendanceResult;
        int days = boardInfo.attendance.monthly - 1;
        for (int i = 0; i < boardInfo.tables.monthly.Length; i++) {
            slotList.GetChild(i).gameObject.SetActive(true);
            if (i < days) 
                slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
            else if (i == days) 
                StartCoroutine(GetRewardAimation(slotList.GetChild(i).gameObject));
            else
                slotList.GetChild(i).Find("Block").gameObject.SetActive(false);
            //if (boardInfo.tables.monthly[i].reward.kind.Contains("Box"))
            //    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
            //else
            string rewardKind = boardInfo.tables.monthly[i].reward[0].kind;
            slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + boardInfo.tables.monthly[i].reward[0].amount;
            slotList.GetChild(i).Find("Days/AttendDay").GetComponent<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();
            slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.scenarioRewardIcon[rewardKind];
            slotList.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
            slotList.GetChild(i).GetComponent<Button>().onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModal(rewardKind));
        }
    }

    public void SetWeaklyBoard() {
        transform.Find("MonthlyBoard").gameObject.SetActive(false);
        welcome = AccountManager.Instance.attendanceResult.attendance.welcome != 0;
        comeback = AccountManager.Instance.attendanceResult.attendance.comeback != 0;
        if (welcome == false && comeback == false) {
            CloseAttendanceBoard();
            return;
        }
        else {
            Transform slotList = transform.Find("WeeklyBoard/DayList");
            InitBoard(slotList);
            dataModules.AttendanceItem[] items;
            int days;
            if (welcome) {
                items = AccountManager.Instance.attendanceResult.tables.welcome;
                days = AccountManager.Instance.attendanceResult.attendance.welcome - 1;
                transform.Find("WeeklyBoard/Image/Type").GetComponent<TMPro.TextMeshProUGUI>().text 
                    = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_checkin_welcomcheckin");
            }
            else {
                items = AccountManager.Instance.attendanceResult.tables.comeback;
                days = AccountManager.Instance.attendanceResult.attendance.comeback - 1;
                transform.Find("WeeklyBoard/Image/Type").GetComponent<TMPro.TextMeshProUGUI>().text
                    = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_checkin_returncheckin");
            }
            for (int i = 0; i < items.Length; i++) {
                slotList.GetChild(i).gameObject.SetActive(true);
                if (i < days)
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
                else if (i == days) {
                    if (i == 6)
                        StartCoroutine(GetWeekLastReward(slotList.GetChild(i).gameObject));
                    else if (i == 5)
                        StartCoroutine(GetRewardAimation(slotList.GetChild(i).gameObject, true));
                    else
                        StartCoroutine(GetRewardAimation(slotList.GetChild(i).gameObject));
                }
                else 
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(false);

                //if (items[i].reward.kind.Contains("Box"))
                //    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
                //else
                string rewardKind = items[i].reward[0].kind;
                slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + items[i].reward[0].amount;
                slotList.GetChild(i).Find("Days/AttendDay").GetComponent<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();
                slotList.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                if (!items[i].reward[0].kind.Contains("cardSpecific")) {
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.scenarioRewardIcon[rewardKind];
                    slotList.GetChild(i).GetComponent<Button>().onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModal(rewardKind));
                }
                else {
                    Transform specificCardWindow = transform.Find("SpecificCard");
                    string card1 = items[i].reward[0].cardId;
                    string card2 = items[i].reward[1].cardId;
                    for (int j = 0; j < 6; j++) {
                        specificCardWindow.Find("RewardCard1/Rarelity").GetChild(i).gameObject.SetActive(false);
                        specificCardWindow.Find("RewardCard2/Rarelity").GetChild(i).gameObject.SetActive(false);
                    }
                    specificCardWindow.Find("RewardCard1/DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(card1);
                    specificCardWindow.Find("RewardCard1/Name").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.allCardsDic[card1].name;
                    specificCardWindow.Find("RewardCard1/Rarelity/" + AccountManager.Instance.allCardsDic[card1].rarelity).gameObject.SetActive(true);
                    specificCardWindow.Find("RewardCard2/DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(card2);
                    specificCardWindow.Find("RewardCard2/Name").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.allCardsDic[card2].name;
                    specificCardWindow.Find("RewardCard2/Rarelity/" + AccountManager.Instance.allCardsDic[card2].rarelity).gameObject.SetActive(true);
                    slotList.GetChild(i).GetComponent<Button>().onClick.AddListener(() => OpenSpecificCardInfo());
                }
            }
            transform.Find("WeeklyBoard").gameObject.SetActive(true);
        }
    }


    public void SetMonthlyBoardChecked() {
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.RemoveAllListeners();
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.AddListener(() => SetWeaklyBoardChecked());
        Transform slotList = transform.Find("MonthlyBoard/DayList");
        InitBoard(slotList);
        dataModules.AttendanceResult boardInfo = AccountManager.Instance.attendanceResult;

        int days = boardInfo.attendance.monthly - 1;
        for (int i = 0; i < boardInfo.tables.monthly.Length; i++) {
            slotList.GetChild(i).gameObject.SetActive(true);
            if (i <= days)
                slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
            else 
                slotList.GetChild(i).Find("Block").gameObject.SetActive(false);
            
            string rewardKind = boardInfo.tables.monthly[i].reward[0].kind;
            slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + boardInfo.tables.monthly[i].reward[0].amount;
            slotList.GetChild(i).Find("Days/AttendDay").GetComponent<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();
            slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.scenarioRewardIcon[rewardKind];
            slotList.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
            slotList.GetChild(i).GetComponent<Button>().onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModal(rewardKind));
        }
        if(days + 1 < 27)
            StartCoroutine(SetNextSlot(slotList.GetChild(days + 1).transform));
    }

    IEnumerator SetNextSlot(Transform target, bool wait = false) {
        if(wait)
            yield return new WaitForSeconds(0.5f);
        else
            yield return new WaitForSeconds(0.01f);
        nextSlotSpine.transform.SetParent(target);
        nextSlotSpine.transform.localPosition = Vector3.zero;
        nextSlotSpine.transform.localScale = Vector3.one;
        nextSlotSpine.transform.SetParent(transform);
        nextSlotSpine.transform.SetSiblingIndex(2);
        nextSlotSpine.gameObject.SetActive(true);
    }

    IEnumerator SetWeeklyLastSlot(Transform target, bool wait = false) {
        if (wait)
            yield return new WaitForSeconds(0.5f);
        else
            yield return new WaitForSeconds(0.01f);
        target.Find("NextGift").gameObject.SetActive(true);
    }

    public void SetWeaklyBoardChecked() {
        transform.Find("MonthlyBoard").gameObject.SetActive(false);
        welcome = AccountManager.Instance.attendanceResult.attendance.welcome != 0;
        comeback = AccountManager.Instance.attendanceResult.attendance.comeback != 0;
        if (welcome == false && comeback == false) {
            CloseAttendanceBoard();
            return;
        }
        else {
            Transform slotList = transform.Find("WeeklyBoard/DayList");
            InitBoard(slotList);
            dataModules.AttendanceItem[] items;
            int days;
            if (welcome) {
                items = AccountManager.Instance.attendanceResult.tables.welcome;
                days = AccountManager.Instance.attendanceResult.attendance.welcome - 1;
                transform.Find("WeeklyBoard/Image/Type").GetComponent<TMPro.TextMeshProUGUI>().text
                    = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_checkin_welcomcheckin");
            }
            else {
                items = AccountManager.Instance.attendanceResult.tables.comeback;
                days = AccountManager.Instance.attendanceResult.attendance.comeback - 1;
                transform.Find("WeeklyBoard/Image/Type").GetComponent<TMPro.TextMeshProUGUI>().text
                    = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_checkin_returncheckin");
            }
            for (int i = 0; i < items.Length; i++) {
                slotList.GetChild(i).gameObject.SetActive(true);
                if (i <= days)
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
                else 
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(false);

                string rewardKind = items[i].reward[0].kind;
                slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + items[i].reward[0].amount;
                slotList.GetChild(i).Find("Days/AttendDay").GetComponent<TMPro.TextMeshProUGUI>().text = (i + 1).ToString();
                slotList.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                if (!items[i].reward[0].kind.Contains("cardSpecific")) {
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.scenarioRewardIcon[rewardKind];
                    slotList.GetChild(i).GetComponent<Button>().onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModal(rewardKind));
                }
                else {
                    Transform specificCardWindow = transform.Find("SpecificCard");
                    string card1 = items[i].reward[0].cardId;
                    string card2 = items[i].reward[1].cardId;
                    for (int j = 0; j < 6; j++) {
                        specificCardWindow.Find("RewardCard1/Rarelity").GetChild(i).gameObject.SetActive(false);
                        specificCardWindow.Find("RewardCard2/Rarelity").GetChild(i).gameObject.SetActive(false);
                    }
                    specificCardWindow.Find("RewardCard1/DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(card1);
                    specificCardWindow.Find("RewardCard1/Name").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.allCardsDic[card1].name;
                    specificCardWindow.Find("RewardCard1/Rarelity/" + AccountManager.Instance.allCardsDic[card1].rarelity).gameObject.SetActive(true);
                    specificCardWindow.Find("RewardCard2/DictionaryCardVertical").GetComponent<MenuCardHandler>().DrawCard(card2);
                    specificCardWindow.Find("RewardCard2/Name").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.allCardsDic[card2].name;
                    specificCardWindow.Find("RewardCard2/Rarelity/" + AccountManager.Instance.allCardsDic[card2].rarelity).gameObject.SetActive(true);
                    slotList.GetChild(i).GetComponent<Button>().onClick.AddListener(() => OpenSpecificCardInfo());
                }
            }
            if (days + 1 < 6) 
                StartCoroutine(SetNextSlot(slotList.GetChild(days + 1).transform));
            //else
            //    StartCoroutine(SetWeeklyLastSlot(slotList.GetChild(days).transform));
            transform.Find("WeeklyBoard").gameObject.SetActive(true);
        }
    }

    public void OpenSpecificCardInfo() {
        transform.Find("SpecificCard").gameObject.SetActive(true);
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseSpecificCardInfo);
    }

    public void CloseSpecificCardInfo() {
        transform.Find("SpecificCard").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseSpecificCardInfo);
    }


    IEnumerator GetRewardAimation(GameObject target, bool isWeeklyLast = false) {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() =>
            !menuSceneController.hideModal.activeSelf
            && !menuSceneController.storyLobbyPanel.activeSelf
            && !menuSceneController.battleReadyPanel.activeSelf
        );
        getSpine.transform.SetParent(target.transform);
        getSpine.transform.localPosition = new Vector3(0, 0, 0);
        getSpine.transform.SetParent(transform);
        getSpine.transform.SetSiblingIndex(3);
        getSpine.gameObject.SetActive(true);
        getSpine.Initialize(false);
        getSpine.Update(0);
        getSpine.AnimationState.SetAnimation(0, "animation", false);
        
        int index = target.transform.GetSiblingIndex();
        if (index < 27 && !isWeeklyLast) {
            StartCoroutine(SetNextSlot(target.transform.parent.GetChild(index + 1).transform));
        }
        else
            StartCoroutine(SetWeeklyLastSlot(target.transform.parent.GetChild(index + 1).transform, true));
    }

    IEnumerator GetWeekLastReward(GameObject target) {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() =>
            !menuSceneController.hideModal.activeSelf
            && !menuSceneController.storyLobbyPanel.activeSelf
            && !menuSceneController.battleReadyPanel.activeSelf
        );
        SkeletonGraphic spine = target.transform.Find("Spine").GetComponent<SkeletonGraphic>();
        spine.gameObject.SetActive(true);
        spine.Initialize(false);
        spine.Update(0);
        spine.AnimationState.SetAnimation(0, "animation", false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttendanceManager : MonoBehaviour
{
    bool comeback = false;
    bool welcome = false;
    bool onLaunchCheck = false;
    // Start is called before the first frame update
    private void Awake() {
        bool IsTutorialFinished = Convert.ToBoolean(PlayerPrefs.GetInt("IsTutorialFinished", 0));
        if(IsTutorialFinished) AccountManager.Instance.RequestAttendance();

        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_SUCCESS, AttendSuccess);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_ALREADY, AlreadyAttended);
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_SUCCESS, AttendSuccess);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_ALREADY, AlreadyAttended);
    }

    public void OpenAttendanceBoard() {
        transform.gameObject.SetActive(true);
        transform.Find("MonthlyBoard").gameObject.SetActive(true);
        transform.Find("WeeklyBoard").gameObject.SetActive(false);
        AccountManager.Instance.RequestAttendance();
    }

    public void CloseAttendanceBoard() {
        transform.gameObject.SetActive(false);
    }

    private void AttendSuccess(Enum Event_Type, Component Sender, object Param) {
        SetMonthlyBoard();
    }

    private void AlreadyAttended(Enum Event_Type, Component Sender, object Param) {
        if (!onLaunchCheck) {
            onLaunchCheck = true;
            AccountManager.Instance.RequestAttendanceBoard();
            CloseAttendanceBoard();
        }
        else {
            transform.gameObject.SetActive(true);
            transform.Find("MonthlyBoard").gameObject.SetActive(true);
            transform.Find("WeeklyBoard").gameObject.SetActive(false);
            SetMonthlyBoardChecked();
        }
    }

    public void InitBoard(Transform slotList) {
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
        int days = AccountManager.Instance.attendanceResult.attendance.monthly;
        for (int i = 0; i < boardInfo.table.monthly.Length; i++) {
            if (i < days) 
                slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
            else if (i == days) 
                StartCoroutine(GetRewardAimation(slotList.GetChild(i).Find("Block").gameObject));
            else
                slotList.GetChild(i).Find("Block").gameObject.SetActive(false);
            if (boardInfo.table.monthly[i].reward.kind.Contains("Box"))
                slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
            else
                slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[boardInfo.table.monthly[i].reward.kind];
            slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + boardInfo.table.monthly[i].reward.amount;
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
            transform.Find("WeeklyBoard").gameObject.SetActive(true);
            Transform slotList = transform.Find("WeeklyBoard/DayList");
            InitBoard(slotList);
            dataModules.AttendanceItem[] items;
            int days;
            if (welcome) {
                items = AccountManager.Instance.attendanceResult.table.welcome;
                days = AccountManager.Instance.attendanceResult.attendance.welcome;
            }
            else {
                items = AccountManager.Instance.attendanceResult.table.comeback;
                days = AccountManager.Instance.attendanceResult.attendance.comeback;
            }
            for (int i = 0; i < items.Length; i++) {
                if (i < days)
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
                else if (i == days)
                    StartCoroutine(GetRewardAimation(slotList.GetChild(i).Find("Block").gameObject));
                else
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(false);

                if (items[i].reward.kind.Contains("Box"))
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
                else
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[items[i].reward.kind];
                slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + items[i].reward.amount;
            }
        }
    }


    public void SetMonthlyBoardChecked() {
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.RemoveAllListeners();
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.AddListener(() => SetWeaklyBoardChecked());
        Transform slotList = transform.Find("MonthlyBoard/DayList");
        InitBoard(slotList);
        dataModules.AttendanceReward boardInfo = AccountManager.Instance.attendanceBoard;

        int days = AccountManager.Instance.attendanceResult.attendance.monthly;
        for (int i = 0; i < boardInfo.monthly.Length; i++) {
            if (i <= days) 
                slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
            else
                slotList.GetChild(i).Find("Block").gameObject.SetActive(false);

            if (boardInfo.monthly[i].reward.kind.Contains("Box"))
                slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
            else
                slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[boardInfo.monthly[i].reward.kind];
            slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + boardInfo.monthly[i].reward.amount;
        }
    }

    public void SetWeaklyBoardChecked() {
        transform.Find("MonthlyBoard").gameObject.SetActive(false);
        welcome = AccountManager.Instance.attendanceBoard.welcome != null;
        comeback = AccountManager.Instance.attendanceBoard.comeback != null;
        if (welcome == false && comeback == false) {
            CloseAttendanceBoard();
            return;
        }
        else {
            transform.Find("WeeklyBoard").gameObject.SetActive(true);
            Transform slotList = transform.Find("WeeklyBoard/DayList");
            InitBoard(slotList);
            dataModules.AttendanceItem[] items;
            int days;
            if (welcome) {
                items = AccountManager.Instance.attendanceBoard.welcome;
                days = AccountManager.Instance.attendanceResult.attendance.welcome;
            }
            else {
                items = AccountManager.Instance.attendanceBoard.comeback;
                days = AccountManager.Instance.attendanceResult.attendance.comeback;
            }
            for (int i = 0; i < items.Length; i++) {
                if (i <= days)
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
                else
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(false);
                if (items[i].reward.kind.Contains("Box"))
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
                else
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[items[i].reward.kind];
                slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + items[i].reward.amount;
            }
        }
    }


    IEnumerator GetRewardAimation(GameObject target) {
        target.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        target.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        target.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        target.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        target.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        target.SetActive(true);
    }
}

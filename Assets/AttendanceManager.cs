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
        AccountManager.Instance.RequestAttendance();
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_SUCCESS, AttendSuccess);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ATTEND_ALREADY, AlreadyAttended);
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
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.AddListener(() => SetWeaklyBoard());
        Transform slotList = transform.Find("MonthlyBoard/DayList");
        InitBoard(slotList);
        dataModules.AttendanceResult boardInfo = AccountManager.Instance.attendanceResult;
        bool check = false;
        for (int i = 0; i < boardInfo.table.monthly.Length; i++) {
            if (!check) { 
                if (boardInfo.table.monthly[i].attend)
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
                else {
                    if (i - 1 == 0) {
                        StartCoroutine(GetRewardAimation(slotList.GetChild(i - 1).Find("Block").gameObject));
                        check = true;
                    }
                    if (i != 0 && boardInfo.table.monthly[i - 1].attend) {
                        StartCoroutine(GetRewardAimation(slotList.GetChild(i - 1).Find("Block").gameObject));
                        check = true;
                    }
                }
            }
            if (boardInfo.table.monthly[i].reward.kind.Contains("Box"))
                slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
            else
                slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[boardInfo.table.monthly[i].reward.kind];
            slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + boardInfo.table.monthly[i].reward.amount;
        }
    }

    public void SetWeaklyBoard() {
        transform.Find("MonthlyBoard").gameObject.SetActive(false);
        welcome = AccountManager.Instance.attendanceResult.attendance.welcome == 1;
        comeback = AccountManager.Instance.attendanceResult.attendance.comeback == 1;
        if (welcome == false && comeback == false) {
            CloseAttendanceBoard();
            return;
        }
        else {
            transform.Find("WeeklyBoard").gameObject.SetActive(true);
            Transform slotList = transform.Find("WeeklyBoard/DayList");
            InitBoard(slotList);
            dataModules.AttendanceItem[] items;
            if (welcome)
                items = AccountManager.Instance.attendanceResult.table.welcome;
            else
                items = AccountManager.Instance.attendanceResult.table.comeback;
            bool check = false;
            for (int i = 0; i < items.Length; i++) {
                if (!check) {
                    if (items[i].attend)
                        slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
                    else {
                        if (i - 1 == 0) {
                            StartCoroutine(GetRewardAimation(slotList.GetChild(i - 1).Find("Block").gameObject));
                            check = true;
                        }
                        if (i != 0 && items[i - 1].attend) {
                            StartCoroutine(GetRewardAimation(slotList.GetChild(i - 1).Find("Block").gameObject));
                            check = true;
                        }
                    }
                }
                if (items[i].reward.kind.Contains("Box"))
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["supplyBox"];
                else
                    slotList.GetChild(i).Find("Resource").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[items[i].reward.kind];
                slotList.GetChild(i).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + items[i].reward.amount;
            }
        }
    }


    public void SetMonthlyBoardChecked() {
        transform.Find("MonthlyBoard/BackGround").GetComponent<Button>().onClick.AddListener(() => SetWeaklyBoardChecked());
        Transform slotList = transform.Find("MonthlyBoard/DayList");
        InitBoard(slotList);
        dataModules.AttendanceReward boardInfo = AccountManager.Instance.attendanceBoard;
        bool check = false;
        for (int i = 0; i < boardInfo.monthly.Length; i++) {
            if (!check) {
                if (boardInfo.monthly[i].attend)
                    slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
            }
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
            if (welcome)
                items = AccountManager.Instance.attendanceBoard.welcome;
            else
                items = AccountManager.Instance.attendanceBoard.comeback;
            bool check = false;
            for (int i = 0; i < items.Length; i++) {
                if (!check) {
                    if (items[i].attend)
                        slotList.GetChild(i).Find("Block").gameObject.SetActive(true);
                }
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

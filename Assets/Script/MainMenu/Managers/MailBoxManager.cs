using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailBoxManager : MonoBehaviour
{
    [SerializeField] Transform mailListParent;
    [SerializeField] HUDController HUDController;

    private void OnEnable() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_UPDATE, RequestMailOver);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_RECEIVE, RequestResources);
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, RequestOver);
    }

    private void OnDisable() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_UPDATE, RequestMailOver);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAIL_RECEIVE, RequestResources);
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, RequestOver);
    }

    public void OpenMailBox() {
        gameObject.SetActive(true);
        transform.Find("Block").gameObject.SetActive(true);
        AccountManager.Instance.RequestMailBox();

        HUDController.SetHeader(HUDController.Type.ONLY_BAKCK_BUTTON);
        HUDController.SetBackButton(() => { 
            CloseMailBox();
            HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);
            if(quest != null) {
                quest
                    .AddSpinetoButtonAndRemoveClick(
                        quest.manager.tutorialSerializeList.backButton, 
                        quest.BreakCardDictionaryTab
                    );
            }
        });
    }

    public void CloseMailBox() {
        gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseMailBox);
    }

    public void RequestMailOver(Enum Event_Type, Component Sender, object Param) {
        SetMailBox();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseMailBox);
    }

    public Quest.QuestContentController quest;

    public void QuestSet(Quest.QuestContentController quest) {
        this.quest = quest;
    }

    private void TutoQuest() {
        if(quest == null) return;
        quest.MailOpen();
    }

    public void SetMailBox() {
        InitMailBox();
        TutoQuest();
        int count = 0;
        foreach(dataModules.Mail mail in AccountManager.Instance.mailList) {
            Transform slot = mailListParent.GetChild(count);
            slot.gameObject.SetActive(true);
            slot.Find("OpenMailBtn").GetComponent<Button>().onClick.AddListener(() => OpenMail(mail));
            slot.Find("RecieveBtn").GetComponent<Button>().onClick.AddListener(() => ReceiveMail(mail));
            slot.Find("MailText").GetComponent<TMPro.TextMeshProUGUI>().text = mail.context;
            slot.Find("From").GetComponent<TMPro.TextMeshProUGUI>().text = mail.sender;
            if (mail.expiredAt == null)
                slot.Find("LeftTime").GetComponent<TMPro.TextMeshProUGUI>().text = "영구 보관";
            else {
                DateTime endTime = Convert.ToDateTime(mail.expiredAt);
                TimeSpan leftTime = endTime - DateTime.Now;
                if (leftTime.Days >= 1)
                    slot.Find("LeftTime").GetComponent<TMPro.TextMeshProUGUI>().text = leftTime.Days.ToString() + "일";
                else {
                    slot.Find("LeftTime").GetComponent<TMPro.TextMeshProUGUI>().text = leftTime.Hours.ToString() + "시간 " + leftTime.Minutes.ToString() + "분";
                }
            }
            int itemCount = 0;
            foreach(dataModules.MailItem item in mail.items) {
                if (item.kind == null) continue;
                if(AccountManager.Instance.resource.rewardIcon.ContainsKey(item.kind))
                    slot.Find("RewardList").GetChild(itemCount).GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[item.kind];
                slot.Find("RewardList").GetChild(itemCount).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = item.amount.ToString();
                slot.Find("RewardList").GetChild(itemCount).gameObject.SetActive(true);
                itemCount++;
            }
            count++;
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mailListParent.parent.GetComponent<RectTransform>());
        transform.Find("Block").gameObject.SetActive(false);
    }

    public void InitMailBox() {
        for (int i = 0; i < mailListParent.childCount; i++) {
            mailListParent.GetChild(i).gameObject.SetActive(false);
            mailListParent.GetChild(i).Find("RecieveBtn").GetComponent<Button>().onClick.RemoveAllListeners();
            for (int j = 0; j < mailListParent.GetChild(i).Find("RewardList").childCount; j++)
                mailListParent.GetChild(i).Find("RewardList").GetChild(j).gameObject.SetActive(false);
        }
    }

    public void OpenMail(dataModules.Mail mail) {
        AccountManager.Instance.RequestReadMail(mail.id.ToString());
        Transform openedWindow = transform.Find("Content/OpenedMail");
        openedWindow.gameObject.SetActive(true);
        openedWindow.Find("MailText").GetComponent<TMPro.TextMeshProUGUI>().text = mail.context;
        openedWindow.Find("From").GetComponent<TMPro.TextMeshProUGUI>().text = mail.sender;
        if (mail.expiredAt == null)
            openedWindow.Find("Until").GetComponent<TMPro.TextMeshProUGUI>().text = "영구 보관";
        else {
            DateTime endTime = Convert.ToDateTime(mail.expiredAt);
            TimeSpan leftTime = endTime - DateTime.Now;
            if (leftTime.Days >= 1)
                openedWindow.Find("Until").GetComponent<TMPro.TextMeshProUGUI>().text = leftTime.Days.ToString() + "일";
            else {
                openedWindow.Find("Until").GetComponent<TMPro.TextMeshProUGUI>().text = leftTime.Hours.ToString() + "시간 " + leftTime.Minutes.ToString() + "분";
            }
        }
        openedWindow.Find("RecieveBtn").GetComponent<Button>().onClick.AddListener(() => ReceiveMail(mail));
        int itemCount = 0;
        foreach(dataModules.MailItem item in mail.items) {
            if (item.kind == null) continue;
            if (AccountManager.Instance.resource.rewardIcon.ContainsKey(item.kind))
                openedWindow.Find("Rewards").GetChild(itemCount).GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[item.kind];
            openedWindow.Find("Rewards").GetChild(itemCount).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = item.amount.ToString();
            openedWindow.Find("Rewards").GetChild(itemCount).gameObject.SetActive(true);
            itemCount++;
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseMail);
    }

    public void ReceiveMail(dataModules.Mail mail) {
        transform.Find("Block").gameObject.SetActive(true);
        AccountManager.Instance.RequestReceiveMail(mail.id.ToString());
    }

    public void ReceiveAllMail() {
        transform.Find("Block").gameObject.SetActive(true);
        AccountManager.Instance.RequestReceiveMail("all");
    }

    public void RequestResources(Enum Event_Type, Component Sender, object Param) {
        AccountManager.Instance.RequestMailBox();
        AccountManager.Instance.RequestUserInfo();
        AccountManager.Instance.RequestInventories();
        SetReceiveResult();
    }

    public void RequestOver(Enum Event_Type, Component Sender, object Param) {
        transform.Find("Block").gameObject.SetActive(false);
        transform.GetChild(0).Find("ReceivedReward").gameObject.SetActive(true);
        CloseMail();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseReceiveResult);
    }

    public void CloseReceiveResult() {
        InitRewardList();
        transform.GetChild(0).Find("ReceivedReward").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseReceiveResult);
    }

    public void SetReceiveResult(List<dataModules.MailReward> rewardList = null ) {
        Transform slotList = transform.GetChild(0).Find("ReceivedReward/RowSlot");
        List<dataModules.MailReward> itemList = new List<dataModules.MailReward>();
        if (rewardList != null)
            itemList = rewardList;
        else
            itemList = AccountManager.Instance.mailRewardList;
        for (int i = 0; i < AccountManager.Instance.mailRewardList.Count; i++) {
            Transform target;
            slotList.GetChild(i / 3).gameObject.SetActive(true);
            slotList.GetChild(i / 3).GetChild(i % 3).gameObject.SetActive(true);
            if (itemList[i].kind.Contains("card")) {
                target = slotList.GetChild(i / 3).GetChild(i % 3).Find("Reward/RewardCard");
                
                string cardId = itemList[i].cards[0].cardId;
                slotList.GetChild(i / 3).GetChild(i % 3).Find("NameOrNum").GetComponent<TMPro.TextMeshProUGUI>().text
                    = AccountManager.Instance.allCardsDic[cardId].name;
                target.GetComponent<MenuCardHandler>().DrawCard(cardId);
                if(itemList[i].cards[0].crystal != 0) {
                    target.Find("GetCrystal").gameObject.SetActive(true);
                    target.Find("GetCrystal").GetChild(0).Find("MagicBlock").gameObject.SetActive(AccountManager.Instance.allCardsDic[cardId].type == "magic");
                    target.Find("GetCrystal").GetChild(0).Find("UnitBlock").gameObject.SetActive(AccountManager.Instance.allCardsDic[cardId].type == "unit");
                    target.Find("GetCrystal").GetChild(0).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = itemList[i].cards[0].crystal.ToString();
                }
                else
                    target.Find("GetCrystal").gameObject.SetActive(false);
            }
            else {
                target = slotList.GetChild(i / 3).GetChild(i % 3).Find("Reward/Resource");
                string item = itemList[i].kind;
                if (itemList[i].kind.Contains("gold"))
                    item = "gold";
                target.GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon["result_" + item];
                slotList.GetChild(i / 3).GetChild(i % 3).Find("NameOrNum").GetComponent<TMPro.TextMeshProUGUI>().text = itemList[i].amount;
            }
            target.SetAsFirstSibling();
            target.gameObject.SetActive(true);
            if (i == 8 && AccountManager.Instance.mailRewardList.Count > 9) {
                int count = 9;
                while(count > 0) {
                    itemList.RemoveAt(0);
                    count--;
                }
                Button nextBtn = transform.Find("Content/ReceivedReward/Buttons/Next").GetComponent<Button>();
                nextBtn.gameObject.SetActive(true);
                nextBtn.onClick.RemoveAllListeners();
                nextBtn.onClick.AddListener(() => SetNextRewardPage(itemList));
                break;
            }
            else
                transform.Find("Content/ReceivedReward/Buttons/Next").gameObject.SetActive(false);
        }
    }

    void SetNextRewardPage(List<dataModules.MailReward> itemList) {
        InitRewardList();
        SetReceiveResult(itemList);
    }

    void InitRewardList() {
        Transform slotList = transform.GetChild(0).Find("ReceivedReward/RowSlot");
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                slotList.GetChild(i).GetChild(j).gameObject.SetActive(false);
                slotList.GetChild(i).GetChild(j).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
            slotList.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void CloseMail() {
        transform.GetChild(0).Find("OpenedMail").gameObject.SetActive(false);
        transform.GetChild(0).Find("OpenedMail/RecieveBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        for (int i = 0; i < transform.Find("Content/OpenedMail/Rewards").childCount; i++)
            transform.Find("Content/OpenedMail/Rewards").GetChild(i).gameObject.SetActive(false);
        if(EscapeKeyController.escapeKeyCtrl.escapeFunc.Contains(CloseMail))
            EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseMail);

        Handheld.Vibrate();
        
    }
}

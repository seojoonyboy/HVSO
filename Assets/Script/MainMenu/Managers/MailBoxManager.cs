using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailBoxManager : MonoBehaviour
{
    [SerializeField] Transform mailListParent;
    [SerializeField] HUDController HUDController;
    [SerializeField] Button receiveAllBtn;
    [SerializeField] BoxRewardManager boxRewardManager;

    bool received;

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
        HUDController.SetBackButton(CloseMailBox);
    }

    public void CloseMailBox() {
        CloseReceiveResult();
        gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseMailBox);
        HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);
        if(tutoQuest != null && tutoQuest.received) tutoQuest.quest.BreakCardDictionaryTab();
    }

    public void RequestMailOver(Enum Event_Type, Component Sender, object Param) {
        SetMailBox();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseMailBox);
    }

    public TutorialQuest tutoQuest;
    public class TutorialQuest {
        public Quest.QuestContentController quest;
        public Button openBtn;
        public Button receiveBtn;
        public bool pressed = false;
        public bool received = false;
    }

    private void TutoQuest(Button openBtn, Button receiveBtn) {
        if(tutoQuest.quest == null) return;
        if(tutoQuest.openBtn != null) {
            tutoQuest.openBtn.enabled = true;
        }
        if(tutoQuest.receiveBtn != null) {
            tutoQuest.receiveBtn.onClick.RemoveListener(PressTuto);
            Transform hand = tutoQuest.receiveBtn.transform.Find("tutorialHand");
            if(hand == null) return;
            Destroy(hand.gameObject);
        }
        tutoQuest.openBtn = openBtn;
        tutoQuest.receiveBtn = receiveBtn;
        tutoQuest.quest.AddSpinetoButtonAndRemoveClick(tutoQuest.receiveBtn, PressTuto);
        tutoQuest.openBtn.enabled = false;
        tutoQuest.quest.manager.tutorialSerializeList.mailAllGetButton.interactable = false;
    }

    private void PressTuto() {
        tutoQuest.pressed = true;
    }

    public void SetMailBox() {
        transform.Find("Content/RecieveAllBtn").gameObject.SetActive(AccountManager.Instance.mailList.Count != 0);
        GetComponent<ObjectPool.ObjectPoolManager>().Initialize(() => {
            foreach (dataModules.Mail mail in AccountManager.Instance.mailList) {
                GameObject slot = GetComponent<ObjectPool.ObjectPoolManager>().GetObject();

                slot.transform.Find("RecieveBtn").GetComponent<Button>().onClick.RemoveAllListeners();
                for (int j = 0; j < slot.transform.Find("RewardList").childCount; j++)
                    slot.transform.Find("RewardList").GetChild(j).gameObject.SetActive(false);

                slot.transform.Find("OpenMailBtn").GetComponent<Button>().onClick.AddListener(() => OpenMail(mail));
                slot.transform.Find("RecieveBtn").GetComponent<Button>().onClick.AddListener(() => ReceiveMail(mail));
                slot.transform.Find("MailText").GetComponent<TMPro.TextMeshProUGUI>().text = mail.context;
                slot.transform.Find("From").GetComponent<TMPro.TextMeshProUGUI>().text = mail.sender;
                if (mail.expiredAt == null)
                    slot.transform.Find("LeftTime").GetComponent<TMPro.TextMeshProUGUI>().text = "영구 보관";
                else {
                    DateTime endTime = Convert.ToDateTime(mail.expiredAt);
                    TimeSpan leftTime = endTime - DateTime.Now;
                    string localizeText = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_raid_timeleft");
                    string localizeTime = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainUI", "ui_page_shop_dayhourmin");
                    if (leftTime.Days >= 1)
                        slot.transform.Find("LeftTime").GetComponent<TMPro.TextMeshProUGUI>().text = localizeText + leftTime.Days.ToString() + "일";
                    else {
                        slot.transform.Find("LeftTime").GetComponent<TMPro.TextMeshProUGUI>().text = localizeText + leftTime.Hours.ToString() + "시간 " + leftTime.Minutes.ToString() + "분";
                    }
                }
                int itemCount = 0;
                foreach (dataModules.MailItem item in mail.items) {
                    if (item.kind == null) continue;
                    if (AccountManager.Instance.resource.rewardIcon.ContainsKey(item.kind))
                        slot.transform.Find("RewardList").GetChild(itemCount).GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[item.kind];
                    slot.transform.Find("RewardList").GetChild(itemCount).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = item.amount.ToString();
                    slot.transform.Find("RewardList").GetChild(itemCount).gameObject.SetActive(true);

                    if (slot.transform.Find("RewardList").GetChild(itemCount).GetComponent<Button>() == null) slot.transform.Find("RewardList").GetChild(itemCount).gameObject.AddComponent<Button>();

                    string item_kind = item.kind;
                    Button rewardIconBtn = slot.transform.Find("RewardList").GetChild(itemCount).GetComponent<Button>();
                    rewardIconBtn.onClick.RemoveAllListeners();
                    RewardDescriptionHandler rewardDescriptionHandler = RewardDescriptionHandler.instance;
                    rewardIconBtn.onClick.AddListener(() => {
                        rewardDescriptionHandler.RequestDescriptionModal(item_kind);
                        //Logger.Log("item_kind : " + item_kind);
                    });

                    itemCount++;
                }
            }
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(mailListParent.parent.GetComponent<RectTransform>());
            transform.Find("Block").gameObject.SetActive(false);
        });
    }

    public void OpenMail(dataModules.Mail mail) {
        AccountManager.Instance.RequestReadMail(mail.id.ToString());
        Transform openedWindow = transform.Find("Content/OpenedMail");
        openedWindow.gameObject.SetActive(true);
        openedWindow.Find("MailText").GetComponent<TMPro.TextMeshProUGUI>().text = mail.context;
        openedWindow.Find("From").GetComponent<TMPro.TextMeshProUGUI>().text = mail.sender;
        DateTime sentTime = Convert.ToDateTime(mail.createdAt);
        openedWindow.Find("Date").GetComponent<TMPro.TextMeshProUGUI>().text = sentTime.ToShortDateString();
        openedWindow.Find("Time").GetComponent<TMPro.TextMeshProUGUI>().text = sentTime.ToLongTimeString();
        openedWindow.Find("RecieveBtn").GetComponent<Button>().onClick.AddListener(() => ReceiveMail(mail));
        int itemCount = 0;
        RewardDescriptionHandler rewardDescriptionHandler = RewardDescriptionHandler.instance;
        foreach (dataModules.MailItem item in mail.items) {
            if (item.kind == null) continue;
            string item_kind = item.kind;
            Transform itemSlot = openedWindow.Find("Rewards").GetChild(itemCount);
            if (AccountManager.Instance.resource.rewardIcon.ContainsKey(item_kind)) {
                itemSlot.GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[item.kind];
                itemSlot.GetComponent<Button>().onClick.RemoveAllListeners();
                itemSlot.GetComponent<Button>().onClick.AddListener(() => rewardDescriptionHandler.RequestDescriptionModal(item_kind));
            }
            itemSlot.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = item.amount.ToString();
            itemSlot.gameObject.SetActive(true);
            itemCount++;
        }
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseMail);
    }

    public void ReceiveMail(dataModules.Mail mail) {
        transform.Find("Block").gameObject.SetActive(true);
        received = false;
        AccountManager.Instance.RequestReceiveMail(mail.id.ToString());
    }

    public void ReceiveAllMail() {
        transform.Find("Block").gameObject.SetActive(true);
        received = false;
        AccountManager.Instance.RequestReceiveMail("all");
    }

    public void RequestResources(Enum Event_Type, Component Sender, object Param) {
        AccountManager.Instance.RequestMailBox();
        AccountManager.Instance.RequestUserInfo();
        AccountManager.Instance.RequestInventories();
        StartCoroutine(SetReceiveResult());
    }

    public void RequestOver(Enum Event_Type, Component Sender, object Param) {
        transform.Find("Block").gameObject.SetActive(false);
        if (!received) {
            transform.Find("Content/ReceivedReward").gameObject.SetActive(true);
            received = true;
            SetRewardAnimation();
            CloseMail();
            EscapeKeyController.escapeKeyCtrl.AddEscape(CloseReceiveResult);
        }
    }

    public void CloseReceiveResult() {
        List<List<RewardClass>> rewards = new List<List<RewardClass>>();
        if (transform.Find("Content/ReceivedReward").gameObject.activeSelf) {
            for (int i = 0; i < AccountManager.Instance.mailRewardList.Count; i++) {
                if (AccountManager.Instance.mailRewardList[i].kind.Contains("Box")) {
                    if(AccountManager.Instance.mailRewardList[i].kind.CompareTo("supplyBox")==0) continue;
                    for (int j = 0; j < AccountManager.Instance.mailRewardList[i].boxes.Count; j++)
                        rewards.Add(AccountManager.Instance.mailRewardList[i].boxes[j]);
                }
            }
            if (rewards != null)
                boxRewardManager.OpenMultipleBoxes(rewards);
        }
        InitRewardList();
        transform.Find("Content/ReceivedReward").gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseReceiveResult);
    }

    IEnumerator SetReceiveResult(List<dataModules.MailReward> rewardList = null ) {
        transform.Find("Content/ReceivedReward/Buttons/Next").gameObject.SetActive(false);
        yield return SetRewardAnimation();

        Transform slotList = transform.Find("Content/ReceivedReward/RowSlot");
        List<dataModules.MailReward> itemList = new List<dataModules.MailReward>();
        if (rewardList != null)
            itemList = rewardList;
        else
            itemList = AccountManager.Instance.mailRewardList;

        for (int i = 0; i < AccountManager.Instance.mailRewardList.Count; i++) {
            slotList.GetChild(i / 3).gameObject.SetActive(true);
            slotList.GetChild(i / 3).GetChild(i % 3).gameObject.SetActive(true);
            slotList.GetChild(i / 3).GetChild(i % 3).Find("NameOrNum").GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;
            slotList.GetChild(i / 3).GetChild(i % 3).Find("Reward/Effect").gameObject.SetActive(false);
        }

        for (int i = 0; i < AccountManager.Instance.mailRewardList.Count; i++) {
            var effectObj = slotList.GetChild(i / 3).GetChild(i % 3).Find("Reward/Effect");
            effectObj.gameObject.SetActive(true);
            SkeletonGraphic skeletonGraphic = effectObj.GetChild(0).GetComponent<SkeletonGraphic>();

            skeletonGraphic.Initialize(false);
            skeletonGraphic.Update(0);
            skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);

            yield return new WaitForSeconds(0.1f);

            Transform target;
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
                slotList.GetChild(i / 3).GetChild(i % 3).Find("NameOrNum").GetComponent<TMPro.TextMeshProUGUI>().text = "x" + itemList[i].amount;
                target.GetComponent<Button>().onClick.RemoveAllListeners();
                target.GetComponent<Button>().onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModal(item));
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

            yield return new WaitForSeconds(0.3f);
            //slotList.GetChild(i / 3).GetChild(i % 3).Find("Reward/Effect").gameObject.SetActive(false);
            ///TODO : 아이템 수령 애니메이션 완료 직후 작업 (튜토리얼)
            if(tutoQuest != null && tutoQuest.pressed) {
                tutoQuest.quest.SubSet4();
                tutoQuest.received = true;
                PlayerPrefs.SetInt("FirstTutorialClear", 2);
                PlayerPrefs.Save();
                tutoQuest.quest.data.cleared = true;
            }
        }
        //slotList.GetChild(i / 3).GetChild(i % 3).Find("Reward/Effect").gameObject.SetActive(false);
    }

    private IEnumerator SetRewardAnimation() {
        Transform mailTransform = transform.Find("Content/ReceivedReward/Mail_Reward");
        SkeletonGraphic mail_animation = mailTransform.gameObject.GetComponent<SkeletonGraphic>();
        mail_animation.Initialize(false);
        mail_animation.Update(0);
        mail_animation.AnimationState.SetAnimation(0, "NOMAL", false);
        yield return new WaitForSeconds(0.6f);
    }


    void SetNextRewardPage(List<dataModules.MailReward> itemList) {
        InitRewardList();
        StartCoroutine(SetReceiveResult(itemList));
    }

    void InitRewardList() {
        Transform slotList = transform.Find("Content/ReceivedReward/RowSlot");
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                slotList.GetChild(i).GetChild(j).gameObject.SetActive(false);
                slotList.GetChild(i).GetChild(j).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
            slotList.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void CloseMail() {
        transform.Find("Content/OpenedMail").gameObject.SetActive(false);
        transform.Find("Content/OpenedMail/RecieveBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        for (int i = 0; i < transform.Find("Content/OpenedMail/Rewards").childCount; i++)
            transform.Find("Content/OpenedMail/Rewards").GetChild(i).gameObject.SetActive(false);
        if(EscapeKeyController.escapeKeyCtrl.escapeFunc.Contains(CloseMail))
            EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseMail);
    }
}

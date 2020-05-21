using System.Collections;
using System.Collections.Generic;
using Quest;
using UnityEngine;

public class MainWindowModalEffectManager : MonoBehaviour {
    [SerializeField] private MenuSceneController menuSceneController;
    private Queue<ModalData> modalQueue;
    public static MainWindowModalEffectManager Instance { get; private set; }

    private void Awake() {
        modalQueue = new Queue<ModalData>();
        Instance = this;
    }

    /// <summary>
    /// 모달 이벤트를 축적함
    /// </summary>
    /// <param name="targetModal">등장할 모달 객체</param>
    public void StackModal(GameObject targetModal, ModalType modalType, object args = null) {
        var modalData = new ModalData(modalType, targetModal);
        if (args != null) modalData.args = args;
        modalQueue.Enqueue(modalData);
    }
    
    void Start() {
        StartCoroutine(MainProceed());
    }

    IEnumerator MainProceed() {
        yield return new WaitForSeconds(1.0f);
        yield return new WaitUntil(() =>
            !menuSceneController.hideModal.activeSelf
            && !menuSceneController.storyLobbyPanel.activeSelf
            && !menuSceneController.battleReadyPanel.activeSelf
        );
        
        while (true) {
            yield return new WaitForSeconds(1.0f);
            if (modalQueue.Count > 0) {
                yield return Dequeueing();
            }
        }
    }

    IEnumerator Dequeueing() {
        var modalData = modalQueue.Dequeue();
        
        switch (modalData.modalType) {
            case ModalType.DAILY_QUEST:
                modalData.modal.SetActive(true);
                var data = (List<QuestData>) modalData.args;
                modalData.modal.GetComponent<DailyQuestAlarmHandler>().ShowQuestList(data);
                break;
            case ModalType.SOFT_RESET:
                var softResetData = (AccountManager.ClaimRewardResFormat) modalData.args;
                LeagueChangeModalHandler.Instance.OpenFirstWindow(softResetData);
                break;
        }
        
        yield return new WaitUntil(() => !modalData.modal.activeSelf);
    }

    public class ModalData {
        public ModalType modalType;
        public GameObject modal;
        public object args;

        public ModalData(ModalType modalType, GameObject modal) {
            this.modalType = modalType;
            this.modal = modal;
        }
    }
    
    public enum ModalType {
        DAILY_QUEST,         //일일 퀘스트 팝업
        ATTENDANCE_BOARD,    //출석판   :   코드 리뉴얼 이후에 종속시킬 필요가 있음
        SOFT_RESET           //소프트 리셋 공지
    }
}
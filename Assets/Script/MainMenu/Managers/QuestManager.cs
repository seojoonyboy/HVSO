using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;
using System;

namespace Quest {
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] GameObject QuestCanvas;
        [SerializeField] Transform content;
        [SerializeField] HUDController HUDController;

        public GameObject handSpinePrefab;
        private List<QuestContentController> quests;
        private Tutorials[] tutorialJson;

        public void OpenQuestCanvas() {
            HUDController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
            HUDController.SetBackButton(OnBackBtnClicked);

            EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackBtnClicked);
            QuestCanvas.SetActive(true);
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

        private void ReadFile() {
            string data = ((TextAsset)Resources.Load("TutorialDatas/questData")).text;
            tutorialJson = JsonReader.Read<Tutorials[]>(data);
        }

        private void OnDestroy() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, ShowQuest);
        }

        public void AddQuest(QuestData data) {
            QuestContentController quest = quests.Find(x=>!x.gameObject.activeSelf);
            quest.data = data;
            quest.manager = this;
            quest.gameObject.SetActive(true);
            if(data.cleared == false && data.tutorials == null) return;
            quest.ActiveTutorial();
        }

        public void ResetQuest() {
            quests.ForEach(x=>x.gameObject.SetActive(false));
        }

        private void ShowQuest(Enum type, Component Sender, object Param) {
            QuestData[] datas = (QuestData[])Param;
            ResetQuest();
            Array.ForEach(datas, x=>{
                Array.ForEach(tutorialJson, y=> {
                    if(x.id == y.id) x.tutorials = y.tutorials;
                });
            });
            Array.ForEach(datas, x=>AddQuest(x));
        }
    }

    [Serializable] public class QuestData {
        public int id;
        public string name;
        public string desc;
        public int progMax;
        public int prog;
        public bool cleared = false;
        public bool rewardGet = false;
        public TutorialShowList[] tutorials;
        public Reward[] rewards;
    }

    public class Reward {
        public string kind;
        public int amount;
    }

    public class Tutorials {
        public int id;
        public TutorialShowList[] tutorials;
    }

    public class TutorialShowList {
        public string method;
        public string[] args;
    }
}
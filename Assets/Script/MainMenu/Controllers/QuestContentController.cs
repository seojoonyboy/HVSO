using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Reflection;
using UnityEngine.Events;

namespace Quest {
    public class QuestContentController : MonoBehaviour {
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI info;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI sliderInfo;
        [SerializeField] private Button getBtn;
        [SerializeField] private Button rerollBtn;

        public QuestData data;
        public QuestManager manager;
        
        private void Start() {
            if(data == null) return;
            title.text = data.name;
            info.text = data.desc;
            slider.maxValue = (float)data.progMax;
            slider.value = (float)data.prog;
            sliderInfo.text = data.prog.ToString() + "/" + data.progMax.ToString();
            if(data.cleared) {
                getBtn.enabled = true;
                getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "획득하기";
            }
            else {
                getBtn.enabled = false;
                getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "진행중";
            }
        }

        private void GetReward() {
            gameObject.SetActive(false);
        }

        public void ActiveTutorial() {
            for(int i = 0; i < data.tutorials.Length; i++) {
                MethodInfo theMethod = this.GetType().GetMethod(data.tutorials[i].method);
                object[] args = new object[]{data.tutorials[i].args};
                theMethod.Invoke(this, args);
            }
        }

        public void ShowStoryHand(string[] args) {
            
        }

        /// <summary>
        /// 메뉴 화면 카드 도감 휴먼 영웅에 손가락 표시
        /// </summary>
        /// <param name="args"></param>
        public void MenuDictionaryShowHand(string[] args) {
            MenuSceneController menu = MenuSceneController.menuSceneController;
            menu.DictionaryShowHand(this, args);
        }

        /// <summary>
        /// 도감 메뉴 해당 카드에 손가락 표시
        /// cardManager.cardShowHand 함수 별개로 만들어서 거기 안에 손가락 생성 별도로 만들게 하기
        /// </summary>
        /// <param name="args">해당 카드 id</param>
        public void DictionaryCardHand(string[] args) {
            CardDictionaryManager cardManager = CardDictionaryManager.cardDictionaryManager;
            cardManager.cardShowHand(this, args);
        }

        /// <summary>
        /// 카드 생성 됐을 때 확인 하는 용도
        /// </summary>
        /// <param name="args">해당 카드 id</param>
        public void CreateCardCheck(string[] args) {
            OnEvent theEvent = null;
            theEvent = (type, Sender, Param) => {
                var card = Array.Find(AccountManager.Instance.myCards, x=>x.cardId.CompareTo(args[0])==0);
                if(card == null) return;
                if(card.cardCount != 4) return;
                createCardDone();
                NoneIngameSceneEventHandler.Instance.RemoveListener(
                    NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, 
                    theEvent
                );
            };
            NoneIngameSceneEventHandler.Instance.AddListener(
                NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, 
                theEvent);
        }

        private void createCardDone() {
            GameObject hand;
            while(true) {
                hand = GameObject.Find("tutorialHand");
                if(hand == null) break;
                DestroyImmediate(hand);
            }
            //튜토리얼 완료
            data.cleared = true;
            data.prog = 1;
            slider.value = slider.maxValue;
            sliderInfo.text = "1/1";
            MenuSceneController menu = MenuSceneController.menuSceneController;
            menu.DictionaryRemoveHand();
            AccountManager.Instance.RequestQuestInfo();
        }
    }
}
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

//TODO : 튜토리얼이 아닌 시나리오 플레이시 challenge 리스트를 수정해줄 필요가 있음.
public class ChallengerHandler : SerializedMonoBehaviour {
    public List<Challenge> challenges;
    [SerializeField] GameObject challengeUI;
    TextMeshProUGUI text;
    GameObject check;
    Image textShadow;
    [SerializeField] Sprite[] textShadowImages;
    IngameEventHandler eventHandler;
    void Start() {
        if(challengeUI == null) {
            PrintLogError("UI가 설정되어 있지 않습니다.");
            return;
        }
        text = challengeUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        check = challengeUI.transform.Find("Check/Image").gameObject;
        textShadow = challengeUI.transform.Find("Shadow").GetComponent<Image>();

        eventHandler = PlayMangement.instance.EventHandler;
    }

    /// <summary>
    /// 이벤트 리스너 등록
    /// </summary>
    public void AddListener(string eventName) {
        IngameEventHandler.EVENT_TYPE type;
        switch (eventName) {
            case "UNIT_SUMMONED":
                type = IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED;
                break;
            default:
                type = IngameEventHandler.EVENT_TYPE.DO_NOTHING;
                break;
        }
        eventHandler.AddListener(type, OnEventOccured);

        ShowChallenge();
    }

    private void OnEventOccured(Enum Event_Type, Component Sender, object Param) {
        ProceedChallenge();
    }

    public void RemoveListener(string eventName) {
        IngameEventHandler.EVENT_TYPE type;
        switch (eventName) {
            case "UNIT_SUMMONED":
                type = IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED;
                break;
            default:
                type = IngameEventHandler.EVENT_TYPE.DO_NOTHING;
                break;
        }
        eventHandler.RemoveListener(type, OnEventOccured);
    }

    public void ShowChallenge() {
        if (!challengeUI.activeSelf) challengeUI.SetActive(true);
        if (challenges.Count == 0) return;

        Challenge challenge = challenges[0];
        if (check.activeSelf) check.SetActive(false);
        text.text = challenge.content + "(" + challenge.currentNum + "/" + challenge.targetNum + ")";
    }

    /// <summary>
    /// 챌린지 진행함
    /// </summary>
    /// <param name="id">해당 챌린지의 id</param>
    public void ProceedChallenge() {
        if(challengeUI == null) {
            PrintLogError("UI가 설정되어 있지 않습니다.");
            return;
        }

        if (challenges.Count == 0) return;

        Challenge challenge = challenges[0];
        if (challenge == null) return;

        challenge.currentNum++;
        text.text = challenge.content + "(" + challenge.currentNum + "/" + challenge.targetNum + ")";

        //미션 달성
        if (challenge.currentNum == challenge.targetNum) {
            StartCoroutine(CompleteChallenge());
        }
    }

    /// <summary>
    /// 챌린지 완료
    /// </summary>
    IEnumerator CompleteChallenge() {
        check.SetActive(true);
        textShadow.sprite = textShadowImages[1];

        yield return new WaitForSeconds(2.0f);
        if (challenges.Count != 0) {
            challenges.Remove(challenges[0]);
        }
        else {
            Logger.Log("챌린지 모두 완료");
        }
        yield return new WaitForSeconds(1.5f);
        check.SetActive(false);
        challengeUI.SetActive(false);
    }

    private void PrintLogError(string msg) {
        Logger.LogError(msg);
    }

    public class Challenge {
        public int id;              //고유 번호
        public string content;      //내용
        public int targetNum;       //목표
        public int currentNum;      //현재 진척도
    }
}

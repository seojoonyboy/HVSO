using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//TODO : 튜토리얼이 아닌 시나리오 플레이시 challenge 리스트를 수정해줄 필요가 있음.
public class ChallengerHandler : SerializedMonoBehaviour {
    public List<Challenge> challenges;
    [SerializeField] GameObject challengeUI;
    TextMeshProUGUI text;
    GameObject check;

    void Start() {
        if(challengeUI == null) {
            PrintLogError("UI가 설정되어 있지 않습니다.");
            return;
        }
        text = challengeUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        check = challengeUI.transform.Find("Check").gameObject;
    }

    /// <summary>
    /// 챌린지 진행함
    /// </summary>
    /// <param name="id">해당 챌린지의 id</param>
    public void ProceedChallenge(int id) {
        if(challengeUI == null) {
            PrintLogError("UI가 설정되어 있지 않습니다.");
            return;
        }

        Challenge challenge = challenges.Find(x => x.id == id);
        if (challenge == null) return;

        challenge.currentNum++;
        text.text = challenge.content + "(" + challenge.currentNum + "/" + challenge.targetNum + ")";
        if(challenge.currentNum == challenge.targetNum) {
            CompleteChallenge(challenge);
        }
    }

    /// <summary>
    /// 챌린지 완료
    /// </summary>
    public void CompleteChallenge(Challenge challenge) {
        check.SetActive(true);
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

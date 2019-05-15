using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIModule {
    public class LoadingTextEffect : MonoBehaviour {
        IEnumerator coroutine;
        Text targetText;
        Text timeText;

        string[] effectArr = new string[] { "불러오는 중...", "불러오는 중..", "불러오는 중." };

        // Start is called before the first frame update
        void Awake() {
            timeText = transform.Find("Time").GetComponent<Text>();
            targetText = transform.Find("Text").GetComponent<Text>();
        }

        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        void OnEnable() {
            coroutine = Effect();
            StartCoroutine(coroutine);
        }

        void OnDisable() {
            StopCoroutine(coroutine);
            timeText.text = "";
            targetText.text = "";
        }

        IEnumerator Effect() {
            int cnt = 0;
            while (true) {
                if (cnt >= 60) break;
                timeText.text = "대기시간..." + cnt + "초";
                targetText.text = effectArr[cnt % 3];
                yield return new WaitForSeconds(1.0f);
                cnt++;
            }
            timeText.text = "네트워크 오류가 발생하였습니다.";
        }

        public void OffEffect() {
            StopCoroutine(coroutine);
        }
    }
}
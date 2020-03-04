using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TextTyping : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI textObj;
    string typingText;
    private IEnumerator textmeshTyping;
    ScenarioExecuteHandler scenarioHandler;
    MenuExecuteHandler menuExecuteHandler;
    bool isTyping;

    public void StartTyping(string text, ScenarioExecuteHandler handler) {
        if (isTyping) return;
        typingText = text.Replace("<br> ", "|");
        scenarioHandler = handler;
        menuExecuteHandler = null;
        textmeshTyping = TypeTextMesh();
        StartCoroutine(textmeshTyping);
    }

    public void StartTyping(string text, MenuExecuteHandler handler) {
        if (isTyping) return;
        typingText = text.Replace("<br> ", "|");
        scenarioHandler = null;
        menuExecuteHandler = handler;
        textmeshTyping = TypeTextMesh();
        StartCoroutine(textmeshTyping);
    }


    public IEnumerator TypeTextMesh(){
        isTyping = true;
        textObj.text = "";
        SoundManager.Instance.PlayIngameSfx(IngameSfxSound.TEXTTYPING);

        //typingText = "<color=red>퀘스트 보상</color>은 <color=green>우편</color>으로 발송되니 우편함을 확인해 봐야겠어.";
        string pattern = @"(\<[a-zA-Z]+[^>]+[\>])([^<]*)(\<\/[a-zA-Z]+\>)";
        string sentence = typingText;
        var matches = Regex.Matches(sentence, pattern);
        var result = Regex.Split(typingText, pattern);
        List<string> list = new List<string>();
        for(int i=0; i<result.Length; i++) {
            if (i % 2 == 0) list.Add(result[i]);
            else {
                if (result[i].Contains("<color")) {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(result[i]);
                    sb.Append(result[i + 1]);
                    sb.Append(result[i + 2]);
                    list.Add(sb.ToString());
                    i += 2;

                    if (i > result.Length) {
                        list.Add(result[i + 3]);
                        break;
                    }
                }
            }
        }

        foreach(string str in list) {
            if (str.Contains("<color")) {
                textObj.text += str;
            }
            else {
                int num = str.Length;
                int count = 0;
                while (true) {
                    if (count < num) {
                        textObj.text += str[count];
                        count++;
                    }
                    else break;
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        
        isTyping = false;

        if (scenarioHandler != null)
            scenarioHandler.isDone = true;
        if (menuExecuteHandler != null)
            menuExecuteHandler.isDone = true;
        
        transform.Find("StopTypingTrigger").gameObject.SetActive(false);
    }

    public void StopTyping() {
        if(textmeshTyping != null)
            StopCoroutine(textmeshTyping);
        textObj.text = typingText.Replace("|", "<br>");
        isTyping = false;
        if (scenarioHandler != null)
            scenarioHandler.isDone = true;
        if (menuExecuteHandler != null)
            menuExecuteHandler.isDone = true;
        transform.Find("StopTypingTrigger").gameObject.SetActive(false);
    }

    public class RegMatch {
        public int start;
        public int length;

        public RegMatch(int start, int length) {
            this.start = start;
            this.length = length;
        }
    }
}

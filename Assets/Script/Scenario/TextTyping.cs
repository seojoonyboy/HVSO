using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTyping : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI textObj;
    string typingText;
    private IEnumerator textmeshTyping;
    ScenarioExecuteHandler scenarioHandler;
    bool isTyping;

    public void StartTyping(string text, ScenarioExecuteHandler handler) {
        if (isTyping) return;
        typingText = text.Replace("<br> ", "<br>");
        scenarioHandler = handler;
        textmeshTyping = TypeTextMesh();
        StartCoroutine(textmeshTyping);
    }

    public IEnumerator TypeTextMesh(){
        isTyping = true;
        int num = typingText.Length;
        int count = 0;
        textObj.text = "";
        while(true) {
            if (count < num) {
                if (typingText[count].ToString() == "<") {
                    if (typingText[count + 1].ToString() == "b" && typingText[count + 2].ToString() == "r") {
                        textObj.text += "<br>";
                        count += 4;
                    }
                }
                else {
                    textObj.text += typingText[count];
                    count++;
                }
            }
            else break;
            yield return new WaitForSeconds(0.05f);
        }
        isTyping = false;
        scenarioHandler.isDone = true;
        transform.Find("StopTypingTrigger").gameObject.SetActive(false);
    }

    public void StopTyping() {
        if(textmeshTyping != null)
            StopCoroutine(textmeshTyping);
        textObj.text = typingText;
        isTyping = false;
        scenarioHandler.isDone = true;
        transform.Find("StopTypingTrigger").gameObject.SetActive(false);
    }
}

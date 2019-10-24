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
        int num = typingText.Length;
        int count = 0;
        textObj.text = "";
        while(true) {
            SoundManager.Instance.PlayIngameSfx("TextTyping");
            if (count < num) {
                if (typingText[count].ToString() == "|") 
                    textObj.text += "<br>";
                else 
                    textObj.text += typingText[count];
                count++;
            }
            else break;            
            yield return new WaitForSeconds(0.02f);
        }
        isTyping = false;

        if (scenarioHandler != null)
            scenarioHandler.isDone = true;
        if (menuExecuteHandler != null)
            menuExecuteHandler.isDone = true;

#if UNITY_EDITOR
        transform.Find("StopTypingTrigger").gameObject.SetActive(false);
#endif
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
#if UNITY_EDITOR
        transform.Find("StopTypingTrigger").gameObject.SetActive(false);
#endif
    }
}

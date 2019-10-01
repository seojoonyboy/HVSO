using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTyping : MonoBehaviour
{
    TMPro.TextMeshProUGUI textObj;
    string typingText;
    private IEnumerator textmeshTyping;
    bool isTyping;

    public void StartTyping(GameObject obj, string text) {
        if (isTyping) return;
        textObj = obj.GetComponent<TMPro.TextMeshProUGUI>();
        typingText = text;
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
                textObj.text += typingText[count];
                count++;
            }
            else break;
            yield return new WaitForSeconds(0.05f);
        }
        isTyping = false;
    }

    public void StopTyping() {
        if(textmeshTyping != null)
            StopCoroutine(textmeshTyping);
        textObj.text = typingText;
        isTyping = false;
    }
}

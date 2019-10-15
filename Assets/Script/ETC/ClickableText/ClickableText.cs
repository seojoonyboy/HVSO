using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ClickableText : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    private TextMeshProUGUI text;
    private bool modalOn;
    public TextMeshProUGUI modalText;

    private void Awake() {
        text = GetComponent<TextMeshProUGUI>();
        modalOn = false;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if(modalOn) return;
        modalOn = true;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

        if (linkIndex > -1) {
            var linkInfo = text.textInfo.linkInfo[linkIndex];
            var linkId = linkInfo.GetLinkID();

            Logger.Log("linkInfo : " + linkInfo);
            Logger.Log("linkId : " + linkId);

            modalText.text = linkId + "에 대한 설명입니다.";
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        modalText.text = "";
        modalOn = false;
    }
}

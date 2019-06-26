using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ClickableText : MonoBehaviour, IPointerClickHandler {
    public void OnPointerClick(PointerEventData eventData) {
        var text = GetComponent<TextMeshProUGUI>();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

        Logger.Log("Clicked");

        if (linkIndex > -1) {
            var linkInfo = text.textInfo.linkInfo[linkIndex];
            var linkId = linkInfo.GetLinkID();

            Logger.Log("linkInfo : " + linkInfo);
            Logger.Log("linkId : " + linkId);

            Modal.instantiate(linkId + "에 대한 설명입니다.", Modal.Type.CHECK);
        }
    }
}

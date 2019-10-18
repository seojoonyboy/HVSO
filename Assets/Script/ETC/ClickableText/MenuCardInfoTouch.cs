using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public partial class MenuCardInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    [SerializeField] private TextMeshProUGUI dialogText;
    private bool modalOn = false;

    public void OnPointerDown(PointerEventData eventData) {
        if(modalOn) return;
        modalOn = true;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(dialogText, Input.mousePosition, null);

        if (linkIndex > -1) {
            var linkInfo = dialogText.textInfo.linkInfo[linkIndex];
            var linkId = linkInfo.GetLinkID();
            var data = translator.GetTranslatedSkillTypeDesc(linkId);
            OpenClassDescModal(linkId, accountManager.resource.skillIcons[linkId]);
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        CloseClassDescModal();
        modalOn = false;
    }
}

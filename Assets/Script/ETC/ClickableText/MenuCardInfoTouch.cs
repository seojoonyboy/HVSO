using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public partial class MenuCardInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    [SerializeField] private TextMeshProUGUI dialogText;
    private bool _modalOn = false;

    public void OnPointerDown(PointerEventData eventData) {
        if(_modalOn) return;
        _modalOn = true;

        var linkIndex = TMP_TextUtilities.FindIntersectingLink(dialogText, Input.mousePosition, null);

        if (linkIndex <= -1) return;
        var linkInfo = dialogText.textInfo.linkInfo[linkIndex];
        var linkId = linkInfo.GetLinkID();
        translator.GetTranslatedSkillTypeDesc(linkId);
        OpenClassDescModal(linkId, accountManager.resource.GetSkillIcons(linkId));
    }
    public void OnPointerUp(PointerEventData eventData) {
        CloseClassDescModal();
        _modalOn = false;
    }
}

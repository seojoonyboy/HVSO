using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoOnDrag : MonoBehaviour
{
    private static CardInfoOnDrag _instance;
    public static CardInfoOnDrag instance {
        get {
            if (_instance == null) {
                _instance = (CardInfoOnDrag)FindObjectOfType(typeof(CardInfoOnDrag));
                if (_instance == null) {
                    Logger.LogWarning("아직 준비가 안됐습니다. 양해바랍니다.");
                }
            }
            return _instance;
        }
    }

    public void Awake() {
        _instance = this;
        gameObject.SetActive(false);
        leftEdge.position = Camera.main.ScreenToWorldPoint(new Vector3(9, 0, 0));
        rightEdge.position = -leftEdge.position;
    }

    public void OnDestroy() {
        _instance = null;
    }

    [SerializeField] Transform leftEdge;
    [SerializeField] Transform rightEdge;
    float xWidth;

    public void SetCardDragInfo(string info, Vector3 cardPos, string skillInfo = null) {
        leftEdge.position = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        rightEdge.position = -leftEdge.position;
        transform.localPosition = new Vector3(cardPos.x, cardPos.y + 170, 0);
        xWidth = transform.GetComponent<RectTransform>().sizeDelta.x / 2.0f;
        if (skillInfo.Length != 0) {
            ResizeBox(skillInfo.Length);
            transform.Find("SkillText").GetComponent<TMPro.TextMeshProUGUI>().text = skillInfo;
        }
        gameObject.SetActive(true);
    }

    private void ResizeBox(int textLength) {
        int lineNum = textLength / 19;
        transform.GetComponent<RectTransform>().sizeDelta = transform.Find("SkillText").GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100 + (50 * lineNum));    
    }

    public void SetInfoPosOnDrag(Vector3 cardPos) {
        float xPos = 0;
        float yPos = 0;
        if (cardPos.x + xWidth < rightEdge.localPosition.x && cardPos.x - xWidth > leftEdge.localPosition.x)
            xPos = cardPos.x;
        else
            xPos = transform.localPosition.x;
        if (cardPos.y + 530 < rightEdge.localPosition.y)
            yPos = cardPos.y + 170;
        else
            yPos = cardPos.y - 170;
        transform.localPosition = new Vector3(xPos, yPos, 0);

    }

    public void OffCardDragInfo() {
        gameObject.SetActive(false);
    }
}

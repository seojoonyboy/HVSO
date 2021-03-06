using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockerController : MonoBehaviour
{
    [SerializeField] public GameObject touchBlocker;
    [SerializeField] Transform blockerCanvas;
    [SerializeField] Transform upBlock;
    [SerializeField] Transform downBlock;
    [SerializeField] Transform rightBlock;
    [SerializeField] Transform leftBlock;
    public static BlockerController blocker;

    private void Start() {
        blocker = this;
        gameObject.SetActive(false);
    }

    public void SetBlocker(GameObject target = null) {
        gameObject.SetActive(true);
        OnBlocks(false);
        transform.SetParent(target.transform);
        transform.localPosition = Vector3.zero;
        transform.SetParent(blockerCanvas);
        transform.localScale = Vector3.one;
        OnBlocks(true);
        float width = target != null ? target.GetComponent<RectTransform>().sizeDelta.x / 2 : 0;
        float height = target != null ? target.GetComponent<RectTransform>().sizeDelta.y / 2 : 0;

        if(width == 0 && height == 0) {
            width = target != null ? target.GetComponent<RectTransform>().rect.width / 2 : 0;
            height = target != null ? target.GetComponent<RectTransform>().rect.height / 2 : 0;
        }

        upBlock.localPosition = new Vector2(0, 1500 + height);
        downBlock.localPosition = new Vector2(0, -(1500 + height));
        rightBlock.localPosition = new Vector2(1500 + width, 0);
        leftBlock.localPosition = new Vector2(-(1500 + width), 0);
        touchBlocker.SetActive(false);
    }

    void OnBlocks(bool onBlock) {
        upBlock.gameObject.SetActive(onBlock);
        downBlock.gameObject.SetActive(onBlock);
        rightBlock.gameObject.SetActive(onBlock);
        leftBlock.gameObject.SetActive(onBlock);
    }
}

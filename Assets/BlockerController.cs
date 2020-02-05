using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockerController : MonoBehaviour
{
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

    public void SetBlocker(GameObject target) {
        gameObject.SetActive(true);
        OnBlocks(false);
        transform.SetParent(target.transform);
        transform.localPosition = new Vector3(0, 0, 0);
        transform.SetParent(blockerCanvas);
        OnBlocks(true);

        float width = target.GetComponent<RectTransform>().sizeDelta.x / 2;
        float height = target.GetComponent<RectTransform>().sizeDelta.y / 2;
        upBlock.localPosition = new Vector2(0, 1500 + height);
        downBlock.localPosition = new Vector2(0, -(1500 + height));
        rightBlock.localPosition = new Vector2(1500 + width, 0);
        leftBlock.localPosition = new Vector2(-(1500 + width), 0);
    }

    void OnBlocks(bool onBlock) {
        upBlock.gameObject.SetActive(onBlock);
        downBlock.gameObject.SetActive(onBlock);
        rightBlock.gameObject.SetActive(onBlock);
        leftBlock.gameObject.SetActive(onBlock);
    }

    public void BlockTouch() {
        touchBlocker.SetActive(true);
        gameObject.SetActive(false);
    }
}

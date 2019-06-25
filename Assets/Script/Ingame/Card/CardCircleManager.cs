using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCircleManager : MonoBehaviour {
    public int cardNum = 0;
    [SerializeField] Transform cardStorage;
    [SerializeField] Transform showPos;
    public bool dragable = true;
    public bool isMultiple = false;


    // Update is called once per frame

    public void AddCard() {
        if (cardNum == 10) return;
        dragable = false;
        cardNum++;
        Transform cardTransform = cardStorage.GetChild(0);
        Transform cardPos = transform.GetChild(cardNum - 1).GetChild(0);
        cardTransform.GetComponent<CardDragAndDrop>().CARDINDEX = cardNum - 1;
        cardTransform.gameObject.SetActive(true);
        cardTransform.SetParent(transform.GetChild(cardNum - 1));
        StartCoroutine(SendCardToHand(cardTransform.gameObject, cardPos));
    }

    public void AddMultipleCard() {
        isMultiple = true;
        StartCoroutine(SendMultipleCard(4));
    }

    IEnumerator SendMultipleCard(int num) {
        for (int i = 0; i < num; i++) {
            if (cardNum == 10) break;
            dragable = false;
            cardNum++;
            Transform cardTransform = cardStorage.GetChild(0);
            Transform cardPos = transform.GetChild(cardNum - 1).GetChild(0);
            cardTransform.GetComponent<CardDragAndDrop>().CARDINDEX = cardNum - 1;
            cardTransform.gameObject.SetActive(true);
            cardTransform.SetParent(transform.GetChild(cardNum - 1));
            StartCoroutine(SendCardToHand(cardTransform.gameObject, cardPos));
            yield return new WaitForSeconds(0.3f);
            if (i == num - 1 || cardNum == 10)
                isMultiple = false;
        }
    }


    IEnumerator SendCardToHand(GameObject card, Transform pos) {
        iTween.MoveTo(card, showPos.position, 0.2f);
        iTween.RotateTo(card, iTween.Hash("rotation", new Vector3(0, 0, 0), "islocal", true, "time", 0.2f));
        iTween.ScaleTo(card, new Vector3(1.2f, 1.2f, 1), 0.2f);
        iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 1) * 4), 0.2f);
        yield return new WaitForSeconds(0.2f);
        //card.transform.SetParent(transform.GetChild(cardNum - 1));
        iTween.MoveTo(card, iTween.Hash("position", new Vector3(0, 4500, 0), "islocal", true, "time", 0.2f));
        iTween.ScaleTo(card, new Vector3(1, 1, 1), 0.2f);
        yield return new WaitForSeconds(0.2f);
        card.transform.rotation = pos.rotation;
        if (cardNum > 2 && !isMultiple)
            iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 2) * 4), 0.2f);
        yield return new WaitForSeconds(0.3f);
        dragable = true;
    }

    public IEnumerator UseCard(int index) {
        dragable = false;
        Transform removeCard = transform.GetChild(index).GetChild(1);
        iTween.RotateTo(transform.GetChild(index).gameObject, iTween.Hash("rotation", new Vector3(0, 0, -4 * (cardNum - 1)), "islocal", true, "time", 0.2f));
        transform.GetChild(index).SetSiblingIndex(cardNum - 1);
        removeCard.SetParent(cardStorage);
        removeCard.gameObject.SetActive(false);
        removeCard.eulerAngles = Vector3.zero;
        removeCard.localPosition = Vector3.zero;

        cardNum--;
        for (int i = index; i < cardNum; i++) {
            transform.GetChild(i).GetChild(1).GetComponent<CardDragAndDrop>().CARDINDEX = i;
            iTween.RotateTo(transform.GetChild(i).gameObject, iTween.Hash("rotation", new Vector3(0, 0, -4 * i), "islocal", true, "time", 0.2f));
        }
        StartCoroutine(SortCircleAngle());
        yield return new WaitForSeconds(0.3f);
        //dragable = true;
    }

    public IEnumerator SortCircleAngle() {
        dragable = false;
        Debug.Log(transform.rotation.eulerAngles.z);
        if (transform.rotation.eulerAngles.z > 300 && transform.rotation.eulerAngles.z < 360)
            iTween.RotateTo(gameObject, new Vector3(0, 0, 4), 0.2f);
        else if (transform.rotation.eulerAngles.z < 4 && cardNum > 2)
            iTween.RotateTo(gameObject, new Vector3(0, 0, 4), 0.2f);
        else if (transform.rotation.eulerAngles.z < 60 && transform.rotation.eulerAngles.z > (cardNum - 2) * 4 && cardNum > 0)
            iTween.RotateTo(gameObject, new Vector3(0, 0, (cardNum - 1) * 4), 0.2f);
        yield return new WaitForSeconds(0.3f);
        dragable = true;
    }
}

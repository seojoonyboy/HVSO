using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class CardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector3 startPos;
    public GameObject unit;
    public Sprite unitSprite;
    private bool blockButton = false;
    CardListManager csm;
    Animator cssAni;
    private string cardID;

    public CardData cardData;
    public CardDataPackage cardDataPackage;

    public void Start() {
    }

    public void DrawCard(string ID) {
        cardDataPackage = Resources.Load("CardDatas/CardDataPackage_01") as CardDataPackage;
        cardID = ID;

        if (cardDataPackage.data.ContainsKey(cardID)) {
            cardData = cardDataPackage.data[cardID];
            unitSprite = Resources.Load <Sprite> ("Sprite/" + cardID);
        }
        else
            Debug.Log("NoData");

        transform.Find("Health").Find("Text").GetComponent<Text>().text = cardData.hp.ToString();
        transform.Find("attack").Find("Text").GetComponent<Text>().text = cardData.attack.ToString();
        transform.Find("Cost").Find("Text").GetComponent<Text>().text = cardData.cost.ToString();

        csm = GameObject.Find("Canvas").transform.GetChild(3).GetComponent<CardListManager>();
        csm.AddCardInfo(cardData);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {

        blockButton = true;
        startPos = transform.position;
        PlayMangement.instance.player.isPicking.Value = true;
    }

    public void OnDrag(PointerEventData eventData)
    {

        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        transform.position = startPos;
        blockButton = false;
        PlayMangement.instance.player.isPicking.Value = false;
    }

    public void OpenCardInfoList() {
        if (!blockButton) {
            csm.OpenCardList(transform.GetSiblingIndex());
        }
    }

    public void DisableCard() {
        gameObject.transform.Find("Portrait").GetComponent<Image>().color = Color.gray;
        gameObject.transform.Find("attack").GetComponent<Image>().color = Color.gray;
        gameObject.transform.Find("Health").GetComponent<Image>().color = Color.gray;
        gameObject.transform.Find("Cost").GetComponent<Image>().color = Color.gray;
    }

    public void ActivateCard() {
        gameObject.transform.Find("Portrait").GetComponent<Image>().color = Color.white;
        gameObject.transform.Find("attack").GetComponent<Image>().color = Color.white;
        gameObject.transform.Find("Health").GetComponent<Image>().color = Color.white;
        gameObject.transform.Find("Cost").GetComponent<Image>().color = Color.white;
    }


}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class EditCardHandler : MonoBehaviour, IPointerDownHandler,  IPointerClickHandler
{
    public string cardID;
    public DeckEditController deckEditController;
    public SelectCard cardgroup;

    public bool clicking = false;
    public float time = 0f;

    private void Update() {        
        if(clicking == true) {
            time += Time.deltaTime; 

            if(time > 1f) {
                clicking = false;
                ShowInfo();
                time = 0;
            }
        }
    }

    public void CardSet() {
        deckEditController.OnTouchCard(cardgroup);
    }

    public void ExepctBtn() {
        deckEditController.ExpectFromDeck();
    }

    public void ShowInfo() {
        Debug.Log("테스팅!");
        
    }

    public void OnPointerDown(PointerEventData eventData) {
        clicking = true;
    }


    public void OnPointerClick(PointerEventData eventData) {
        if (clicking == false) return;

        clicking = false;
        CardSet();
        time = 0;
    }
}

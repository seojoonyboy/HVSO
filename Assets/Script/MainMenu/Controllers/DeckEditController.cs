using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckEditController : MonoBehaviour
{
    CardData card;

    public void SettingNewDeck() {
    }

    public void SettingEditDeck() {
    }    

}


/*
public interface LoadEdit {
    void LoadCardData();
}


public class NewDeck : MonoBehaviour, LoadEdit {

    public void LoadCardData() {

    }
}

public class EditDeck : MonoBehaviour, LoadEdit {

    public void LoadCardData() {

    }
}

public class GetDeck : MonoBehaviour {

    private LoadEdit loadEdit;    

    public void Execute(LoadEdit deck) {
        loadEdit = deck;

        if (loadEdit == null)
            Logger.Log("에러!");
        else
            loadEdit.LoadCardData();
    }
}
*/

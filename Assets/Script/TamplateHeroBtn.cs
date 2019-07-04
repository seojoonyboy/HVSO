using UnityEngine;

public class TamplateHeroBtn : MonoBehaviour
{
    public TamplateMenu menu;
    public string heroID;

    public void HeroSelectBtn() {
        menu.ChangeHeroID(heroID);
    }
}

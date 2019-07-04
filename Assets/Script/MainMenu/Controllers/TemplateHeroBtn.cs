using UnityEngine;

public class TemplateHeroBtn : MonoBehaviour
{
    public TemplateMenu menu;
    public string heroID;

    public void HeroSelectBtn() {
        menu.ChangeHeroID(heroID);
    }
}

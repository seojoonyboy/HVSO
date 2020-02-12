namespace SkillModules {
    public class poison : UnitAttribute { 
        private void Start() {
            gameObject.GetComponent<PlaceMonster>().AddAttackProperty("poison");
        }

        void OnDestroy() {
            gameObject.GetComponent<PlaceMonster>().ChangeAttackProperty();
        } 
    }
}

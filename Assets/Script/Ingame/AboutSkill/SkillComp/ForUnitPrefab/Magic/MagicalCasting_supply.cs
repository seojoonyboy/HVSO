using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_supply : MagicalCasting {
        public override void RequestUseMagic() {
            isRequested = true;
        }

        public override void UseMagic() {
            IEnumerable<string> query = from effect in skillData.effects.ToList()
                                        select effect.args[0];
            int drawNum = 0;
            int.TryParse(query.ToList()[0], out drawNum);

            Debug.Log("마법 카드를 사용하여 " + drawNum + "장 드로우");
            GetComponent<MagicDragHandler>().AttributeUsed(GetComponent<MagicalCasting_supply>());
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTypeToggleHandler : ToggleHandler {
    public override void OnValueChanged() {
        base.OnValueChanged();

        if (toggle.isOn) {
            var type = (BattleReadySceneController.RaceType)id;
            controller.ChangeRaceType(type);

            ;
            switch (type) {
                case BattleReadySceneController.RaceType.HUMAN:

                    break;
                case BattleReadySceneController.RaceType.ORC:

                    break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTypeToggleHandler : ToggleHandler {
    public override void OnValueChanged() {
        base.OnValueChanged();
        if (toggle.isOn) {
            var type = (BattleReadySceneController.BattleType)id;
            controller.ChangeBattleType(type);
        }
    }
}

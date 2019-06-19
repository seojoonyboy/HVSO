using dataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class MagicalCasting : MonoBehaviour {
        protected Skill skillData;
        public bool isPlayer = true;

        public bool isRequested = false;
        /// <summary>
        /// 소켓에게 요청을 보낼 준비가 된 경우 isRequest를 True로 바꾼다.
        /// </summary>
        public virtual void RequestUseMagic() { }

        /// <summary>
        /// 소켓요청이 성공적으로 끝난 경우 호출된다. 실제 역할 수행
        /// </summary>
        public virtual void UseMagic() { }

        public virtual void InitData(Skill data, bool isPlayer) {
            skillData = data;
            this.isPlayer = isPlayer;
        }

        protected bool IsSubConditionValid(bool isPlayer, GameObject playedObj) {
            var checkers = GetComponents<ConditionChecker>();

            foreach(ConditionChecker checker in checkers) {
                if (!checker.IsConditionSatisfied(isPlayer, playedObj)) {
                    Debug.Log("최종 조건 만족이 안됨");
                    return false;
                }
            }
            Debug.Log("모든 조건 충족됨");
            return true;
        }
    }

}

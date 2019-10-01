using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuffHandler : MonoBehaviour {
    GameObject buffContinue, debuffContinue;
    public List<BuffStat> buffList;
    PlaceMonster placeMonster;
    int origin_atk, origin_hp;

    void Awake() {
        placeMonster = GetComponent<PlaceMonster>();
        origin_atk = placeMonster.unit.originalAttack;
        origin_hp = placeMonster.unit.HP;

        buffList = new List<BuffStat>();
    }

    private void Refresh() {
        int total_buffed_atk = origin_atk;
        int total_buffed_hp = origin_hp;

        foreach(BuffStat stat in buffList) {
            total_buffed_atk += stat.atk;
            total_buffed_hp += stat.hp;
        }

        //버프 이펙트 보여주기
        if(total_buffed_hp > origin_hp || total_buffed_atk > origin_atk) {
            if(buffContinue == null) {
                buffContinue = EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_BUFF, transform);
            }
        }

        //디버프 이펙트 보여주기
        if(total_buffed_hp < origin_hp || total_buffed_atk < origin_atk) {
            if(debuffContinue == null) {
                debuffContinue = EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_DEBUFF, transform);
            }
        }

        //버프 이펙트 보여주기 해제
        if(total_buffed_atk == origin_atk && total_buffed_hp == origin_hp){
            Destroy(buffContinue);
            Destroy(debuffContinue);
        }
    }

    public void AddBuff(BuffStat stat) {
        buffList.Add(stat);
        Refresh();
    }

    public void RemoveBuff(BuffStat stat) {
        var result = buffList.Find(x => x.atk == stat.atk && x.hp == stat.hp);
        if(result.atk != 0 && result.hp != 0) {
            buffList.Remove(result);
        }
    }

    public void RemoveAllBuff() {
        buffList.Clear();
        Refresh();
    }


    public struct BuffStat {
        public int atk;
        public int hp;

        public BuffStat(int atk, int hp) {
            this.atk = atk;
            this.hp = hp;
        }
    }
}

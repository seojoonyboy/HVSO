using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuffHandler : MonoBehaviour {
    [SerializeField] GameObject buffContinue, debuffContinue;
    public List<BuffStat> buffList;
    PlaceMonster placeMonster;
    int origin_atk, origin_hp;

    void Awake() {
        buffList = new List<BuffStat>();
    }

    private void Refresh() {
        placeMonster = GetComponent<PlaceMonster>();
        int total_buffed_atk = placeMonster.unit.originalAttack;
        int total_buffed_hp = placeMonster.unit.HP;

        origin_atk = total_buffed_atk;
        origin_hp = total_buffed_hp;

        Logger.Log(gameObject.name + "의 Buff 이력");
        foreach(BuffStat stat in buffList) {
            total_buffed_atk += stat.atk;
            total_buffed_hp += stat.hp;

            Logger.Log("공격력 : " + stat.atk + ", " + "체력 : " + stat.hp);
        }

        //버프 이펙트 보여주기
        if(total_buffed_hp > origin_hp) {
            if (total_buffed_atk > origin_atk) {
                if (debuffContinue != null) {
                    Destroy(debuffContinue);
                }
            }
            if(buffContinue == null) {
                buffContinue = EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_BUFF, transform);
            }
        }

        if(total_buffed_hp < origin_hp) {
            if(total_buffed_atk < origin_atk) {
                if (buffContinue != null) {
                    Destroy(buffContinue);
                }
            }
            if (debuffContinue == null) {
                debuffContinue = EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_DEBUFF, transform);
            }
        }

        if(total_buffed_hp == origin_hp) {
            if(total_buffed_atk < origin_atk) {
                if (debuffContinue == null) {
                    debuffContinue = EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_DEBUFF, transform);
                }
            }
            else if(total_buffed_atk > origin_atk) {
                if (buffContinue == null) {
                    buffContinue = EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_BUFF, transform);
                }
            }
            else {
                if(debuffContinue != null) Destroy(debuffContinue);
                if(buffContinue != null) Destroy(buffContinue);
            }
        }
    }

    //TODO : Assault인 경우 같은 라인에 적이 있는지 없는지에 따라 계속 호출되어 리스트에 쌓이는 문제가 있음.
    //Assault처리 방법에 대해 고민
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

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EffectLevelController : WeaponLevelController
{
    //##### 強化System #####

    //Effect：ダメージ
    const int CUSTOM_SYSTEM_EFFECT_DAMAGE = 301;
    //Effect：ダメージ/秒
    const int CUSTOM_SYSTEM_EFFECT_DPS = 302;
    //Effect：Scale
    const int CUSTOM_SYSTEM_EFFECT_SCALE = 303;
    //Effect：ActiveTime
    const int CUSTOM_SYSTEM_EFFECT_TIME = 304;
    //Effect：ActiveDistance
    const int CUSTOM_SYSTEM_EFFECT_DISTANCE = 305;
    //Effect：SpawnObject
    const int CUSTOM_SYSTEM_EFFECT_SPAWN_OBJECT = 306;

    //状態異常：ATK
    const int CUSTOM_SYSTEM_EFFECT_DEBUFF_ATTACK = 331;
    //状態異常：SP
    const int CUSTOM_SYSTEM_EFFECT_DEBUFF_SP = 332;
    //状態異常：AVD
    const int CUSTOM_SYSTEM_EFFECT_DEBUFF_AVOID = 333;
    //状態異常：SPD
    const int CUSTOM_SYSTEM_EFFECT_DEBUFF_SPEED = 334;
    //状態異常：DEF
    const int CUSTOM_SYSTEM_EFFECT_DEBUFF_DEFENCE = 335;
    //状態異常：Time
    const int CUSTOM_SYSTEM_EFFECT_DEBUFF_TIME = 336;

    
    //エフェクト強化
    public void EffectCustom(EffectController effectCtrl)
    {
        UnityAction callback = () =>
        {
            for (int i = 0; i < customSystemList.Count; i++)
            {
                int customSystem = customSystemList[i];
                float effectValue = effectValueList[i] * myCustomLevel;
                EffectCustomExe(effectCtrl, customSystem, effectValue);
            }
        };
        WaitCustomReady(callback);
    }

    private void EffectCustomExe(EffectController effectCtrl, int customSystem, float effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_EFFECT_DAMAGE:
                //effect：ダメージ
                effectCtrl.CustomDamage((int)effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_DPS:
                //effect：ダメージ/秒
                effectCtrl.CustomDPS((int)effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_SCALE:
                //effect：スケール
                effectCtrl.CustomEndScale(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_TIME:
                //ActiveTime
                effectCtrl.CustomActiveTime(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_DISTANCE:
                //ActiveDistance
                effectCtrl.CustomActiveDistance(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_SPAWN_OBJECT:
                //BreakEffect
                effectCtrl.CustomBreakEffect(addObject);
                break;

            case CUSTOM_SYSTEM_EFFECT_DEBUFF_ATTACK:
                //debuff:attack
                effectCtrl.CustomDebuffAttack(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_DEBUFF_SP:
                //debuff:sp
                effectCtrl.CustomDebuffSp(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_DEBUFF_AVOID:
                //debuff:avoid
                effectCtrl.CustomDebuffAvoid(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_DEBUFF_SPEED:
                //debuff:speed
                effectCtrl.CustomDebuffSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_DEBUFF_DEFENCE:
                //debuff:defence
                effectCtrl.CustomDebuffDefence(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECT_DEBUFF_TIME:
                //debuff:time
                effectCtrl.CustomDebuffTime(effectValue);
                break;
        }
    }
}
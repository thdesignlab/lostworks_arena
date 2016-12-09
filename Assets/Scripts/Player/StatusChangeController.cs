using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusChangeController : Photon.MonoBehaviour
{
    [SerializeField]
    private float effectTime;
    [SerializeField, TooltipAttribute("1:Atk,2:SP,3:Avd,4:Speed,5:Def")]
    private List<int> effectTypeList = new List<int>();
    [SerializeField]
    private List<float> effectRateList = new List<float>();
    [SerializeField]
    private GameObject effect;
    
    public const int EFFECT_ATTACK = 1;
    public const int EFFECT_RECOVER_SP = 2;
    public const int EFFECT_AVOID = 3;
    public const int EFFECT_SPEED = 4;
    public const int EFFECT_DEFENCE = 5;

    void Awake()
    {
        if (effect != null) effect.SetActive(false);
    }

    public void Action(PlayerStatus playerStatus)
    {
        if (playerStatus == null) return;

        for (int i = 0; i < effectTypeList.Count; i++)
        {
            int effectType = effectTypeList[i];
            float effectRate = effectRateList[i];

            if (effectRate < 1)
            {
                if (playerStatus.IsForceInvincible()) continue;
                if (!playerStatus.SetDebuff(effectType, effectTime)) continue;
            }
            GameObject setEffect = (i == 0) ? effect : null;
            
            switch (effectType)
            {
                case EFFECT_ATTACK:
                    //攻撃力
                    playerStatus.ChangeAttackRate(effectRate, effectTime, setEffect);
                    break;

                case EFFECT_RECOVER_SP:
                    //SP回復量
                    playerStatus.AccelerateRecoverSp(effectRate, effectTime, setEffect);
                    break;

                case EFFECT_AVOID:
                    //回避時間
                    playerStatus.AvoidBurst(effectRate, effectTime, setEffect);
                    break;

                case EFFECT_SPEED:
                    //移動速度
                    playerStatus.AccelerateRunSpeed(effectRate, effectTime, setEffect);
                    break;

                case EFFECT_DEFENCE:
                    //防御力
                    playerStatus.ChangeDefRate(effectRate, effectTime, setEffect);
                    break;
            }
        }
    }

    public float GetEffectTime()
    {
        return effectTime;
    }

    //エフェクト時間変更
    public void AddEffectTime(float value)
    {
        effectTime += value;
        if (effectTime < 0) effectTime = 0;
    }

    //ステータス変化追加
    public void AddStatusChange(int type, float value)
    {
        int index = effectTypeList.IndexOf(type);
        if (index < 0)
        {
            //新規追加
            effectTypeList.Add(type);
            effectRateList.Add(1 + value);
        }
        else
        {
            //既存効果変化
            effectRateList[index] += value;
        }
    }
}

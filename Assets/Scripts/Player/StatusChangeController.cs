using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusChangeController : Photon.MonoBehaviour
{
    [SerializeField]
    private float effectTime;
    [SerializeField, TooltipAttribute("1:Atk,2:SP,3:Avd,4:Speed,5:Def")]
    private List<int> effectTypeList;
    [SerializeField]
    private List<float> effectRateList;
    [SerializeField]
    private GameObject effect;

    const int EFFECT_ATTACK = 1;
    const int EFFECT_RECOVER_SP = 2;
    const int EFFECT_AVOID = 3;
    const int EFFECT_SPEED = 4;
    const int EFFECT_DEFENCE = 5;

    //次の効果適用までの必要時間
    private float leftTime = 0;

    void Awake()
    {
        if (effect != null) effect.SetActive(false);
    }

    void Update()
    {
        if (leftTime >= 0) return;
        leftTime -= Time.deltaTime;
    }

    public void Action(PlayerStatus playerStatus)
    {
        if (playerStatus == null) return;
        if (leftTime > 0) return;

        leftTime = effectTime;

        for (int i = 0; i < effectTypeList.Count; i++)
        {
            int effectType = effectTypeList[i];
            float effectRate = effectRateList[i];
            GameObject setEffect = effect;
            if (i > 0)
            {
                setEffect = null;
            }
            else if (effectRate < 1)
            {
                setEffect = playerStatus.GetDebuffEffect();
            }

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
}

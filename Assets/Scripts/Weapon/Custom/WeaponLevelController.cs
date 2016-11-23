using UnityEngine;
using System.Collections;

public class WeaponLevelController : Photon.MonoBehaviour
{
    protected int customType = 0;
    protected int customLevel = 0;

    protected float powerEffectValue = 0;
    protected float technicEffectValue = 0;
    protected float uniqueEffectValue = 0;

    protected WeaponController weaponCtrl;

    public virtual void Init(WeaponController ctrl, int type, int level = 1)
    {
        if (level <= 0 || Common.Weapon.MAX_CUSTOM_LEVEL < level) return;
        if (!Common.Weapon.customTypeNameDic.ContainsKey(type)) return;

        //WeaponController
        weaponCtrl = ctrl;

        //Custom情報
        customType = type;
        customLevel = level;

        //効果値設定
        SetEffectValue();

        //通常武器
        switch (type)
        {
            case Common.Weapon.CUSTOM_TYPE_POWER:
                CustomPower();
                break;

            case Common.Weapon.CUSTOM_TYPE_TECHNIC:
                CustomSpeed();
                break;

            case Common.Weapon.CUSTOM_TYPE_UNIQUE:
                CustomUnique();
                break;
        }
    }

    protected virtual void SetEffectValue()
    {
        powerEffectValue = 30;
        technicEffectValue = 30;
        uniqueEffectValue = 30;
    }

protected virtual void SetCustom()
    {
    }

    protected virtual void CustomPower()
    {
        float rate = 100 - powerEffectValue;
        weaponCtrl.CustomReloadTime(rate);
    }
    protected virtual void CustomSpeed()
    {
        float rate = 100 - technicEffectValue;
        weaponCtrl.CustomReloadTime(rate);
    }
    protected virtual void CustomUnique()
    {
        weaponCtrl.CustomNoReload((int)uniqueEffectValue);
    }

    public int GetWeaponType()
    {
        return customType;
    }
    public int GetWeaponLevel()
    {
        return customLevel;
    }
}
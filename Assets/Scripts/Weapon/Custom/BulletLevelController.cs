using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletLevelController : WeaponLevelController
{
    [SerializeField, TooltipAttribute("101:RpdCnt,102:RpdInt,103:SpreadCnt,104:SpreadDiff,105:FocusDiff")]
    protected List<int> powerCustomSystemList;
    [SerializeField]
    protected List<int> powerEffectValueDiffList;
    [SerializeField, TooltipAttribute("131:Dmg,132:TurnSpd,133:Collider,134:ActTime,135:ActDistance,136:Stuck")]
    protected List<int> technicCustomSystemList;
    [SerializeField]
    protected List<int> technicEffectValueDiffList;
    [SerializeField, TooltipAttribute("")]
    protected List<int> uniqueCustomSystemList;
    [SerializeField]
    protected List<int> uniqueEffectValueDiffList;

    [SerializeField]
    protected GameObject AddObject;     //追加用オブジェクト

    private BulletWeaponController _bulletWeaponCtrl;
    protected BulletWeaponController bulletWeaponCtrl
    {
        get { return _bulletWeaponCtrl ? _bulletWeaponCtrl : _bulletWeaponCtrl = GetComponent<BulletWeaponController>(); }
    }

    //##### 強化System #####
    //発射数増加
    const int CUSTOM_SYSTEM_RAPID_COUNT = 101;
    //発射間隔
    const int CUSTOM_SYSTEM_RAPID_INTERVAL = 102;
    //拡散数
    const int CUSTOM_SYSTEM_SPREAD_COUNT = 103;
    //発射角
    const int CUSTOM_SYSTEM_SPREAD_DIFF = 104;
    //ブレ
    const int CUSTOM_SYSTEM_FOCUS_DIFF = 105;

    //ダメージアップ
    const int CUSTOM_SYSTEM_DAMAGE = 131;
    //旋回速度UP
    const int CUSTOM_SYSTEM_TURN_SPEED = 132;
    //判定拡大
    const int CUSTOM_SYSTEM_COLLIDER = 133;
    //ActiveTime
    const int CUSTOM_SYSTEM_ACTIVE_TIME = 134;
    //ActiveDistance
    const int CUSTOM_SYSTEM_ACTIVE_DISTANCE = 135;
    //StuckTime
    const int CUSTOM_SYSTEM_STUCK_TIME = 136;


    //カスタムSystemセットアップ
    protected override void SetCustomSystem()
    {
        switch (myCustomType)
        {
            case Common.Weapon.CUSTOM_TYPE_POWER:
                customSystemList = powerCustomSystemList;
                effectValueList = powerEffectValueDiffList;
                break;

            case Common.Weapon.CUSTOM_TYPE_TECHNIC:
                customSystemList = technicCustomSystemList;
                effectValueList = technicEffectValueDiffList;
                break;

            case Common.Weapon.CUSTOM_TYPE_UNIQUE:
                customSystemList = uniqueCustomSystemList;
                effectValueList = uniqueEffectValueDiffList;
                break;
        }
    }

    //武器強化実行
    protected override void WeaponCustomExe(int customSystem, int effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_RAPID_COUNT:
                //発射数増加
                bulletWeaponCtrl.CustomRapidCount(effectValue);
                break;

            case CUSTOM_SYSTEM_RAPID_INTERVAL:
                //発射間隔
                bulletWeaponCtrl.CustomRapidInterval(effectValue);
                break;

            case CUSTOM_SYSTEM_SPREAD_COUNT:
                //同時発射数
                bulletWeaponCtrl.CustomSpreadCount(effectValue);
                break;

            case CUSTOM_SYSTEM_SPREAD_DIFF:
                //発射角
                bulletWeaponCtrl.CustomSpreadDiff(effectValue);
                break;

            case CUSTOM_SYSTEM_FOCUS_DIFF:
                //ブレ抑制
                bulletWeaponCtrl.CustomFocusDiff(effectValue);
                break;

            default:
                base.WeaponCustomExe(customSystem, effectValue);
                break;
        }
    }

    //弾丸強化
    public void BulletCustom(GameObject bulletObj)
    {
        for (int i = 0; i < customSystemList.Count; i++)
        {
            int customSystem = customSystemList[i];
            float effectValue = effectValueList[i] * myCustomLevel;
            BulletCustomExe(bulletObj, customSystem, effectValue);
        }
    }

    //弾丸強化実行
    protected void BulletCustomExe(GameObject bulletObj, int customSystem, float effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_DAMAGE:
                //ダメージアップ
                break;

            case CUSTOM_SYSTEM_TURN_SPEED:
                //旋回速度UP
                break;

            case CUSTOM_SYSTEM_COLLIDER:
                //判定拡大
                break;

            case CUSTOM_SYSTEM_ACTIVE_TIME:
                //ActiveTime
                break;

            case CUSTOM_SYSTEM_ACTIVE_DISTANCE:
                //ActiveDistance
                break;

            case CUSTOM_SYSTEM_STUCK_TIME:
                //StuckTime
                break;
        }
    }
}
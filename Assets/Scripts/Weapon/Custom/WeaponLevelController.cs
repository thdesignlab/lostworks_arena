using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class WeaponLevelController : Photon.MonoBehaviour
{
    protected Transform myTran;

    //強化している系統、レベル
    protected int myCustomType = 0;
    protected int myCustomLevel = 0;

    //対象の強化System
    protected List<int> customSystemList;

    //強化の効果値
    protected List<int> effectValueList;

    //武器Ctrl
    protected WeaponController weaponCtrl;


    //##### 強化System #####
    //リロード短縮
    const int CUSTOM_SYSTEM_RELOAD_REDUCTION = 0;
    //一定確率でリロードなし
    const int CUSTOM_SYSTEM_NO_RELOAD = 1;


    protected virtual void Awake()
    {
        myTran = transform;
    }

    public virtual void Init(WeaponController ctrl, int type, int level = 1)
    {
        if (level <= 0 || Common.Weapon.MAX_CUSTOM_LEVEL < level) return;
        if (!Common.Weapon.customTypeNameDic.ContainsKey(type)) return;

        //WeaponController
        weaponCtrl = ctrl;

        //Custom情報
        myCustomType = type;
        myCustomLevel = level;

        //強化System,効果値を決定
        SetCustomSystem();
    }

    //カスタムSystemセットアップ
    protected abstract void SetCustomSystem();

    //武器強化
    protected void WeaponCustom()
    {
        for (int i = 0; i < customSystemList.Count; i++)
        {
            int customSystem = customSystemList[i];
            int effectValue = effectValueList[i] * myCustomLevel;
            WeaponCustomExe(customSystem, effectValue);
        }
    }

    //強化実行
    protected virtual void WeaponCustomExe(int customSystem, int effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_RELOAD_REDUCTION:
                //リロード短縮
                CustomReloadReduction(effectValue);
                break;

            case CUSTOM_SYSTEM_NO_RELOAD:
                //NOリロード
                CustomNoReload(effectValue);
                break;
        }
    }

    //リロード
    protected void CustomReloadReduction(int value)
    {
        weaponCtrl.CustomReloadTime(value);
    }

    //一定確率でリロード無効
    protected void CustomNoReload(int value)
    {
        weaponCtrl.CustomNoReload(value);
    }
}
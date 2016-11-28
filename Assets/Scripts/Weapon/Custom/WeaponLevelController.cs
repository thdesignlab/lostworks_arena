using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class WeaponLevelController : Photon.MonoBehaviour
{
    private Transform _myTran;
    protected Transform myTran
    {
        get { return _myTran ? _myTran : _myTran = transform; }
    }
    private WeaponController _weaponCtrl;
    protected WeaponController weaponCtrl
    {
        get { return _weaponCtrl ? _weaponCtrl : _weaponCtrl = GetComponent<WeaponController>(); }
    }
    protected bool isReady = false;

    //強化している系統、レベル
    protected int myCustomType = 0;
    protected int myCustomLevel = 0;

    //対象の強化System
    protected List<int> customSystemList;

    //強化の効果値
    protected List<int> effectValueList;


    //##### 強化System #####
    //リロード短縮
    const int CUSTOM_SYSTEM_RELOAD_REDUCTION = 0;
    //一定確率でリロードなし
    const int CUSTOM_SYSTEM_NO_RELOAD = 1;

    void Start()
    {
        if (!photonView.isMine)
        {
            CustomLevelSync(false);
        }
    }

    public virtual void Init(int type, int level = 1)
    {
        if (level <= 0 || Common.Weapon.MAX_CUSTOM_LEVEL < level) return;
        if (!Common.Weapon.customTypeNameDic.ContainsKey(type)) return;

        //Custom情報
        myCustomType = type;
        myCustomLevel = level;

        //強化System,効果値を決定
        SetCustomSystem();

        //装備強化
        WeaponCustom();

        isReady = true;

        //カスタムレベル同期
        CustomLevelSync();
    }

    //カスタムレベル同期
    protected void CustomLevelSync(bool isMine = true)
    {
        if (isMine)
        {
            //自分のレベルを同期
            object[] args = new object[] { myCustomType, myCustomLevel };
            photonView.RPC("CustomLevelSyncRPC", PhotonTargets.Others, args);
        }
        else
        {
            //相手のレベルを同期
            photonView.RPC("RetrunCustomLevelRPC", PhotonTargets.Others);
        }
    }
    [PunRPC]
    protected void CustomLevelSyncRPC(int type, int level)
    {
        myCustomType = type;
        myCustomLevel = level;
    }
    [PunRPC]
    protected void RetrunCustomLevelRPC()
    {
        WaitCustomReady(() => CustomLevelSync());
    }
    //IEnumerator CustomLevelSyncProc()
    //{
    //    for (;;)
    //    {
    //        if (!isReady) yield return null;
    //        CustomLevelSync();
    //        break;
    //    }
    //}
    protected void WaitCustomReady(UnityAction callback)
    {
        if (isReady)
        {
            callback.Invoke();
        }
        else
        {
            StartCoroutine(WaitCustomReadyProc(callback));
        }
    }
    IEnumerator WaitCustomReadyProc(UnityAction callback)
    {
        for (;;)
        {
            if (isReady) break;
            yield return null;
        }
        callback.Invoke();
    }

    //カスタムSystemセットアップ
    protected abstract void SetCustomSystem();

    //武器強化
    protected void WeaponCustom()
    {
        UnityAction callback = () =>
        {
            for (int i = 0; i < customSystemList.Count; i++)
            {
                int customSystem = customSystemList[i];
                int effectValue = effectValueList[i] * myCustomLevel;
                WeaponCustomExe(customSystem, effectValue);
            }
        };

        WaitCustomReady(callback);
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
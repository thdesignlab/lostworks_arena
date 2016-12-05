using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class WeaponLevelController : Photon.MonoBehaviour
{
    //##### 強化System #####

    //リロード短縮
    const int CUSTOM_SYSTEM_RELOAD_REDUCTION = 0;
    //一定確率でリロードなし
    const int CUSTOM_SYSTEM_NO_RELOAD = 1;

    //ステータスUP：ATK
    const int CUSTOM_SYSTEM_BUFF_ATTACK = 11;
    //ステータスUP：SP
    const int CUSTOM_SYSTEM_BUFF_SP = 12;
    //ステータスUP：AVD
    const int CUSTOM_SYSTEM_BUFF_AVOID = 13;
    //ステータスUP：SPD
    const int CUSTOM_SYSTEM_BUFF_SPEED = 14;
    //ステータスUP：DEF
    const int CUSTOM_SYSTEM_BUFF_DEFENCE = 15;
    //ステータスUP：TIME
    const int CUSTOM_SYSTEM_BUFF_TIME = 16;


    [SerializeField]
    protected List<int> powerCustomSystemList;
    [SerializeField]
    protected List<float> powerEffectValueDiffList;
    [SerializeField]
    protected GameObject powerObject;
    [SerializeField]
    protected List<int> technicCustomSystemList;
    [SerializeField]
    protected List<float> technicEffectValueDiffList;
    [SerializeField]
    protected GameObject technicObject;
    [SerializeField]
    protected List<int> uniqueCustomSystemList;
    [SerializeField]
    protected List<float> uniqueEffectValueDiffList;
    [SerializeField]
    protected GameObject uniqueObject;

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
    protected List<float> effectValueList;

    //追加オブジェクト
    protected GameObject addObject;

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
        SetCustomSystem();
    }
    [PunRPC]
    protected void RetrunCustomLevelRPC()
    {
        WaitCustomReady(() => CustomLevelSync());
    }

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
    const float WAIT_LIMIT = 15;
    IEnumerator WaitCustomReadyProc(UnityAction callback)
    {
        float procTime = 0;
        for (;;)
        {
            if (isReady) break;
            procTime += Time.deltaTime;
            if (procTime >= WAIT_LIMIT) yield break;
            yield return null;
        }
        callback.Invoke();
    }

    //カスタムSystemセットアップ
    protected void SetCustomSystem()
    {
        switch (myCustomType)
        {
            case Common.Weapon.CUSTOM_TYPE_POWER:
                customSystemList = powerCustomSystemList;
                effectValueList = powerEffectValueDiffList;
                addObject = powerObject;
                break;

            case Common.Weapon.CUSTOM_TYPE_TECHNIC:
                customSystemList = technicCustomSystemList;
                effectValueList = technicEffectValueDiffList;
                addObject = technicObject;
                break;

            case Common.Weapon.CUSTOM_TYPE_UNIQUE:
                customSystemList = uniqueCustomSystemList;
                effectValueList = uniqueEffectValueDiffList;
                addObject = uniqueObject;
                break;
        }
        isReady = true;
    }

    //武器強化
    protected void WeaponCustom()
    {
        UnityAction callback = () =>
        {
            for (int i = 0; i < customSystemList.Count; i++)
            {
                int customSystem = customSystemList[i];
                float effectValue = effectValueList[i] * myCustomLevel;
                WeaponCustomExe(customSystem, effectValue);
            }
        };

        WaitCustomReady(callback);
    }

    //強化実行
    protected virtual void WeaponCustomExe(int customSystem, float effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_RELOAD_REDUCTION:
                //リロード短縮
                weaponCtrl.CustomReloadTime(effectValue);
                break;

            case CUSTOM_SYSTEM_NO_RELOAD:
                //NOリロード
                weaponCtrl.CustomNoReload((int)effectValue);
                break;

            case CUSTOM_SYSTEM_BUFF_ATTACK:
                weaponCtrl.CustomBuffAttack(effectValue);
                break;

            case CUSTOM_SYSTEM_BUFF_SP:
                weaponCtrl.CustomBuffSp(effectValue);
                break;

            case CUSTOM_SYSTEM_BUFF_AVOID:
                weaponCtrl.CustomBuffAvoid(effectValue);
                break;

            case CUSTOM_SYSTEM_BUFF_SPEED:
                weaponCtrl.CustomBuffSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_BUFF_DEFENCE:
                weaponCtrl.CustomBuffDefence(effectValue);
                break;

            case CUSTOM_SYSTEM_BUFF_TIME:
                weaponCtrl.CustomBuffTime(effectValue);
                break;
        }
    }
}
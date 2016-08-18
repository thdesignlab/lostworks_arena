using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CrossRangeWeaponController : WeaponController
{
    [SerializeField]
    protected GameObject blade;
    [SerializeField]
    protected Vector3 attackStartPos;
    [SerializeField]
    protected Vector3 attackEndPos;
    [SerializeField]
    protected float attackTime;

    private Vector3 bitPos;

    private Transform parentTran;

    protected override void Awake()
    {
        base.Awake();
        bitPos = myBitTran.localPosition;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Action()
    {
        base.Action();
        StartCoroutine(BladeOn());
    }

    IEnumerator BladeOn()
    {
        //ブレードON
        blade.SetActive(true);

        //初期位置＞振り始め位置
        StartBitMove(bitFromPos, attackStartPos);

        float procTime = 0;
        for (;;)
        {
            if (!isBitMoved)
            {
                Debug.Log("isBitMoved:false");
                yield return null;
                continue;
            }
            procTime += Time.deltaTime;
            Debug.Log("procTime:"+ procTime);
            float posRate = procTime / attackTime;
            if (posRate > 1) posRate = 1;

            //振り始め位置＞振り終わり位置
            myBitTran.localPosition = Vector3.Lerp(attackStartPos, attackEndPos, posRate);

            if (procTime >= attackTime) break;
            yield return null;
        }

        //ブレードOFF
        blade.SetActive(false);

        //振り終わり位置＞初期位置
        StartBitMove(attackEndPos, bitFromPos);

        base.EndAction();
    }

    public override bool IsEnableFire()
    {
        if (!base.IsEnableFire()) return false;
        if (blade == null) return false;
        return true;
    }
    
}
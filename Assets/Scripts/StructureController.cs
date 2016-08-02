﻿using UnityEngine;
using System.Collections;

public class StructureController : Photon.MonoBehaviour
{
    private Transform myTran;
    private Transform parentTran;
    private StructureController parentCtrl;

    [SerializeField]
    private GameObject breakEffect;
    [SerializeField]
    private int maxHp;
    private int nowHp;

    void Awake ()
    {
        myTran = transform;
        parentTran = transform.parent;
        if (parentTran != null)
        {
            parentCtrl = parentTran.GetComponent<StructureController>();
        }

        nowHp = maxHp;
    }

    void OnCollisionEnter(Collision other)
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            int damage = Random.Range(1, 10);
            if (myTran.parent == null)
            {
                if (Common.Func.IsDamageAffect(other.transform.tag))
                {
                    AddDamage(damage);
                }
            }
            else
            {
                if (parentCtrl != null)
                {
                    parentCtrl.AddDamage(damage);
                }
            }
        }
    }

    public void AddDamage(int damage)
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            nowHp -= damage;
            if (nowHp <= 0) Break();
        }
    }

    private void Break()
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            //子供がいる場合パージ
            if (myTran.childCount > 0) myTran.DetachChildren();

            if (breakEffect != null)
            {
                //破壊時エフェクト
                PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(breakEffect.name), myTran.position, breakEffect.transform.rotation, 0);
            }

            //破壊
            GetComponent<ObjectController>().DestoryObject();
        }
    }
}
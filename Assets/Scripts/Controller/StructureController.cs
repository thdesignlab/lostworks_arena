using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StructureController : Photon.MonoBehaviour
{
    private Transform myTran;
    private Transform parentTran;
    private StructureController parentCtrl;

    [SerializeField]
    private GameObject breakEffect;
    [SerializeField]
    private int maxHp;
    [SerializeField]
    private bool isReflaction = false;

    private int nowHp;

    private int stockDamage = 0;

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
    
    public void AddDamage(int damage, bool isSendRPC = true)
    {
        if (damage <= 0) return;

        if (photonView.isMine)
        {
            if (parentCtrl != null)
            {
                parentCtrl.AddDamage(damage);
            }
            else
            {
                nowHp -= damage;
                if (nowHp <= 0) Break();
            }
        }
        else
        {
            if (isSendRPC)
            {
                stockDamage += damage;
                if (stockDamage >= maxHp / 10)
                {
                    photonView.RPC("AddDamageRPC", PhotonTargets.Others, stockDamage);
                    stockDamage = 0;
                }
            }
        }
    }

    [PunRPC]
    private void AddDamageRPC(int damage)
    {
        AddDamage(damage, false);
    }

    private void Break()
    {
        if (photonView.isMine)
        {
            //子供がいる場合パージ
            foreach (Transform child in myTran)
            {
                if (child.GetComponent<StructureController>() != null) child.parent = null;
            }
            //if (myTran.childCount > 0) myTran.DetachChildren();

            if (breakEffect != null)
            {
                //破壊時エフェクト
                PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(breakEffect.name), myTran.position, breakEffect.transform.rotation, 0);
            }

            //破壊
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public bool IsReflaction()
    {
        if (nowHp <= 0) return false;
        return isReflaction;
    }

    //##### CUSTOM #####

    public void CustomHp(int value)
    {
        maxHp = value;
        nowHp = maxHp;
    }
    public void CustomReflection(bool flg)
    {
        isReflaction = flg;
    }
    public void CustomChangeBreakEffect(GameObject obj)
    {
        breakEffect = obj;
    }
}

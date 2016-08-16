using UnityEngine;
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
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
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
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

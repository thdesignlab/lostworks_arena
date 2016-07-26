using UnityEngine;
using System.Collections;

public class StructureParentController : Photon.MonoBehaviour
{
    private Transform myTran;

    [SerializeField]
    private int maxHp;


    private int nowHp;
    private int totalDamage = 0;


    const int SEND_MIN_DAMAGE = 50;

    void Awake()
    {
        myTran = transform;
        nowHp = maxHp;
    }

    void Update()
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            if (nowHp <= 0)
            {
                Break();
            }

            if (myTran.childCount == 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    public void AddDamage(int damage)
    {
        totalDamage += damage;
        if (nowHp <= totalDamage || totalDamage >= SEND_MIN_DAMAGE)
        {
            photonView.RPC("AddDamageRPC", PhotonTargets.All, totalDamage);
        }
    }

    [PunRPC]
    private void AddDamageRPC(int damage)
    {
        nowHp -= damage;
    }

    private void Break()
    {
        photonView.RPC("BreakRPC", PhotonTargets.All);
    }
    [PunRPC]
    private void BreakRPC()
    {
        myTran.DetachChildren();
    }
}

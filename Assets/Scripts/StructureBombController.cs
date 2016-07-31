using UnityEngine;
using System.Collections;

public class StructureBombController : Photon.MonoBehaviour
{
    private Transform myTran;

    [SerializeField]
    private int maxHp;

    private int nowHp;
    private GameController gameCtrl;

    void Awake ()
    {
        myTran = transform;
        nowHp = maxHp;
        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
    }

    void OnCollisionEnter(Collision other)
    {
        if (!gameCtrl.isGameStart) return;

        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            if (myTran.parent == null)
            {
                if (Common.Func.IsDamageAffect(other.transform.tag))
                {
                    nowHp -= Random.Range(1, 10);
                    if (nowHp <= 0) Break();
                }
            }
        }
    }

    private void Break()
    {
        GetComponent<ObjectController>().DestoryObject();
    }
}

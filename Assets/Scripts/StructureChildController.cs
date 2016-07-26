using UnityEngine;
using System.Collections;

public class StructureChildController : Photon.MonoBehaviour
{
    private Transform myTran;
    private Rigidbody myRigidbody;
    private StructureParentController parentCtrl;

    [SerializeField]
    private int damage;
    [SerializeField]
    private float damageVelocity;
    [SerializeField]
    private int liveTime;

    private float destroyTime = 0;

    void Awake()
    {
        myTran = transform;
        myRigidbody = GetComponent<Rigidbody>();
        parentCtrl = myTran.root.GetComponent<StructureParentController>();
    }

    void Update()
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            if (myTran.parent == null)
            {
                destroyTime += Time.deltaTime;
                myRigidbody.isKinematic = false;
                if (destroyTime >= liveTime)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            if (myTran.parent == null)
            {
                if (other.transform.tag == "Player" && myRigidbody.velocity.magnitude >= damageVelocity)
                {
                    other.gameObject.GetComponent<PlayerStatus>().AddDamage(damage);
                }
            }
            else
            {
                if (Common.Func.IsDamageAffect(other.transform.tag))
                {
                    parentCtrl.AddDamage(100);
                }
            }
        }
    }
}
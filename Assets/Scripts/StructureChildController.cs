using UnityEngine;
using System.Collections;

public class StructureChildController : Photon.MonoBehaviour
{
    private Transform myTran;
    private Rigidbody myRigidbody;
    private StructureParentController parentCtrl;
    //private Material myMat;

    [SerializeField]
    private float liveTime;

    private float destroyTime = 0;
    //private Color alpha = new Color(0, 0, 0, 0.1f);

    void Awake()
    {
        myTran = transform;
        myRigidbody = GetComponent<Rigidbody>();
        parentCtrl = myTran.root.GetComponent<StructureParentController>();
        //myMat = GetComponent<Renderer>().material;
        liveTime *= Random.Range(1.0f, 2.0f);
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
            if (myTran.parent != null)
            {
                if (Common.Func.IsDamageAffect(other.transform.tag))
                {
                    int damage = Random.Range(1, 25);
                    parentCtrl.AddDamage(damage);
                }
            }
        }
    }
}
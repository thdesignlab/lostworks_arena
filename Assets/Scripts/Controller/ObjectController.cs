using UnityEngine;
using System.Collections;

public class ObjectController : Photon.MonoBehaviour {

    [SerializeField]
    private GameObject effectSpawn;
    private int effectSpawnGroupId = 0;
    [SerializeField]
    private float activeLimitTime = 0;
    [SerializeField]
    private float activeLimitDistance = 0;
    [SerializeField]
    private bool isNotAutoBreak = false;

    private Transform myTran;
    private Transform ownerTran;
    private string ownerWeapon;

    private float activeTime = 0;
    private float activeDistance = 0;

    void Start()
    {
        myTran = transform;

        if (photonView.isMine)
        {
            if (activeLimitTime == 0 && !isNotAutoBreak) activeLimitTime = 15;
            if (activeLimitTime > 0) StartCoroutine(CountDown());
            if (activeLimitDistance > 0) StartCoroutine(CheckDistance());
        }
    }

    IEnumerator CountDown()
    {
        for (;;)
        {
            activeTime += Time.deltaTime;
            if (activeTime >= activeLimitTime) break;
            yield return null;
        }
        DestoryObject();
    }

    IEnumerator CheckDistance()
    {
        Vector3 prePos = myTran.position;
        for (;;)
        {
            activeDistance += Mathf.Abs(Vector3.Distance(myTran.position, prePos));
            if (activeDistance >= activeLimitDistance)
            {
                DestoryObject();
                break;
            }
            prePos = myTran.position;
            yield return null;
        }
    }

    public void DestoryObject(bool isSendRpc = false)
    {
        //Debug.Log(myTran.name + " >>" + photonView + " / "+ PhotonNetwork.player);
        //if (!photonView) return;
        if (photonView.isMine)
        {
            //Debug.Log("DestroyProccess >> " + myTran.name + " /" + PhotonNetwork.player);
            DestroyProccess();
        }
        else
        {
            if (isSendRpc)
            {
                photonView.RPC("DestroyObjectRPC", PhotonTargets.Others);
            }
        }
    }

    [PunRPC]
    private void DestroyObjectRPC()
    {
        DestoryObject();
    }

    private void DestroyProccess()
    {
        if (effectSpawn != null)
        {
            GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(effectSpawn.name), myTran.position, effectSpawn.transform.rotation, effectSpawnGroupId);
            GetOwnerTran();
            effectObj.GetComponent<EffectController>().SetOwner(ownerTran, ownerWeapon);
        }
        PhotonNetwork.Destroy(gameObject);
    }

    private void GetOwnerTran()
    {
        EffectController effectCtrl = myTran.GetComponent<EffectController>();
        if (effectCtrl != null) effectCtrl.GetOwner(out ownerTran, out ownerWeapon);

        BulletController bulletCtrl = myTran.GetComponent<BulletController>();
        if (bulletCtrl != null) bulletCtrl.GetOwner(out ownerTran, out ownerWeapon);
    }

    public void Reset()
    {
        activeTime = 0;
        activeDistance = 0;
    }
}

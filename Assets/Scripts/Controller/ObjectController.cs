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
    private Transform targetTran;
    private Transform weaponTran;

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
        if (photonView.isMine)
        {
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
            effectObj.GetComponent<EffectController>().EffectSetting(ownerTran, targetTran, weaponTran);
        }
        PhotonNetwork.Destroy(gameObject);
    }

    public void ObjectSetting(Transform owner, Transform target, Transform weapon)
    {
        SetOwner(owner);
        SetTarget(targetTran);
        SetWeapon(weaponTran);
    }
    public void SetOwner(Transform owner)
    {
        ownerTran = owner;
    }
    public void SetTarget(Transform target)
    {
        targetTran = target;
    }
    public void SetWeapon(Transform weapon)
    {
        weaponTran = weapon;
    }

    public void Reset()
    {
        activeTime = 0;
        activeDistance = 0;
    }


    //##### CUSTOM #####

    //ActiveTime
    public void CustomActiveTime(float value)
    {
        if (activeLimitTime <= 0) return;
        activeLimitTime *= 1 + (value + 100);
        if (activeLimitTime <= 0) activeLimitTime = 0.1f;
    }

    //ActiveDistance
    public void CustomActiveDistance(float value)
    {
        if (activeLimitDistance <= 0) return;
        activeLimitDistance *= 1 + (value + 100);
        if (activeLimitDistance <= 0) activeLimitDistance = 10; ;
    }

    //SpawnEffect
    public void CustomSpawnEffect(GameObject obj)
    {
        effectSpawn = obj;
    }
}

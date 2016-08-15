﻿using UnityEngine;
using System.Collections;

public class ObjectController : Photon.MonoBehaviour {

    [SerializeField]
    private GameObject effectSpawn;
    private int effectSpawnGroupId = 0;
    [SerializeField]
    private float activeLimitTime = 0;
    [SerializeField]
    private float activeLimitDistance = 0;

    private Transform myTran;

    void Start()
    {
        myTran = transform;

        if (photonView.isMine)
        {
            if (activeLimitTime > 0)
            {
                StartCoroutine(CountDown());
            }
            if (activeLimitDistance > 0)
            {
                StartCoroutine(CheckDistance());
            }
        }
    }

    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(activeLimitTime);
        DestoryObject();
    }

    IEnumerator CheckDistance()
    {
        float distance = 0;
        Vector3 prePos = myTran.position;
        for (;;)
        {
            distance += Mathf.Abs(Vector3.Distance(myTran.position, prePos));
            if (distance >= activeLimitDistance)
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
        //if (!photonView) return;
        if (photonView.isMine)
        {
            //Debug.Log(transform.name+" >> "+PhotonNetwork.player);
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
            //PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(effectSpawn.name), myTran.position, myTran.rotation, effectSpawnGroupId);
            PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(effectSpawn.name), myTran.position, effectSpawn.transform.rotation, effectSpawnGroupId);
        }
        PhotonNetwork.Destroy(gameObject);
    }
}

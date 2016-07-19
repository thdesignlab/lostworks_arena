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

    private Transform myTran;

    const string EFFECT_FOLDER = "Effect/";

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
        Vector3 fromPos = myTran.position;
        for (;;)
        {
            yield return new WaitForSeconds(0.5f);
            distance += Mathf.Abs(Vector3.Distance(myTran.position, fromPos));
            if (distance >= activeLimitDistance)
            {
                DestoryObject();
                break;
            }
            fromPos = transform.position;
        }
    }

    public void DestoryObject(float delay = 0, bool isSendRpc = true)
    {
        if (!photonView) return;
        if (photonView.isMine)
        {
            if (delay > 0)
            {
                StartCoroutine(DelayDestroy(delay));
            }
            else
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            if (isSendRpc)
            {
                object[] args = new object[] { delay };
                photonView.RPC("DestroyObjectRPC", PhotonTargets.All, args);
            }
        }
    }

    [PunRPC]
    private void DestroyObjectRPC(float delay)
    {
        DestoryObject(delay, false);
    }

    IEnumerator DelayDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (photonView == null) yield break;
        PhotonNetwork.Destroy(gameObject);
    }
}

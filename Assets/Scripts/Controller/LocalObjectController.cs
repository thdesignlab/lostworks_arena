using UnityEngine;
using System.Collections;

public class LocalObjectController : MonoBehaviour
{

    [SerializeField]
    private GameObject effectSpawn;
    [SerializeField]
    private float activeLimitTime = 0;
    [SerializeField]
    private float activeLimitDistance = 0;

    private Transform myTran;

    void Start()
    {
        myTran = transform;

        if (activeLimitTime > 0)
        {
            StartCoroutine(CountDown());
        }
        if (activeLimitDistance > 0)
        {
            StartCoroutine(CheckDistance());
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
            yield return new WaitForSeconds(0.5f);
            distance += Mathf.Abs(Vector3.Distance(myTran.position, prePos));
            if (distance >= activeLimitDistance)
            {
                DestoryObject();
                break;
            }
            prePos = myTran.position;
        }
    }

    public void DestoryObject()
    {
        DestroyProccess();
    }

    private void DestroyProccess()
    {
        if (effectSpawn != null)
        {
            GameObject.Instantiate(effectSpawn, myTran.position, effectSpawn.transform.rotation);
        }
        Destroy(gameObject);
    }
}

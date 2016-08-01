using UnityEngine;
using System.Collections;

public class BeamBulletController : EnergyBulletController
{
    [SerializeField]
    private float effectiveLength;   //射程距離
    [SerializeField]
    private float effectiveTime;   //最大射程になるまでの時間
    [SerializeField]
    private float effectiveWidthTime; //最大射程になってから消滅するまでの時間
    [SerializeField]
    private float runSpeedRate;   //移動速度制限
    [SerializeField]
    private float turnSpeedRate;   //回転速度制限

    private float lengthRate;
    private Transform endPoint;
    private Vector3 startPos;
    private Vector3 endPos;

    protected override void Awake()
    {
        base.Awake();

        foreach (Transform child in myTran)
        {
            if (child.tag == "LaserEnd")
            {
                endPoint = child;
                startPos = endPoint.position;
                endPos = startPos - endPoint.up * effectiveLength;
                break;
            }
        }
    }

    protected override void Update()
    {
        //稼働時間
        activeTime += Time.deltaTime;
        if (activeTime >= 10) base.DestoryObject();

        if (activeTime >= safetyTime)
        {
            if (myCollider != null) myCollider.enabled = true;
        }

        if (lengthRate < 1)
        {
            lengthRate = activeTime / effectiveTime;
            if (lengthRate > 1) lengthRate = 1;

            endPoint.position = Vector3.Lerp(startPos, endPos, lengthRate);
            if (lengthRate == 1) StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        float nowTime = 0;
        Vector3 startScale = new Vector3(1, 1, 0);
        Vector3 endScale = Vector3.zero;
        for (;;)
        {
            yield return null;
            nowTime += Time.deltaTime;
            if (nowTime > effectiveWidthTime) break;
            myTran.localScale = Vector3.Lerp(startScale, endScale, nowTime / effectiveWidthTime);            
        }
        myTran.localScale = endScale;
        DestoryObject();
    }
}

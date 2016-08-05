using UnityEngine;
using System.Collections;

public class ChangeEffectController : EffectController
{
    [SerializeField]
    private float startScale;
    [SerializeField]
    private float endScale;
    [SerializeField]
    private float scaleTime;
    [SerializeField]
    private float effectiveTime;

    private float activeTime = 0;
    private Vector3 startScaleVector;
    private Vector3 lastScaleVector;

    protected override void Awake()
    {
        base.Awake();

        startScaleVector = new Vector3(startScale, startScale, startScale);
        lastScaleVector = new Vector3(endScale, endScale, endScale);

        base.myTran.localScale = startScaleVector;
    }
	
	void Update ()
    {
        activeTime += Time.deltaTime;

        float rate = activeTime / scaleTime;
        if (rate > 1) rate = 1;

        base.myTran.localScale = Vector3.Lerp(startScaleVector, lastScaleVector, rate);
    }
}

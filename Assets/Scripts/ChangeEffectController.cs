using UnityEngine;
using System.Collections;

public class ChangeEffectController : EffectController
{
    [SerializeField]
    private Vector3 startScale;
    [SerializeField]
    private Vector3 endScale;
    [SerializeField]
    private float scaleTime;
    [SerializeField]
    private float effectiveTime;

    private float activeTime = 0;

    protected override void Awake()
    {
        base.Awake();

        base.myTran.localScale = startScale;
    }
	
	void Update ()
    {
        activeTime += Time.deltaTime;

        float rate = activeTime / scaleTime;
        if (rate > 1) rate = 1;

        base.myTran.localScale = Vector3.Lerp(startScale, endScale, rate);
    }
}

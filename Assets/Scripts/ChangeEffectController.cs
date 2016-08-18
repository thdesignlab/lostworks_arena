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
    [SerializeField]
    private float scaleLateTime;
    [SerializeField]
    protected bool isParticle;

    private SphereCollider myCollider;
    private float waitTime = 0;
    private float activeTime = 0;

    protected override void Awake()
    {
        base.Awake();
        myCollider = GetComponent<SphereCollider>();

        //base.myTran.localScale = startScale;
        ChangeScale(startScale);
    }
	
	void Update ()
    {
        if (scaleLateTime > 0)
        {
            waitTime += Time.deltaTime;
            if (scaleLateTime >= waitTime) return;
        }

        activeTime += Time.deltaTime;

        float rate = activeTime / scaleTime;
        if (rate > 1) rate = 1;

        ChangeScale(Vector3.Lerp(startScale, endScale, rate));
        //myTran.localScale = Vector3.Lerp(startScale, endScale, rate);
    }

    private void ChangeScale(Vector3 scale)
    {
        myTran.localScale = scale;
        if (isParticle && myCollider != null)
        {
            myCollider.radius = scale.x / 2;
        }
    }
}

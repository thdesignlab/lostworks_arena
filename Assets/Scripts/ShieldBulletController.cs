using UnityEngine;
using System.Collections;

public class ShieldBulletController : BulletController
{
    [SerializeField]
    private float maxRadius;
    [SerializeField]
    private float radiusTime;

    private float startRadius = 1;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        if (ownerTran != null) startRadius = Vector3.Distance(myTran.position, ownerTran.position);
    }

    protected override void Update()
    {
        base.Update();

        if (ownerTran == null) return;

        //自分に対して常に垂直にする
        //myTran.LookAt(ownerTran.position);
        //float radiusRate = activeTime / radiusTime;
        //if (radiusRate <= 1)
        //{
        //    float nowRadius = Mathf.Lerp(startRadius, maxRadius, radiusRate);
        //    float diffRadius = Mathf.Abs(Vector3.Distance(myTran.position, ownerTran.position) - Mathf.Lerp(startRadius, maxRadius, radiusRate));
        //    myTran.position -= myTran.forward * diffRadius;
        //}
        //myTran.Rotate(Vector3.up, -90);
    }
}

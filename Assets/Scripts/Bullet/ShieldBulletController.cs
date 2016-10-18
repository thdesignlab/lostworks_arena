using UnityEngine;
using System.Collections;

public class ShieldBulletController : BulletController
{
    [SerializeField]
    private float angleSpeed;

    private Vector3 preOwnerPos;

    protected override void Awake()
    {
        base.Awake();
        preOwnerPos = Vector3.zero;
    }

    protected override void Start()
    {
        base.Start();
        if (ownerTran != null) preOwnerPos = ownerTran.position;

    }

    protected override void Update()
    {
        base.Update();

        if (ownerTran == null) return;

        //自分に対して常に垂直にする
        //myTran.Rotate(ownerTran.up, angleSpeed);
        myTran.RotateAround(ownerTran.position, ownerTran.up, angleSpeed * Time.deltaTime);
        if (preOwnerPos != Vector3.zero)
        myTran.position += ownerTran.position - preOwnerPos;
        preOwnerPos = ownerTran.position;

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

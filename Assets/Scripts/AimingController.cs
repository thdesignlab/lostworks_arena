using UnityEngine;
using System.Collections;

public class AimingController: BaseMoveController
{
    private Transform targetTran;
    private PlayerStatus status;

    private float aimSpeed = 60;

    protected override void Update()
    {
        if (targetTran == null || status == null) return;

        if (status.IsLocked() || status.IsNpc())
        {
            base.SetAngle(targetTran, aimSpeed);
        }
        else
        {
            base.myTran.rotation = base.myTran.root.rotation;
        }
    }

    public void SetTarget(Transform target)
    {
        if (target == null) return;

        status = target.GetComponent<PlayerStatus>();
        if (status == null) return;
        targetTran = target;
    }

    //protected override void Move(Vector3 vector, float speed, float limit = 0){}
}

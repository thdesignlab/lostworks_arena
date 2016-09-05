using UnityEngine;
using System.Collections;

public class AimingController: BaseMoveController
{
    private Transform targetTran;
    private PlayerStatus targetStatus;
    private PlayerStatus myStatus;

    [SerializeField]
    private float aimSpeed = 60;
    private float defaultAimSpeed;

    protected override void Awake()
    {
        base.Awake();
        defaultAimSpeed = aimSpeed;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (targetStatus == null) return;
        if (aimSpeed <= 0) return;

        if (targetStatus.IsLocked() || base.isNpc)
        {
            //Debug.Log("Aiming:"+base.isNpc);
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

        targetStatus = target.GetComponent<PlayerStatus>();
        targetTran = target;
    }
    
    public void SetAimSpeed(float rate = -1)
    {
        if (rate < 0)
        {
            aimSpeed = defaultAimSpeed;
        }
        else
        {
            aimSpeed *= rate;
        }
    }

    public void SetNpc(bool flg)
    {
        base.isNpc = flg;
    }
}

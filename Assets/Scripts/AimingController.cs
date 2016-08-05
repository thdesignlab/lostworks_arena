using UnityEngine;
using System.Collections;

public class AimingController: BaseMoveController
{
    private Transform targetTran;
    private PlayerStatus targetStatus;
    private PlayerStatus myStatus;

    [SerializeField]
    private float aimSpeed = 60;
    

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        GameObject gameObj = GameObject.Find("GameController");
        if (gameObj)
        {
            //myStatus = gameObj.GetComponent<GameController>().GetMyTran().GetComponent<PlayerStatus>();
            base.CheckNpc();
        }
    }

    protected override void Update()
    {
        if (targetStatus == null) return;

        if (targetStatus.IsLocked() || base.isNpc)
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

        targetStatus = target.GetComponent<PlayerStatus>();
        //if (targetStatus == null) return;
        targetTran = target;
    }

    //protected override void Move(Vector3 vector, float speed, float limit = 0){}
}

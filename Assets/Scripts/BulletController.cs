using UnityEngine;
using System.Collections;

public class BulletController : MoveOfVelocity
{
    [SerializeField]
    protected float firstSpeed;  //発射速度
    [SerializeField]
    protected int damage; //ダメージ量

    protected int playerId;
    protected int ownerId;

    protected float activeTime = 0;
    protected float safetyTime = 1.0f;
    protected bool isHit = false;


    protected override void Awake()
    {
        base.Awake();

        //プレイヤーIDと所有者ID取得
        playerId = PhotonNetwork.player.ID;
        ownerId = photonView.ownerId;
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        //初速設定
        SetSpeed(firstSpeed);

    }

    protected override void Update()
    {
        base.Update();
        activeTime += Time.deltaTime;
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (isHit) return;
        isHit = true;

        if (other.gameObject.CompareTag("Player"))
        {
            //ダメージ処理
            if (photonView.isMine)
            {
                other.gameObject.GetComponent<PlayerStatus>().AddDamage(damage);
            }
        }
        else if (other.gameObject.CompareTag("Bullet"))
        {
            other.gameObject.GetComponent<ObjectController>().DestoryObject();
        }

        base.DestoryObject();
    }
    void OnControllerColliderHit(ControllerColliderHit other)
    {
        // hit.gameObjectで衝突したオブジェクト情報が得られる
    }
    //protected bool IsSafety()
    //{
    //    if (playerId == ownerId)
    //        return true;
    //}

    public float GetFirstSpeed()
    {
        return firstSpeed;
    }
}

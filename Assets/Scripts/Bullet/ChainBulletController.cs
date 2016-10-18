using UnityEngine;
using System.Collections;

public class ChainBulletController : TrackingBulletController
{
    [SerializeField]
    protected GameObject chainObject;
    [SerializeField]
    protected int chainCount;  //連鎖回数
    [SerializeField]
    protected float chainTime; //連鎖間隔(時間)
    [SerializeField]
    protected float chainDistance; //連鎖間隔(距離)

    private float activeDistance = 0;
    private int nowChainCount = 0;
    private float preChainTime = 0;
    private float preChainDistance = 0;

    private int layerMask;

    protected override void Awake()
    {
        base.Awake();

        int layerNo = LayerMask.NameToLayer("Floor");
        layerMask = 1 << layerNo;
    }
    protected override void Update()
    {
        if (photonView.isMine)
        {
            base.Update();

            //移動距離
            activeDistance += moveDiffVector.magnitude;

            if (chainTime > 0)
            {
                if (preChainTime + chainTime <= activeTime)
                {
                    SpawnEffect();
                    preChainTime += chainTime;
                }
            }
            else if (chainDistance > 0)
            {
                if (preChainDistance + chainDistance <= activeDistance)
                {
                    SpawnEffect();
                    preChainDistance += chainDistance;
                }
            }
        }
    }

    private void SpawnEffect()
    {
        RaycastHit hit;
        Ray ray = new Ray(myTran.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, 10.0f, layerMask))
        {
            Vector3 pos = new Vector3(myTran.position.x, 0, myTran.position.z);
            GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(chainObject.name), pos, chainObject.transform.rotation, 0);
            effectObj.GetComponent<EffectController>().SetOwner(ownerTran, ownerWeapon);
        }
        nowChainCount++;

        if (nowChainCount >= chainCount)
        {
            DestoryObject();
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LockOnController : Photon.MonoBehaviour
{
    private Transform myTran;
    private PlayerStatus status;

    private RawImage targetMarkImg;
    private RectTransform targetMarkRectTran;
    private float markFirstSizeRate = 1.2f;
    private float markLastSizeRate = 0.7f;
    private float markResizeTime = 0.3f;

    private bool isLockOn = false;
    private float lockOnTime = 0;

    void Awake ()
    {
        myTran = transform;
        status = myTran.root.gameObject.GetComponent<PlayerStatus>();
        //GameObject targetObj = GameObject.Find(Common.CO.SCREEN_CANVAS+"TargetMark");
        GameObject targetObj = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS + Common.CO.TARGET_MARK).gameObject;
        if (targetObj != null)
        {
            targetMarkImg = targetObj.GetComponent<RawImage>();
            targetMarkRectTran = targetObj.GetComponent<RectTransform>();
        }
	}


    void OnBecameInvisible()
    {
        if (!photonView.isMine || status.IsNpc())
        {
            //Debug.Log("Invisible" + transform.root.name);
            isLockOn = false;
            status.SetLocked(false);
            SetTargetMark(false);
        }
    }
    void OnBecameVisible()
    {
        if (!photonView.isMine || status.IsNpc())
        {
            //Debug.Log("Visible: " + myTran.root.name);
            isLockOn = true;
            status.SetLocked(true);
            SetTargetMark(true);
        }
    }

    private void SetTargetMark(bool flg)
    {
        if (targetMarkImg == null) return;

        targetMarkRectTran.localScale = Vector3.one;
        targetMarkImg.enabled = flg;
        if (!flg)
        {
            lockOnTime = 0;
        }
    }

    void Update()
    {
        if (isLockOn)
        {
            if (targetMarkRectTran.localScale != Vector3.one * markLastSizeRate)
            {
                lockOnTime += Time.deltaTime;
                if (lockOnTime > markResizeTime) lockOnTime = markResizeTime;
                targetMarkRectTran.localScale = Vector3.Lerp(Vector3.one * markFirstSizeRate, Vector3.one * markLastSizeRate, lockOnTime / markResizeTime);
            }
            targetMarkRectTran.position = RectTransformUtility.WorldToScreenPoint(Camera.main, myTran.position);
        }
    }
}

﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LockOnController : Photon.MonoBehaviour
{
    private Transform myTran;
    private PlayerStatus status;

    private RectTransform CanvasRect;
    private RawImage targetMarkImg;
    private RectTransform targetMarkRectTran;
    private float markFirstSizeRate = 1.5f;
    private float markLastSizeRate = 0.7f;
    private float markResizeTime = 0.3f;

    private bool isLockOn = false;
    private float lockOnTime = 0;

    private bool isACtiveSceane = true;

    void Awake ()
    {
        if (SceneManager.GetActiveScene().name == Common.CO.SCENE_CUSTOM)
        {
            //カスタム画面
            isACtiveSceane = false;
            return;
        }

        myTran = transform;
        //GameObject targetObj = GameObject.Find(Common.CO.SCREEN_CANVAS+"TargetMark");
	}
    void Start()
    {
        if (!isACtiveSceane) return;

        CanvasRect = GameObject.FindGameObjectWithTag("PlayerCanvas").GetComponent<RectTransform>();
        GameObject targetObj = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS + Common.CO.TARGET_MARK).gameObject;
        if (targetObj != null)
        {
            targetMarkImg = targetObj.GetComponent<RawImage>();
            targetMarkRectTran = targetObj.GetComponent<RectTransform>();
        }

        //ステータス取得
        StartCoroutine(SetStatus());
    }

    void OnBecameInvisible()
    {
        if (!isACtiveSceane) return;

        if (status == null) return;

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
        if (!isACtiveSceane) return;

        if (status == null) return;

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
        if (!isACtiveSceane) return;

        if (isLockOn)
        {
            if (targetMarkRectTran.localScale != Vector3.one * markLastSizeRate)
            {
                //サイズ変更
                lockOnTime += Time.deltaTime;
                if (lockOnTime > markResizeTime) lockOnTime = markResizeTime;
                targetMarkRectTran.localScale = Vector3.Lerp(Vector3.one * markFirstSizeRate, Vector3.one * markLastSizeRate, lockOnTime / markResizeTime);
            }

            //位置変更
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(myTran.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
                ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
                ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));
            targetMarkRectTran.anchoredPosition = WorldObject_ScreenPosition;
        }
    }

    IEnumerator SetStatus()
    {
        for (;;)
        {
            status = myTran.root.GetComponent<PlayerStatus>();
            if (status != null) break;
            yield return null;
        }
    }
}

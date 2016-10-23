using UnityEngine;
using System.Collections;

public class LaserPointerController : MonoBehaviour
{
    [SerializeField]
    private LineRenderer pointer;
    [SerializeField]
    private GameObject impactPoint;
    [SerializeField]
    private float maxLength = 200;
    [SerializeField]
    private float pointSize = 1;

    private Transform myTran;
    private Transform pointTran;
    private bool isActive = false;
    private Vector3 impactScale;
    private float procTime = 0;
    private float pointScaleTime = 0.3f;
    private bool isDispPoint = false;
    private int layerMask;

    void Awake()
    {
        myTran = transform;
        if (impactPoint != null) impactScale = new Vector3(pointSize, impactPoint.transform.localScale.y, pointSize);
        int layerNo = LayerMask.NameToLayer(Common.CO.LAYER_FLOOR);
        layerMask = 1 << layerNo;
        SetOff();
    }

    void Update()
    {
        if (isActive)
        {
            RaycastHit hit;
            if (GetRaycastHit(out hit))
            {
                SetDistance(hit);
            }
            else
            {
                SetPointerLength(maxLength);
                //SetOff();
            }
        }
    }

    private bool GetRaycastHit(out RaycastHit hit)
    {
        //Ray ray = new Ray(myTran.position, myTran.forward);
        bool isHit = Physics.Raycast(myTran.position, myTran.forward, out hit, maxLength, layerMask);
        if (impactPoint != null)
        {
            //着弾点
            if (isHit)
            {
                isDispPoint = true;
                if (pointTran != null)
                {
                    //再表示
                    if (pointTran.localScale == Vector3.zero) StartCoroutine(SetPointScale());
                }
                else
                {
                    //生成
                    GameObject pointObj = (GameObject)Instantiate(impactPoint, hit.point, Quaternion.identity);
                    pointTran = pointObj.transform;
                    StartCoroutine(SetPointScale());
                }
            }
            else
            {
                //非表示
                isDispPoint = false;
                if (pointTran != null) pointTran.localScale = Vector3.zero;
            }
        }
        return isHit;
    }
    IEnumerator SetPointScale()
    {
        procTime = 0;
        for (;;)
        {
            if (!isDispPoint) break;
            procTime += Time.deltaTime;
            pointTran.localScale = Vector3.Slerp(Vector3.zero, impactScale, procTime / pointScaleTime); ;
            if (procTime / pointScaleTime >= 1) break;
            yield return null;
        }
    }

    private void SetPointerLength(float distance)
    {
        if (pointer != null)
        {
            pointer.SetPosition(1, Vector3.forward * distance);
        }
    }

    private void SetDistance(RaycastHit hit)
    {
        if (pointer != null)
        {
            float distance = Vector3.Distance(myTran.position, hit.point);
            SetPointerLength(distance);
        }
        if (pointTran != null) pointTran.position = hit.point;
    }

    public void SetOn()
    {
        isActive = true;
        RaycastHit hit;
        GetRaycastHit(out hit);
        if (pointer != null) pointer.enabled = true;
    }

    public void SetOff()
    {
        isActive = false;
        if (pointer != null) pointer.enabled = false;
        if (pointTran != null) Destroy(pointTran.gameObject);
    }

    void OnDestroy()
    {
        SetOff();
    }
}

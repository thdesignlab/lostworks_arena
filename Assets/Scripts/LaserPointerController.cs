using UnityEngine;
using System.Collections;

public class LaserPointerController : MonoBehaviour
{
    [SerializeField]
    private LineRenderer pointer;
    [SerializeField]
    private float maxLength = 200;
    //[SerializeField]
    //private float width = 0.1f;

    private Transform myTran;
    private bool isActive = false;

    void Awake()
    {
        myTran = transform;
        SetOff();
    }

    void Update()
    {
        if (isActive)
        {
            RaycastHit hit;
            int layerNo = LayerMask.NameToLayer(Common.CO.LAYER_FLOOR);
            int layerMask = 1 << layerNo;
            Ray ray = new Ray(myTran.position, myTran.forward);
            if (Physics.Raycast(ray, out hit, maxLength, layerMask))
            {
                float len = Vector3.Distance(myTran.position, hit.transform.position);
                pointer.SetPosition(1, new Vector3(0, 0, len));
            }
            else
            {
                SetOff();
            }
        }
    }

    public void SetOn()
    {
        isActive = true;
        pointer.enabled = true;
    }

    public void SetOff()
    {
        isActive = false;
        pointer.enabled = false;
    }
}

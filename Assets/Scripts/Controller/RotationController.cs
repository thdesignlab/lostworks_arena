using UnityEngine;
using System.Collections;

public class RotationController : Photon.MonoBehaviour
{
    [SerializeField]
    private float rollSpeed;
    [SerializeField]
    private Vector3 rollAxis = Vector3.zero;

    private Transform myTran;

    void Awake()
    {
        myTran = transform;
    }

    void Update()
    {
        if (rollSpeed > 0)
        {
            //回転
            if (rollAxis == Vector3.zero)
            {
                float x = Random.Range(0, 1.0f);
                float y = Random.Range(0, 1.0f);
                float z = Random.Range(0, 1.0f);
                rollAxis = new Vector3(x, y, z);
            }
            myTran.Rotate(rollAxis, rollSpeed * Time.deltaTime);
        }
    }
}

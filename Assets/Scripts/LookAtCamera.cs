using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour
{
    private Transform myTran;
    private Transform cameraTran;
    private Quaternion defaultRotation;

    void Start ()
    {
        myTran = transform;
        cameraTran = Camera.main.transform;
        defaultRotation = myTran.rotation;
    }

    void Update ()
    {
        myTran.rotation = cameraTran.rotation * defaultRotation;
    }
}

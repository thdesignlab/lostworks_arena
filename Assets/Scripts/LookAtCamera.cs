using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour
{
    private Transform myTran;
    private Transform cameraTran;

	void Start ()
    {
        myTran = transform;
        cameraTran = Camera.main.transform;
	}
	
	void Update ()
    {
        myTran.rotation = cameraTran.rotation;
	}
}

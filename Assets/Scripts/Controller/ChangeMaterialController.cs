using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChangeMaterialController : Photon.MonoBehaviour
{
    [SerializeField]
    private Material changeMaterial;
    [SerializeField]
    private bool isMineOnly = false;

    private Transform myTran;
    private Renderer myRenderer;

    void Awake ()
    {
        myTran = transform;
        myRenderer = myTran.GetComponent<Renderer>();

        if (!isMineOnly || (isMineOnly && photonView.isMine))
        {
            myRenderer.material = changeMaterial;
        }
    }
}

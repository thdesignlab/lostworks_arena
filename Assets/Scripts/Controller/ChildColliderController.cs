using UnityEngine;
using System.Collections;

public class ChildColliderController : MonoBehaviour {

    private GameObject parent;

    // Use this for initialization
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        parent.SendMessage("OnChildTriggerEnter", other);
    }

    void OnTriggerStay(Collider other)
    {
        parent.SendMessage("OnChildTriggerStay", other);
    }
}

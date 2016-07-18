using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    Transform myTran;

    [SerializeField]
    private float shakeValue = 0.2f;
    [SerializeField]
    private float shakeTime = 0.5f;
    private Vector3 originPosition;
    private Quaternion originRotation;
    private float shake_intensity;

    private Vector3 defaultPos;
    private Quaternion defaultQuat;

    void Awake ()
    {
        myTran = transform;
    }

    void Start()
    {
        //defaultPos = transform.root.InverseTransformPoint(transform.position);
        defaultQuat = myTran.localRotation;
    }

    public void Shake()
    {
        StartCoroutine(ShakeCamera());
    }

    IEnumerator ShakeCamera()
    {
        float time = shakeTime;
        for (;;)
        {
            //transform.position = originPosition + Random.insideUnitSphere * shake_intensity;
            myTran.localRotation  = new Quaternion(
                defaultQuat.x + Random.Range(shakeValue * -1, shakeValue),
                defaultQuat.y + Random.Range(shakeValue * -1, shakeValue),
                defaultQuat.z + Random.Range(shakeValue * -1, shakeValue),
                defaultQuat.w + Random.Range(shakeValue * -1, shakeValue)
            );
            time += Time.deltaTime;
            if (time >= shakeTime) break;
            yield return null;
        }

        //transform.position = defaultPos;
        myTran.localRotation = defaultQuat;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteStudioController : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonFlash;

    private GameObject view3D;
    public Dictionary<string, Script_SpriteStudio_Root> scriptRoots = new Dictionary<string, Script_SpriteStudio_Root>();
	
	void Update ()
    {
        if (view3D == null)
        {
            view3D = GameObject.FindGameObjectWithTag("SpriteStudioView3D");
        }
    }

    IEnumerator SetView3D(Transform animation)
    {
        for (;;)
        {
            if (view3D != null)
            {
                animation.parent = view3D.transform;
                //Debug.Log("parent:"+ animation.parent.localPosition+ " /" + animation.parent.localRotation);
                //animation.localPosition += animation.parent.localPosition;
                //animation.localRotation *= animation.parent.localRotation;
                //animation.localPosition = Vector3.zero + screenPos;
                break;
            }
            yield return null;
        }
    }

    public Script_SpriteStudio_Root CreateAnimation(GameObject animation, string key, Vector3 worldPos)
    {
        if (scriptRoots.ContainsKey(key))
        {
            return scriptRoots[key];
        }

        //Debug.Log("pos:" + worldPos);
        //Debug.Log("WorldToScreenPoint:" + Camera.main.WorldToScreenPoint(screenPos));
        //Debug.Log("ScreenToWorldPoint:" + Camera.main.ScreenToWorldPoint(screenPos));
        //Debug.Log("rotation:" + animation.transform.rotation);
        //GameObject ob = (GameObject)Instantiate(animation, worldPos, Camera.main.transform.rotation);
        GameObject ob = (GameObject)Instantiate(animation, worldPos, Camera.main.transform.rotation);

        //Camera.main.WorldToScreenPoint
        //Camera.main.ScreenToWorldPoint

        StartCoroutine(SetView3D(ob.transform));
        Script_SpriteStudio_Root scriptRoot = Library_SpriteStudio.Utility.Parts.RootGetChild(ob);
        Stop(scriptRoot);

        scriptRoots[key] = scriptRoot;
        return scriptRoot;
    }
    public Script_SpriteStudio_Root CreateButtonFlash(GameObject targetBtn)
    {
        Vector3 pos = targetBtn.transform.position;
        //Debug.Log("btnPos : "+pos);
        //Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(pos));
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit))
        //{
        //    pos = hit.transform.position;
        //    Debug.Log("hitPos : " + pos);
        //}
        return CreateAnimation(buttonFlash, targetBtn.name, pos);
    }

    public void DeleteAnimation(string key)
    {
        if (!scriptRoots.ContainsKey(key)) return;

        Destroy(scriptRoots[key].gameObject);
        scriptRoots.Remove(key);
    }

    public void Play(Script_SpriteStudio_Root scriptRoot, int timesPlay = 1)
    {
        scriptRoot.FlagHideForce = false;
        scriptRoot.AnimationPlay(-1, timesPlay);
    }
    public void Stop(Script_SpriteStudio_Root scriptRoot)
    {
        scriptRoot.FlagHideForce = true;
        scriptRoot.AnimationStop();
    }
}

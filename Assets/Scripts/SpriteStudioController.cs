using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteStudioController : MonoBehaviour
{
    //リロード完了
    public string ANIMATION_BUTTON_FLASH = "ButtonFlash";
    //メッセージ
    public string ANIMATION_TEXT_WIN = "Win";
    public string ANIMATION_TEXT_LOSE = "Lose";


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

        GameObject ob = (GameObject)Instantiate(animation, worldPos, Camera.main.transform.rotation);

        StartCoroutine(SetView3D(ob.transform));
        Script_SpriteStudio_Root scriptRoot = Library_SpriteStudio.Utility.Parts.RootGetChild(ob);
        Stop(scriptRoot);

        scriptRoots[key] = scriptRoot;
        return scriptRoot;
    }

    public void DeleteAnimation(string key)
    {
        if (!scriptRoots.ContainsKey(key)) return;

        Destroy(scriptRoots[key].gameObject);
        scriptRoots.Remove(key);
    }

    public void Play(Script_SpriteStudio_Root scriptRoot, int timesPlay = 1, bool isHideEnd = false)
    {
        scriptRoot.FlagHideForce = false;
        scriptRoot.AnimationPlay(-1, timesPlay);
        if (isHideEnd)
        {
            //ループ終了チェック
            StartCoroutine(CheckAnimationEnd(scriptRoot));
        }
    }
    IEnumerator CheckAnimationEnd(Script_SpriteStudio_Root scriptRoot)
    {
        for (;;)
        {
            if (!scriptRoot.AnimationCheckPlay())
            {
                //終了
                scriptRoot.FlagHideForce = true;
                break;
            }
            yield return null;
        }
    }

    public void Stop(Script_SpriteStudio_Root scriptRoot)
    {
        scriptRoot.FlagHideForce = true;
        scriptRoot.AnimationStop();
    }

    private Vector3 GetObjPos(GameObject obj)
    {
        Vector3 pos = obj.transform.position;
        RectTransform objRectTran = obj.GetComponent<RectTransform>();
        if (objRectTran != null)
        {
            //中心を求める
            //Debug.Log("pos: " + pos);
            //Vector3 center = Camera.main.ScreenToWorldPoint(objRectTran.rect.center);
            //pos = new Vector3(center.x, center.y, pos.z);
            //Debug.Log("pos: " + pos);
            //Vector2 pivot = objRectTran.pivot;
            //Debug.Log("center: " + objRectTran.rect.center);
            //Debug.Log("world: " + );
            ////Debug.Log(objRectTran.sizeDelta);
            //////if (pivot.x != 0.5f)
            //////{
            ////Debug.Log("x: " + objRectTran.rect.x.ToString());
            ////Debug.Log("y: " + objRectTran.rect.y.ToString());
            //////    Debug.Log(pos);
            //////}
            //////if (pivot.y != 0.5f)
            //////{
            //////    pos += (pivot.y - 0.5f) * objRectTran.up * objRectTran.sizeDelta.y;
            //////    Debug.Log("height: " + objRectTran.rect.height.ToString());
            //////    Debug.Log(pos);
            //////}
        }

        return pos;
    }

    // ##### 各アニメーションごとのメソッド #####

    //ボタンFlash
    public Script_SpriteStudio_Root CreateButtonFlash(GameObject targetBtn)
    {
        Vector3 pos = GetObjPos(targetBtn);
        GameObject obj = (GameObject)Resources.Load(Common.Func.GetResourceAnimation(ANIMATION_BUTTON_FLASH));
        return CreateAnimation(obj, targetBtn.name, pos);
    }

    //メッセージ
    public Script_SpriteStudio_Root DispMessage(GameObject targetObj, string text)
    {
        Vector3 pos = GetObjPos(targetObj);
        GameObject obj = (GameObject)Resources.Load(Common.Func.GetResourceAnimation(text));
        if (obj == null) return null;

        return CreateAnimation(obj, text, pos);
    }

}

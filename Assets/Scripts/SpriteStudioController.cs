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

    private Vector3 GetObjPos(GameObject targetObj)
    {
        //Vector3 pos = targetObj.transform.position;
        RectTransform objRectTran = targetObj.GetComponent<RectTransform>();
        Vector3 pos = objRectTran.position;
        if (objRectTran.pivot != new Vector2(0.5f, 0.5f))
        {
            //RectTransform canvasRect = Camera.main.transform.GetComponentInChildren<RectTransform>();

            Vector2 defaultAncPos = objRectTran.anchoredPosition;
            Vector2 spritePos = defaultAncPos;
            spritePos.x += objRectTran.rect.width * (0.5f - objRectTran.pivot.x) * objRectTran.localScale.x;
            spritePos.y += objRectTran.rect.height * (0.5f - objRectTran.pivot.y) * objRectTran.localScale.y;
            objRectTran.anchoredPosition = spritePos;
            pos = objRectTran.position;
            objRectTran.anchoredPosition = defaultAncPos;
        }

        return pos;
    }

    // ##### 各アニメーションごとのメソッド #####

    //ボタンFlash
    public Script_SpriteStudio_Root CreateButtonFlash(GameObject targetBtn)
    {
        GameObject obj = (GameObject)Resources.Load(Common.Func.GetResourceAnimation(ANIMATION_BUTTON_FLASH));
        Vector3 pos = GetObjPos(targetBtn);
        return CreateAnimation(obj, targetBtn.name, pos);
    }

    //メッセージ
    public Script_SpriteStudio_Root DispMessage(GameObject targetObj, string text)
    {
        GameObject obj = (GameObject)Resources.Load(Common.Func.GetResourceAnimation(text));
        Vector3 pos = GetObjPos(targetObj);
        if (obj == null) return null;

        return CreateAnimation(obj, text, pos);
    }

}

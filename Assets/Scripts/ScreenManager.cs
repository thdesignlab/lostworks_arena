using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScreenManager : Photon.MonoBehaviour
{
    private Transform myTran;
    private Image fadeImg;
    private Text messageText;

    [SerializeField]
    private float fadeTime = 0.5f;

    private bool isUiFade = false;

    void Awake()
    {
        myTran = transform;
        Transform fadeTran = myTran.FindChild("FadeImage");
        if (fadeTran != null)
        {
            fadeImg = fadeTran.GetComponent<Image>();
        }
    }

    public void Load(string sceneName, string message = "")
    {
        StartCoroutine(LoadProccess(sceneName, message));
    }

    IEnumerator LoadProccess(string sceneName, string message = "")
    {
        Image[] imgs = new Image[] { fadeImg };

        //メッセージ表示
        if (message != "") DialogController.OpenMessage(message);

        //フェードアウト
        Coroutine fadeOut = StartCoroutine(Fade(imgs, false));
        yield return fadeOut;

        //シーンロード
        PhotonNetwork.LoadLevel(sceneName);

        //フェードイン
        Coroutine fadeIn = StartCoroutine(Fade(imgs, true));
        yield return fadeIn;

        //メッセージ非表示
        if (message != "") DialogController.CloseMessage();
    }

    IEnumerator Fade(Image[] imgs, bool isFadeIn, bool isBlackOut = true)
    {
        if (imgs.Length == 0 || fadeTime <= 0) yield break;

        Color alphaZero = new Color(0, 0, 0, 0);
        Color alphaOne = new Color(0, 0, 0, 1);
        if (!isBlackOut)
        {
            alphaOne = new Color(1, 1, 1, 0);
            alphaZero = new Color(1, 1, 1, 1);
        }

        float procTime = 0;
        for (;;)
        {
            procTime += Time.deltaTime;
            float procRate = procTime / fadeTime;
            if (procRate > 1) procRate = 1;
            if (isFadeIn)
            {
                //フェードイン
                foreach (Image img in imgs)
                {
                    if (!IsFadeImage(img)) continue;
                    img.color = Color.Lerp(alphaOne, alphaZero, procRate);
                }
            }
            else
            {
                //フェードアウト
                foreach (Image img in imgs)
                {
                    if (!IsFadeImage(img)) continue;
                    img.color = Color.Lerp(alphaZero, alphaOne, procRate);
                }
            }
            if (procRate >= 1) break;
            yield return null;
        }
    }

    public void FadeUI(GameObject fadeOutObj, GameObject fadeInObj, bool isChild = true)
    {
        StartCoroutine(LoadUIProccess(fadeOutObj, fadeInObj, isChild));
    }

    public void FadeUI(GameObject uiObj, bool isFadeIn, bool isChild = true)
    {
        GameObject fadeOutObj = null;
        GameObject fadeInObj = null;
        if (isFadeIn)
        {
            fadeInObj = uiObj;
        }
        else
        {
            fadeOutObj = uiObj;
        }
        StartCoroutine(LoadUIProccess(fadeOutObj, fadeInObj, isChild));
    }

    IEnumerator LoadUIProccess(GameObject fadeOutObj, GameObject fadeInObj, bool isChild)
    {
        if (isUiFade)
        {
            //Debug.Log("### wait fade start ###");
            for (;;)
            {
                if (!isUiFade) break;
                yield return null;
            }
            //Debug.Log("### wait fade end ###");
        }

        isUiFade = true;
        if (fadeOutObj != null)
        {
            //Debug.Log("Fade out start:"+ fadeOutObj.name);
            //フェードアウト
            Image[] fadeOutImgs;
            if (isChild)
            {
                fadeOutImgs = GetComponentsInChildrenWithoutSelf<Image>(fadeOutObj.transform);
            }
            else
            {
                fadeOutImgs = fadeOutObj.transform.GetComponents<Image>();
            }
            Coroutine fadeOut = StartCoroutine(Fade(fadeOutImgs, false, false));
            yield return fadeOut;
            fadeOutObj.SetActive(false);
            //Debug.Log("Fade out end:" + fadeOutObj.name);
        }

        if (fadeInObj != null)
        {
            //Debug.Log("Fade in start :" + fadeInObj.name);
            //フェードイン
            Image[] fadeInImgs;
            if (isChild)
            {
                fadeInImgs = GetComponentsInChildrenWithoutSelf<Image>(fadeInObj.transform);
            }
            else
            {
                fadeInImgs = fadeInObj.transform.GetComponents<Image>();
            }
            Coroutine fadeIn = StartCoroutine(Fade(fadeInImgs, true, false));
            fadeInObj.SetActive(true);
            //Debug.Log("Fade in end:" + fadeInObj.name);
            yield return fadeIn;
        }
        isUiFade = false;
    }

    public static T[] GetComponentsInChildrenWithoutSelf<T>(Transform self)
    {
        List<T> compList = new List<T>(); 
        foreach (Transform child in self)
        {
            T comp = child.GetComponent<T>();
            if (comp != null) compList.Add(comp);
        }
        return compList.ToArray();
    }

    private bool IsFadeImage(Image img)
    {
        if (img == fadeImg) return true;
        if (img.sprite == null) return false;

        switch (img.sprite.name)
        {
            case "Background":
                return false;
        }

        return true;
    } 
}


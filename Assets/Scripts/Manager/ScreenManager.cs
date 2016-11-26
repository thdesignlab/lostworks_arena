using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class ScreenManager : SingletonMonoBehaviour<ScreenManager>
{
    private Transform myTran;
    private Image fadeImg;
    private Text messageText;

    [SerializeField]
    private float fadeTime = 0.5f;

    public bool isSceneFade = false;
    public bool isUiFade = false;

    protected override void Awake()
    {
        base.Awake();

        myTran = transform;
        Transform fadeTran = myTran.FindChild("FadeImage");
        fadeImg = fadeTran.GetComponent<Image>();
        fadeImg.raycastTarget = false;
    }

    public void Load(string sceneName, string message = "")
    {
        StartCoroutine(LoadProccess(sceneName, message));
    }

    IEnumerator LoadProccess(string sceneName, string message = "")
    {
        Image[] imgs = new Image[] { fadeImg };

        isSceneFade = true;

        //メッセージ表示
        if (message != "") DialogController.OpenMessage(message, DialogController.MESSAGE_POSITION_RIGHT);

        //フェードアウト
        Coroutine fadeOut = StartCoroutine(Fade(imgs, false));
        yield return fadeOut;

        //BGM停止
        SoundManager.Instance.StopBgm(sceneName);

        //シーンロード
        PhotonNetwork.LoadLevel(sceneName);

        //フェードイン
        Coroutine fadeIn = StartCoroutine(Fade(imgs, true));
        yield return fadeIn;

        //BGM再生
        SoundManager.Instance.PlayBgm(sceneName);

        //メッセージ非表示
        if (message != "") DialogController.CloseMessage();
        isSceneFade = false;
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
            Color startColor;
            Color endColor;
            foreach (Image img in imgs)
            {
                if (!IsFadeImage(img)) continue;
                img.raycastTarget = isBlackOut;

                if (isFadeIn)
                {
                    //フェードイン
                    startColor = alphaOne;
                    endColor = alphaZero;
                }
                else
                {
                    //フェードアウト
                    startColor = alphaZero;
                    endColor = alphaOne;
                }
                img.color = Color.Lerp(startColor, endColor, procRate);
                if (procRate >= 1) img.raycastTarget = !isBlackOut;
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
            for (;;)
            {
                if (!isUiFade) break;
                yield return null;
            }
        }

        isUiFade = true;
        //if (isLoadMessage) DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

        if (fadeOutObj != null)
        {
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
        }

        if (fadeInObj != null)
        {
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
            yield return fadeIn;
        }

        //if (isLoadMessage) DialogController.CloseMessage();
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

    public void TextFadeOut(Text obj, float time = -1)
    {
        if (time <= 0) time = fadeTime;
        StartCoroutine(TextFadeOutProc(obj, time));
    }
    IEnumerator TextFadeOutProc(Text obj, float time)
    {
        float startAlpha = obj.color.a;
        float nowAlpha = startAlpha;
        for (;;)
        {
            nowAlpha -= Time.deltaTime / time * startAlpha;
            obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, nowAlpha);
            if (nowAlpha <= 0) break;
            yield return null;
        }
    }

    public void ImageFadeOut(Image obj, float time = -1)
    {
        if (time <= 0) time = fadeTime;
        StartCoroutine(ImageFadeOutProc(obj, time));
    }
    IEnumerator ImageFadeOutProc(Image obj, float time)
    {
        float startAlpha = obj.color.a;
        float nowAlpha = startAlpha;
        for (;;)
        {
            nowAlpha -= Time.deltaTime / time * startAlpha;
            obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, nowAlpha);
            if (nowAlpha <= 0) break;
            yield return null;
        }
    }
}


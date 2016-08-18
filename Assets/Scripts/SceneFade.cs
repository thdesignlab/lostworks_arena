﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneFade : Photon.MonoBehaviour
{
    private Transform myTran;
    private Image fadeImg;
    private Text messageText;

    [SerializeField]
    private float fadeTime = 0.5f;

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
        //メッセージ表示
        if (message != "") DialogController.OpenMessage(message);

        //フェードアウト
        Coroutine fadeOut = StartCoroutine(Fade(false));
        yield return fadeOut;

        //シーンロード
        PhotonNetwork.LoadLevel(sceneName);

        //フェードイン
        Coroutine fadeIn = StartCoroutine(Fade(true));
        yield return fadeIn;

        //メッセージ非表示
        if (message != "") DialogController.CloseMessage();
    }

    IEnumerator Fade(bool isFadeIn)
    {
        if (fadeImg == null || fadeTime <= 0) yield break;

        Color alphaZero = new Color(0, 0, 0, 0);
        Color alphaOne = new Color(0, 0, 0, 1);
        float procTime = 0;
        for (;;)
        {
            procTime += Time.deltaTime;
            float procRate = procTime / fadeTime;
            if (procRate > 1) procRate = 1;
            if (isFadeIn)
            {
                //フェードイン
                fadeImg.color = Color.Lerp(alphaOne, alphaZero, procRate);
            }
            else
            {
                //フェードアウト
                fadeImg.color = Color.Lerp(alphaZero, alphaOne, procRate);
            }
            if (procRate >= 1) break;
            yield return null;
        }
    }
}

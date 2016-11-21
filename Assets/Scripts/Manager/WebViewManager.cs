using UnityEngine;
using System.Collections;

public class WebViewManager : SingletonMonoBehaviour<WebViewManager>
{
    [SerializeField]
    private WebViewObject webViewObject;
    [SerializeField]
    private GameObject webViewCanvas;

    public void Open(string url)
    {
        if (webViewObject == null) return;
        try
        {
            webViewObject.Init();
            webViewObject.LoadURL(url + Common.CO.WEBVIEW_KEY);
            webViewObject.SetMargins(0, 0, 0, 110);
            webViewObject.SetVisibility(true);
            webViewCanvas.SetActive(true);
        }
        catch
        {
            Application.OpenURL(url);
        }
    }

    public void Close()
    {
        webViewCanvas.SetActive(false);
        webViewObject.SetVisibility(false);
    }
}
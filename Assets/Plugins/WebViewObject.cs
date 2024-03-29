/*
 * Copyright (C) 2011 Keijiro Takahashi
 * Copyright (C) 2012 GREE, Inc.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_EDITOR || UNITY_STANDALONE_OSX
using System.IO;
using System.Text.RegularExpressions;
#endif

using Callback = System.Action<string>;

#if UNITY_EDITOR || UNITY_STANDALONE_OSX
public class UnitySendMessageDispatcher
{
    public static void Dispatch(string name, string method, string message)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
            obj.SendMessage(method, message);
    }
}
#endif

public class WebViewObject : MonoBehaviour
{
    Callback onJS;
    Callback onError;
    Callback onLoaded;
    bool visibility;
#if UNITY_WEBPLAYER
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
    IntPtr webView;
    Rect rect;
    Texture2D texture;
    string inputString;
    bool hasFocus;
#elif UNITY_IPHONE
    IntPtr webView;
#elif UNITY_ANDROID
    AndroidJavaObject webView;
    
    bool mIsKeyboardVisible = false;
    
    /// Called from Java native plugin to set when the keyboard is opened
    public void SetKeyboardVisible(string pIsVisible)
    {
        mIsKeyboardVisible = (pIsVisible == "true");
    }
#endif
    
    public bool IsKeyboardVisible {
        get {
#if !UNITY_EDITOR && UNITY_ANDROID
            return mIsKeyboardVisible;
#elif !UNITY_EDITOR && UNITY_IPHONE
            return TouchScreenKeyboard.visible;
#else
            return false;
#endif
        }
    }

#if UNITY_EDITOR || UNITY_STANDALONE_OSX
    [DllImport("WebView")]
    private static extern string _CWebViewPlugin_GetAppPath();
    [DllImport("WebView")]
    private static extern IntPtr _CWebViewPlugin_Init(
        string gameObject, bool transparent, int width, int height, string ua, bool ineditor);
    [DllImport("WebView")]
    private static extern int _CWebViewPlugin_Destroy(IntPtr instance);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_SetRect(
        IntPtr instance, int width, int height);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_SetVisibility(
        IntPtr instance, bool visibility);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_LoadURL(
        IntPtr instance, string url);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_LoadHTML(
        IntPtr instance, string html, string baseUrl);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_EvaluateJS(
        IntPtr instance, string url);
    [DllImport("WebView")]
    private static extern bool _CWebViewPlugin_CanGoBack(
        IntPtr instance);
    [DllImport("WebView")]
    private static extern bool _CWebViewPlugin_CanGoForward(
        IntPtr instance);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_GoBack(
        IntPtr instance);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_GoForward(
        IntPtr instance);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_Update(IntPtr instance,
        int x, int y, float deltaY, bool down, bool press, bool release,
        bool keyPress, short keyCode, string keyChars);
    [DllImport("WebView")]
    private static extern int _CWebViewPlugin_BitmapWidth(IntPtr instance);
    [DllImport("WebView")]
    private static extern int _CWebViewPlugin_BitmapHeight(IntPtr instance);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_SetTextureId(IntPtr instance, int textureId);
    [DllImport("WebView")]
    private static extern void _CWebViewPlugin_SetCurrentInstance(IntPtr instance);
    [DllImport("WebView")]
    private static extern IntPtr GetRenderEventFunc();
#elif UNITY_IPHONE
    [DllImport("__Internal")]
    private static extern IntPtr _CWebViewPlugin_Init(string gameObject, bool transparent, bool enableWKWebView);
    [DllImport("__Internal")]
    private static extern int _CWebViewPlugin_Destroy(IntPtr instance);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_SetMargins(
        IntPtr instance, int left, int top, int right, int bottom);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_SetVisibility(
        IntPtr instance, bool visibility);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_LoadURL(
        IntPtr instance, string url);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_LoadHTML(
        IntPtr instance, string html, string baseUrl);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_EvaluateJS(
        IntPtr instance, string url);
    [DllImport("__Internal")]
    private static extern bool _CWebViewPlugin_CanGoBack(
        IntPtr instance);
    [DllImport("__Internal")]
    private static extern bool _CWebViewPlugin_CanGoForward(
        IntPtr instance);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_GoBack(
        IntPtr instance);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_GoForward(
        IntPtr instance);
    [DllImport("__Internal")]
    private static extern void _CWebViewPlugin_SetFrame(
        IntPtr instance, int x , int y , int width , int height);
#endif

    public void Init(Callback cb = null, bool transparent = false, string ua = @"Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D257 Safari/9537.53", Callback err = null, Callback ld = null, bool enableWKWebView = false)
    {
        onJS = cb;
        onError = err;
        onLoaded = ld;
#if UNITY_WEBPLAYER
        Application.ExternalCall("unityWebView.init", name);
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
        throw new Exception();
#elif UNITY_IPHONE
        webView = _CWebViewPlugin_Init(name, transparent, enableWKWebView);
#elif UNITY_ANDROID
        webView = new AndroidJavaObject("net.gree.unitywebview.CWebViewPlugin");
        webView.Call("Init", name, transparent);
#endif
    }

    protected virtual void OnDestroy()
    {
#if UNITY_WEBPLAYER
        Application.ExternalCall("unityWebView.destroy", name);
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_Destroy(webView);
        webView = IntPtr.Zero;
#elif UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_Destroy(webView);
        webView = IntPtr.Zero;
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("Destroy");
        webView = null;
#endif
    }

    /** Use this function instead of SetMargins to easily set up a centered window */
    public void SetCenterPositionWithScale(Vector2 center , Vector2 scale)
    {
#if UNITY_WEBPLAYER
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
        rect.x = center.x + (Screen.width - scale.x)/2;
        rect.y = center.y + (Screen.height - scale.y)/2;
        rect.width = scale.x;
        rect.height = scale.y;
#elif UNITY_IPHONE
        if (webView == IntPtr.Zero) return;
        _CWebViewPlugin_SetFrame(webView,(int)center.x,(int)center.y,(int)scale.x,(int)scale.y);
#endif
    }

    public void SetMargins(int left, int top, int right, int bottom)
    {
#if UNITY_WEBPLAYER
        Application.ExternalCall("unityWebView.setMargins", name, left, top, right, bottom);
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
        if (webView == IntPtr.Zero)
            return;
        int width = Screen.width - (left + right);
        int height = Screen.height - (bottom + top);
        _CWebViewPlugin_SetRect(webView, width, height);
        rect = new Rect(left, bottom, width, height);
#elif UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_SetMargins(webView, left, top, right, bottom);
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("SetMargins", left, top, right, bottom);
#endif
    }

    public void SetVisibility(bool v)
    {
#if UNITY_WEBPLAYER
        Application.ExternalCall("unityWebView.setVisibility", name, v);
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_SetVisibility(webView, v);
#elif UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_SetVisibility(webView, v);
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("SetVisibility", v);
#endif
        visibility = v;
    }

    public bool GetVisibility()
    {
        return visibility;
    }

    public void LoadURL(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;
#if UNITY_WEBPLAYER
        Application.ExternalCall("unityWebView.loadURL", name, url);
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_LoadURL(webView, url);
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("LoadURL", url);
#endif
    }

    public void LoadHTML(string html, string baseUrl)
    {
        if(string.IsNullOrEmpty(html))
            return;
        if(string.IsNullOrEmpty(baseUrl))
            baseUrl = "";
#if UNITY_WEBPLAYER
        //TODO: UNSUPPORTED
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_LoadHTML(webView, html, baseUrl);
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("LoadHTML", html, baseUrl);
#endif
    }

    public void EvaluateJS(string js)
    {
#if UNITY_WEBPLAYER
        Application.ExternalCall("unityWebView.evaluateJS", name, js);
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_EvaluateJS(webView, js);
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("LoadURL", "javascript:" + js);
#endif
    }

    public bool CanGoBack()
    {
#if UNITY_WEBPLAYER
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return false;
        return _CWebViewPlugin_CanGoBack(webView);
#elif UNITY_ANDROID
        if (webView == null)
            return false;
        return webView.Get<bool>("canGoBack");
#endif
    }

    public bool CanGoForward()
    {
#if UNITY_WEBPLAYER
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return false;
        return _CWebViewPlugin_CanGoForward(webView);
#elif UNITY_ANDROID
        if (webView == null)
            return false;
        return webView.Get<bool>("canGoForward");
#endif
    }

    public void GoBack()
    {
#if UNITY_WEBPLAYER
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_GoBack(webView);
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("GoBack");
#endif
    }

    public void GoForward()
    {
#if UNITY_WEBPLAYER
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_IPHONE
        if (webView == IntPtr.Zero)
            return;
        _CWebViewPlugin_GoForward(webView);
#elif UNITY_ANDROID
        if (webView == null)
            return;
        webView.Call("GoForward");
#endif
    }

    public void CallOnError(string error)
    {
        if (onError != null) {
            onError(error);
        }
    }

    public void CallOnLoaded(string url)
    {
        if (onLoaded != null) {
            onLoaded(url);
        }
    }

    public void CallFromJS(string message)
    {
        if (onJS != null) {
#if !UNITY_ANDROID
            message = WWW.UnEscapeURL(message);
#endif
            onJS(message);
        }
    }

#if UNITY_WEBPLAYER
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
    void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;
    }

    void Update()
    {
        if (hasFocus) {
            inputString += Input.inputString;
        }
    }

    void OnGUI()
    {
        if (webView == IntPtr.Zero || !visibility)
            return;

        Vector3 pos = Input.mousePosition;
        bool down = Input.GetButton("Fire1");
        bool press = Input.GetButtonDown("Fire1");
        bool release = Input.GetButtonUp("Fire1");
        float deltaY = Input.GetAxis("Mouse ScrollWheel");
        bool keyPress = false;
        string keyChars = "";
        short keyCode = 0;
        if (inputString != null && inputString.Length > 0) {
            keyPress = true;
            keyChars = inputString.Substring(0, 1);
            keyCode = (short)inputString[0];
            inputString = inputString.Substring(1);
        }
        _CWebViewPlugin_Update(webView,
            (int)(pos.x - rect.x), (int)(pos.y - rect.y), deltaY,
            down, press, release, keyPress, keyCode, keyChars);
        {
            var w = _CWebViewPlugin_BitmapWidth(webView);
            var h = _CWebViewPlugin_BitmapHeight(webView);
            if (texture == null || texture.width != w || texture.height != h) {
                texture = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
            }
        }
        _CWebViewPlugin_SetTextureId(webView, (int)texture.GetNativeTexturePtr());
        _CWebViewPlugin_SetCurrentInstance(webView);
#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
        GL.IssuePluginEvent(-1);
#else
        GL.IssuePluginEvent(GetRenderEventFunc(), -1);
#endif
        Matrix4x4 m = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, Screen.height, 0),
            Quaternion.identity, new Vector3(1, -1, 1));
        GUI.DrawTexture(rect, texture);
        GUI.matrix = m;
    }
#endif
}

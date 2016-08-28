using UnityEngine;
using System.Collections;

public class AspectKeeper : Photon.MonoBehaviour
{
    public bool isUpdate = false;

    private RectTransform _rt;
    private Vector2 _originSizeDelta;
    private float _aspect;
    private float _screenAspect;

    void Start()
    {
        _rt = GetComponent<RectTransform>();
        _originSizeDelta = _rt.sizeDelta;
        _aspect = _rt.sizeDelta.x / _rt.sizeDelta.y;

        keepAspect();
    }

    void Update()
    {
        if (isUpdate)
            keepAspect();
    }

    private void keepAspect()
    {
        _rt.anchoredPosition = Vector2.zero;

        _screenAspect = (float)Screen.width / (float)Screen.height;
        // 画像のアスペクト比と画面のアスペクト比を比較します
        if (_aspect > _screenAspect)
        {
            // 縦長の画面の時
            float w = (float)Screen.width;
            float h = w / _aspect;
            _rt.sizeDelta = new Vector2(w, h);
        }
        else
        {
            // 横長の画面の時
            float h = (float)Screen.height;
            float w = h * _aspect;
            _rt.sizeDelta = new Vector2(w, h);
        }
    }
}

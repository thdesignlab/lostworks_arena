using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class StoreManager : SingletonMonoBehaviour<StoreManager>
{
    [SerializeField]
    private GameObject storeCanvas;

    //private int dailyPlayCount = 10;
    private int minPoint = 10;
    private int maxPoint = 50;

    protected override void Awake()
    {
        base.Awake();

        storeCanvas.SetActive(false);
    }

    public void OpenStore()
    {
        //ダイアログ表示
        ScreenManager.Instance.FadeUI(storeCanvas.gameObject, true);
        SoundManager.Instance.PlayStoreBgm();
    }

    public void CloseConfig()
    {
        //ダイアログ非表示
        ScreenManager.Instance.FadeUI(storeCanvas.gameObject, false);
        SoundManager.Instance.PlayBgm();
        GameObject.Find("PhotonManager").GetComponent<PhotonManager>().ReturnModeSelect();
    }

    public void PlayGacha()
    {
        System.Action onFinish = () => AddPoint(minPoint, maxPoint);
        System.Action onFailed = () => DialogController.OpenDialog("失敗しました…");
        System.Action onSkipped = () => AddPoint(minPoint, maxPoint / 2);
        UnityAds.Instance.Play(null, null, onFinish, onFailed, onSkipped);
    }

    public void AddPoint(int minPt, int maxPt)
    {
        int getPt = Random.Range(minPt, maxPt + 1);
        DialogController.OpenDialog(getPt+"pt獲得!!", "もう一度", () => PlayGacha(), true);
    }
}

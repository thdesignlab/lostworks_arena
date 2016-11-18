using UnityEngine;
using System.Collections;

public class MyDebug : SingletonMonoBehaviour<MyDebug>
{
    [SerializeField]
    public bool isDebugMode;

    [SerializeField]
    private GUISkin guiSkin;

    private Queue logQueue = new Queue();
    private int logCount = 100;
    private int btnDownTime = 3;
    private float btnDown = 0;
    private bool dispLog = false;

    public void AdminLog(object key, object value)
    {
        AdminLog(key + " >> " + value);
    }
    public void AdminLog(object log)
    {
        if (UserManager.isAdmin || Instance.isDebugMode) Debug.Log(log);
    }

    void OnEnable()
    {
        if (!isDebugMode && !UserManager.isAdmin) return;
        //Application.RegisterLogCallback(HandleLog);
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        //Application.RegisterLogCallback(null);
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string condition, string stackTrace, LogType type)
    {
        //stackTrace += "\n"+UnityEngine.StackTraceUtility.ExtractStackTrace();
        // 必要な変数を宣言する
        //string dtNow = System.DateTime.Now.ToString("yyyy/MM/dd (ddd) HH:mm:ss");
        string dtNow = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        string log = "### START ### -- "+ type.ToString() + " -- " + dtNow + "\ncondition : " + condition + "\nstackTrace : " + stackTrace + "\n### END ###\n";
        //string log = "### START ### -- " + dtNow + "\n" + stackTrace + "\ntype : " + type.ToString() + "\n### END ###\n";
        PushLog(log, false);
    }

    private int textAreaWidth = Screen.width;
    private int textAreaheight = Screen.height / 2;
    private int space = Screen.height / 16;

    void OnGUI()
    {
        if (!isDebugMode && !UserManager.isAdmin) return;
        Rect btnRect = new Rect(0, 0, space, space);
        Rect logRect = new Rect(0, space, textAreaWidth, textAreaheight);

        if (dispLog)
        {
            //ログ表示中
            string logText = "";
            foreach (string log in logQueue)
            {
                logText = log + logText;
            }
            GUI.TextArea(logRect, logText);
            if (GUI.Button(btnRect, "-"))
            {
                dispLog = false;
                btnDown = 0;
            }
        }
        else
        {
            SetGuiSkin(GUI.skin);
            GUI.skin = guiSkin;

            //ログ非表示中
            if (GUI.RepeatButton(btnRect, "", "button"))
            {
                btnDown += Time.deltaTime;
                if (btnDown >= btnDownTime)
                {
                    dispLog = true;
                }
            }
            else
            {
                //btnDown = 0;
                //btnDown -= Time.deltaTime / 10;
            }
        }
    }

    private void SetGuiSkin(GUISkin defaultGuiSKin)
    {
        if (guiSkin == null)
        {
            guiSkin = Instantiate(defaultGuiSKin);
            guiSkin.button.normal.background = null;
            guiSkin.button.hover.background = null;
            guiSkin.button.active.background = null;
        }
    }

    /**
     * @brief ログのプッシュ(エンキュー)
     * @param str プッシュするログ
     * @param console trueならばUnityのコンソール上にも表示する
     */
    public void PushLog(string str, bool console = true)
    {
        if (logQueue.Count >= logCount) logQueue.Dequeue();
        
        logQueue.Enqueue(str);
        if (console) Debug.Log(str);
    }
}
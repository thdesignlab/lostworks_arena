using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogController : MonoBehaviour
{
    //ダイアログ
    private static GameObject dialog;
    private static Text textMessage;

    //メッセージ
    private static GameObject messageObj;
    private static Image messageImage;
    private static Text messageText;

    const string RESOURCE_DIALOG = "UI/Dialog";
    const string RESOURCE_MESSAGE = "UI/Message";

    //メッセージ配置
    public const int MESSAGE_POSITION_CENTER = 0;
    public const int MESSAGE_POSITION_LEFT = 1;
    public const int MESSAGE_POSITION_RIGHT = 2;
    private static Dictionary<int, Vector2> messageImagePositionPivots = new Dictionary<int, Vector2>()
    {
        { MESSAGE_POSITION_CENTER, new Vector2(0.5f, 0.0f)},
        { MESSAGE_POSITION_LEFT, new Vector2(0.0f, 0.0f)},
        { MESSAGE_POSITION_RIGHT, new Vector2(1.0f, 0.0f)},
    };
    private static Dictionary<int, TextAnchor> messageTextPositionPivots = new Dictionary<int, TextAnchor>()
    {
        { MESSAGE_POSITION_CENTER, TextAnchor.MiddleCenter},
        { MESSAGE_POSITION_LEFT, TextAnchor.MiddleLeft},
        { MESSAGE_POSITION_RIGHT, TextAnchor.MiddleRight},
    };

    //メッセージ
    public const string MESSAGE_TOP = "Tap to Start";
    public const string MESSAGE_CONNECT = "Connecting...";
    public const string MESSAGE_LOADING = "Now Loading...";
    public const string MESSAGE_CREATE_ROOM = "Create Room";
    public const string MESSAGE_JOIN_ROOM = "Join Room";
    public const string MESSAGE_SEARCH_ROOM = "Search Room";

    private static string MESSAGE_IMAGE_PARENT = "LoadMessage";
    private static Dictionary<string, string> messageImgNameDic = new Dictionary<string, string>()
    {
        { MESSAGE_TOP, "_1" },
        { MESSAGE_CONNECT, "_0" },
        { MESSAGE_LOADING, "_2" },
        { MESSAGE_CREATE_ROOM, "_2" },
        { MESSAGE_JOIN_ROOM, "_2" },
        { MESSAGE_SEARCH_ROOM, "_2" },
    };

    //ボタン
    const string BUTTON_OK_TEXT = "OK";
    const string BUTTON_CANCEL_TEXT = "Cancel";

    //##### 広告ダイアログ表示 #####
    public static void OpenDialogAd()
    {
        //OpenDialog("");
        //UnityAds.Instance.Play();
    }

    //##### ダイアログ表示 #####
    public static GameObject OpenDialog(string text, UnityAction okAction = null, bool isCancel = false)
    {
        return OpenDialog(text, BUTTON_OK_TEXT, okAction, isCancel);
    }
    public static GameObject OpenDialog(string text, string btnText, UnityAction okAction, bool isCancel)
    {
        return OpenDialog(text, new List<string>() { btnText }, new List<UnityAction>() { okAction }, isCancel);
    }
    public static GameObject OpenDialog(string text, UnityAction okAction, UnityAction cancelAction, bool isCancel = false)
    {
        return OpenDialog(text, new List<string>() { BUTTON_OK_TEXT, BUTTON_CANCEL_TEXT }, new List<UnityAction>() { okAction, cancelAction }, isCancel);
    }
    public static GameObject OpenDialog(string text, List<string> buttons, List<UnityAction> actions, bool isCancel = false)
    {
        if (dialog != null) CloseDialog();
        if (buttons.Count <= 0 || actions.Count <= 0) return null;

        dialog = Instantiate((GameObject)Resources.Load(RESOURCE_DIALOG));
        ScreenManager.Instance.FadeDialog(dialog, true);
        Text textMessage = dialog.transform.FindChild("DialogArea/Message").GetComponent<Text>();
        GameObject buttonOkObj = dialog.transform.FindChild("DialogArea/ButtonArea/OK").gameObject;
        GameObject buttonCancelObj = dialog.transform.FindChild("DialogArea/ButtonArea/Cancel").gameObject;

        //テキスト設定
        textMessage.text = text;

        //OKボタン
        SetBtn(buttonOkObj, buttons[0], actions[0]);
             
        //Cancelボタン
        if (isCancel || buttons.Count >= 2)
        {
            string btnText = BUTTON_CANCEL_TEXT;
            UnityAction btnAction = null;
            if (buttons.Count >= 2)
            {
                btnText = buttons[1];
                btnAction = actions[1];
            }
            SetBtn(buttonCancelObj, btnText, btnAction);
            buttonCancelObj.SetActive(true);
        }
        else
        {
            buttonCancelObj.SetActive(false);
        }
        return dialog;
    }

    public static void CloseDialog()
    {
        ScreenManager.Instance.FadeDialog(dialog, false);
        //Destroy(dialog);
    }

    private static void SetBtn(GameObject btnObj, string btnText, UnityAction btnAction)
    {
        btnObj.GetComponent<Button>().onClick.AddListener(() => OnClickButton(btnAction));
        Text buttonOkText = btnObj.transform.GetComponentInChildren<Text>();
        if (buttonOkText != null) buttonOkText.text = btnText;
    }

    public static void OnClickButton(UnityAction unityAction = null)
    {
        if (unityAction != null)
        {
            unityAction.Invoke();
        }
        CloseDialog();
    }

    public static GameObject OpenMessage(string text, int msgPos = MESSAGE_POSITION_CENTER)
    {
        if (text == "")
        {
            CloseMessage();
            return null;
        }

        if (messageObj == null)
        {
            messageObj = Instantiate((GameObject)Resources.Load(RESOURCE_MESSAGE));
            RectTransform messageImageTran = messageObj.transform.FindChild("Image").GetComponent<RectTransform>();
            messageImageTran.pivot = messageImagePositionPivots[msgPos];
            messageImage = messageImageTran.GetComponent<Image>();
            RectTransform messageTextTran = messageObj.transform.FindChild("Text").GetComponent<RectTransform>();
            messageText = messageTextTran.GetComponent<Text>();
            messageText.alignment = messageTextPositionPivots[msgPos]; ;
        }

        if (messageText.text == text) return messageObj;

        Sprite image = GetMessageImage(text);
        messageText.text = text;
        if (image != null)
        {
            //画像
            messageImage.sprite = image;
            messageImage.enabled = true;
            messageText.enabled = false;
        }
        else
        {
            //テキスト
            messageImage.sprite = null;
            messageImage.enabled = false;
            messageText.enabled = true;
        }

        return messageObj;
    }

    public static void CloseMessage()
    {
        if (messageObj == null) return;
        Destroy(messageObj);
    }
    
    private static Sprite GetMessageImage(string text)
    {
        Sprite image = null;
        if (messageImgNameDic.ContainsKey(text))
        {
            string imageName = MESSAGE_IMAGE_PARENT + messageImgNameDic[text];
            Sprite[] sprites = Resources.LoadAll<Sprite>(Common.Func.GetResourceSprite(MESSAGE_IMAGE_PARENT));
            image = System.Array.Find<Sprite>(sprites, (sprite) => sprite.name.Equals(imageName));
        }
        return image;
    }

    public static Image GetMessageImageObj()
    {
        return messageImage;
    }
    public static Text GetMessageTextObj()
    {
        return messageText;
    }
}

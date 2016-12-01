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

    const string RESOURCE_SELECT_DIALOG = "UI/SelectDialog";
    const string RESOURCE_SELECT_BUTTON = "UI/SelectDialogButton";
    const string RESOURCE_DIALOG = "UI/Dialog";
    const string RESOURCE_MESSAGE = "UI/Message";
    const string RESOURCE_IMAGE_DIR = "Image/";

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

    private static string MESSAGE_IMAGE_PARENT = RESOURCE_IMAGE_DIR + "Message/";
    private static string MESSAGE_IMAGE_TOP = "taptostart";
    private static string MESSAGE_IMAGE_LOADING = "nowloading";
    private static string MESSAGE_IMAGE_CONNECTING = "connecting";
    private static Dictionary<string, string> messageImgNameDic = new Dictionary<string, string>()
    {
        { MESSAGE_TOP, MESSAGE_IMAGE_TOP },
        { MESSAGE_CONNECT, MESSAGE_IMAGE_CONNECTING },
        { MESSAGE_LOADING, MESSAGE_IMAGE_LOADING },
        { MESSAGE_CREATE_ROOM, MESSAGE_IMAGE_LOADING },
        { MESSAGE_JOIN_ROOM, MESSAGE_IMAGE_LOADING },
        { MESSAGE_SEARCH_ROOM, MESSAGE_IMAGE_LOADING },
    };
    private static Dictionary<string, Sprite> messageImgDic = null;

    //ボタン
    const string BUTTON_OK_TEXT = "OK";
    const string BUTTON_CANCEL_TEXT = "Cancel";

    //ボタンText色
    public static Color cancelColor = new Color32(255, 255, 255, 255);
    public static Color blueColor = new Color32(0, 255, 234, 255);
    public static Color redColor = new Color32(255, 79, 79, 255);
    public static Color yellowColor = new Color32(238, 255, 79, 255);
    public static Color greenColor = new Color32(79, 255, 109, 255);
    public static Color purpleColor = new Color32(255, 79, 221, 255);

    //##### 選択ダイアログ表示 #####
    public static GameObject OpenSelectDialog(string text, Dictionary<string, UnityAction> btnInfoDic, bool isCancel)
    {
        return OpenSelectDialog("", text, "", btnInfoDic, isCancel);
    }
    public static GameObject OpenSelectDialog(string text, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<Object> btnObjects)
    {
        return OpenSelectDialog("", text, "", btnInfoDic, isCancel, btnObjects);
    }
    public static GameObject OpenSelectDialog(string text, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<Color> btnColors)
    {
        return OpenSelectDialog("", text, "", btnInfoDic, isCancel, null, btnColors);
    }
    public static GameObject OpenSelectDialog(string title, string text, string imageName, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<Color> btnColors)
    {
        return OpenSelectDialog(title, text, imageName, btnInfoDic, isCancel, null, btnColors);
    }
    public static GameObject OpenSelectDialog(string title, string text, string imageName, Dictionary<string, UnityAction> btnInfoDic, bool isCancel, List<Object> btnObjects = null, List<Color> btnColors = null)
    {
        CloseMessage();
        if (dialog != null) CloseDialog();
        if (btnInfoDic.Count <= 0) return null;

        //ダイアログ生成
        dialog = Instantiate((GameObject)Resources.Load(RESOURCE_SELECT_DIALOG));
        Transform dialogTran = dialog.transform;

        //タイトル
        if (!string.IsNullOrEmpty(title))
        {
            Transform titleTran = dialogTran.FindChild("DialogArea/Title");
            titleTran.gameObject.SetActive(true);
            titleTran.GetComponent<Text>().text = title;
        }

        //画像
        if (!string.IsNullOrEmpty(imageName))
        {
            Sprite imgSprite = Resources.Load<Sprite>(RESOURCE_IMAGE_DIR + imageName);
            if (imgSprite != null)
            {
                Transform imageTran = dialogTran.FindChild("DialogArea/Image");
                imageTran.gameObject.SetActive(true);
                Image img = imageTran.GetComponent<Image>();
                img.sprite = imgSprite;
                img.preserveAspect = true;
            }
        }

        //メッセージ
        textMessage = dialogTran.FindChild("DialogArea/Message").GetComponent<Text>();
        textMessage.text = text;

        //セレクトボタン
        int index = 0;
        foreach (string btnText in btnInfoDic.Keys)
        {
            Object btnObj = null;
            Color btnColor = default(Color);
            if (btnObjects != null)
            {
                if (btnObjects.Count > 0 && btnObjects.Count > index) btnObj = btnObjects[index];
            }
            if (btnColors != null)
            {
                if (btnColors.Count > 0 && btnColors.Count > index) btnColor = btnColors[index];
            }
            SetSelectBtn(btnText, btnInfoDic[btnText], btnObj, btnColor);
            index++;
        }

        //Cancelボタン
        if (isCancel)
        {
            SetSelectBtn(BUTTON_CANCEL_TEXT, null, null, cancelColor);
        }
        return dialog;
    }

    private static void SetSelectBtn(string btnText, UnityAction action = null, Object btnObj = null, Color btnColor = default(Color))
    {
        GameObject btn = null;
        if (btnObj != null)
        {
            btn = (GameObject)Instantiate(btnObj);
        }
        else
        {
            btn = (GameObject)Instantiate(Resources.Load(RESOURCE_SELECT_BUTTON));
        }
        btn.GetComponent<Button>().onClick.AddListener(() => OnClickButton(action));
        Text buttonText = btn.transform.GetComponentInChildren<Text>();
        if (buttonText != null) buttonText.text = btnText;
        if (btnColor != default(Color)) buttonText.color = btnColor;
        btn.transform.SetParent(dialog.transform.FindChild("DialogArea"), false);
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
        CloseMessage();
        if (dialog != null) CloseDialog();
        if (buttons.Count <= 0 || actions.Count <= 0) return null;

        dialog = Instantiate((GameObject)Resources.Load(RESOURCE_DIALOG));
        textMessage = dialog.transform.FindChild("DialogArea/Message").GetComponent<Text>();
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

    public static void CloseDialog(bool isFadeOut = true)
    {
        if (dialog == null) return;
        Destroy(dialog);
    }

    private static void SetBtn(GameObject btnObj, string btnText, UnityAction btnAction)
    {
        btnObj.GetComponent<Button>().onClick.AddListener(() => OnClickButton(btnAction));
        Text buttonOkText = btnObj.transform.GetComponentInChildren<Text>();
        if (buttonOkText != null) buttonOkText.text = btnText;
    }

    private static void OnClickButton(UnityAction unityAction = null)
    {
        dialog.transform.FindChild("Filter").gameObject.SetActive(true);
        CloseDialog();
        if (unityAction != null)
        {
            unityAction.Invoke();
        }
    }

    public static Text GetDialogText()
    {
        return textMessage;
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
        }
        RectTransform messageImageTran = messageObj.transform.FindChild("Image").GetComponent<RectTransform>();
        messageImageTran.pivot = messageImagePositionPivots[msgPos];
        messageImage = messageImageTran.GetComponent<Image>();
        RectTransform messageTextTran = messageObj.transform.FindChild("Text").GetComponent<RectTransform>();
        messageText = messageTextTran.GetComponent<Text>();
        messageText.alignment = messageTextPositionPivots[msgPos]; ;

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
        if (messageImgDic == null)
        {
            Sprite[] messageImgs = Resources.LoadAll<Sprite>(MESSAGE_IMAGE_PARENT);
            messageImgDic = new Dictionary<string, Sprite>();
            foreach (Sprite img in messageImgs)
            {
                messageImgDic.Add(img.name, img);
            }
        }

        Sprite image = null;
        if (messageImgNameDic.ContainsKey(text))
        { 
            string imageName = messageImgNameDic[text];
            if (messageImgDic.ContainsKey(imageName))
            {
                image = messageImgDic[imageName];
            }
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

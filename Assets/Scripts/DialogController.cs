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
    private static GameObject message;

    const string RESOURCE_DIALOG = "UI/Dialog";
    const string RESOURCE_MESSAGE = "UI/Message";

    //メッセージ
    public const string MESSAGE_TOP = "Tap to Start";
    public const string MESSAGE_CONNECT = "Connecting...";
    public const string MESSAGE_LOADING = "Now Loading...";
    public const string MESSAGE_CREATE_ROOM = "Create Room";
    public const string MESSAGE_JOIN_ROOM = "Join Room";
    public const string MESSAGE_SEARCH_ROOM = "Search Room";

    //ボタン
    const string BUTTON_OK_TEXT = "OK";
    const string BUTTON_CANCEL_TEXT = "Cancel";

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
        Destroy(dialog);
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

    public static GameObject OpenMessage(string text)
    {
        if (text == "")
        {
            CloseMessage();
            return null;
        }

        if (message == null)
        {
            message = Instantiate((GameObject)Resources.Load(RESOURCE_MESSAGE));
            textMessage = message.transform.FindChild("Label/Text").GetComponent<Text>();
        }
        textMessage.text = text;

        return message;
    }

    public static void CloseMessage()
    {
        if (message == null) return;
        Destroy(message);
    }
    
}

using UnityEngine;
using System.Collections;
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

    public static void OpenDialog(string text, bool isCancel = false)
    {
        OpenDialog(text, null, null, isCancel);
    }

    public static void OpenDialog(string text, UnityAction okAction, bool isCancel = false)
    {
        OpenDialog(text, okAction, null, isCancel);
    }

    public static GameObject OpenDialog(string text, UnityAction okAction, UnityAction cancelAction, bool isCancel = false)
    {
        if (dialog != null) CloseDialog();

        dialog = Instantiate((GameObject)Resources.Load(RESOURCE_DIALOG));
        Text textMessage = dialog.transform.FindChild("DialogArea/Message").GetComponent<Text>();
        Button buttonOk = dialog.transform.FindChild("DialogArea/ButtonArea/OK").GetComponent<Button>();
        GameObject buttonCancelObj = dialog.transform.FindChild("DialogArea/ButtonArea/Cancel").gameObject;

        textMessage.text = text;
        buttonOk.onClick.AddListener(() => OnOk(okAction));
        if (cancelAction != null || isCancel)
        {
            buttonCancelObj.GetComponent<Button>().onClick.AddListener(() => OnCancel(cancelAction));
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

    public static void OnOk(UnityAction unityAction = null)
    {
        if (unityAction != null)
        {
            unityAction.Invoke();
        }
        CloseDialog();
    }

    public static void OnCancel(UnityAction unityAction = null)
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

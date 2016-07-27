using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogController : MonoBehaviour
{
    //ダイアログ
    private static GameObject dialog;
    //private static Text textMessage;
    //private static Button buttonOk;
    //private static GameObject buttonCancelObj;

    //メッセージ
    private static GameObject message;

    const string RESOURCE_DIALOG = "UI/Dialog";
    const string RESOURCE_MESSAGE = "UI/Message";

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

    public static GameObject OpenMessage(string text, int kind = 0)
    {
        if (message != null) CloseMessage();
        message = Instantiate((GameObject)Resources.Load(RESOURCE_MESSAGE));
        Image label = message.transform.FindChild("Label").GetComponent<Image>();
        Text textMessage = message.transform.FindChild("Label/Text").GetComponent<Text>();
        textMessage.text = text;

        //kindで色変える？

        return message;
    }
    public static void CloseMessage()
    {
        Destroy(message);
    }
}

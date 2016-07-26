using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogController : MonoBehaviour
{
    private static Text textMessage;
    private static Button buttonOk;
    private static GameObject buttonCancelObj;

    private static GameObject dialog;

    public static void OpenDialog(string text, bool isCancel = false)
    {
        OpenDialog(text, null, null, isCancel);
    }

    public static void OpenDialog(string text, UnityAction okAction, bool isCancel = false)
    {
        OpenDialog(text, okAction, null, isCancel);
    }

    public static void OpenDialog(string text, UnityAction okAction, UnityAction cancelAction, bool isCancel = false)
    {
        dialog = (GameObject)Instantiate((GameObject)Resources.Load("Dialog"));
        textMessage = dialog.transform.FindChild("DialogArea/Message").GetComponent<Text>();
        buttonOk = dialog.transform.FindChild("DialogArea/ButtonArea/OK").GetComponent<Button>();
        buttonCancelObj = dialog.transform.FindChild("DialogArea/ButtonArea/Cancel").gameObject;

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
}

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DataReset : MonoBehaviour
{
    void Start()
    {
        string message = "Your data format?";
        UnityAction action = Reset;
        DialogController.OpenDialog(message, action, true);
    }

    void Reset()
    {
        UserManager.DeleteUser();
        string message = "complete!!";
        DialogController.OpenDialog(message);
    }
}

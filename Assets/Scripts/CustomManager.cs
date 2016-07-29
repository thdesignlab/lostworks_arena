using UnityEngine;
using System.Collections;

public class CustomManager : MonoBehaviour
{
    public void GoToTitle()
    {
        PhotonNetwork.LoadLevel(Common.CO.SCENE_TITLE);
    }
}

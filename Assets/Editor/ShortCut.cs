using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ShortCut : MonoBehaviour
{
    private static string[] sceneList = new string[]
    {
        "Title",    //0
        "Battle",   //1
        "Custom",   //2
    };


    //&:Alt
    //#:Shift
    //%:Ctrl
    [MenuItem("Tools/PlayGame &q")]
    public static void PlayFromPrelaunchScene()
    {
        try {
            // ÉvÉåÉCíÜÇ»ÇÁÇŒí‚é~Ç∑ÇÈ
            if (EditorApplication.isPlaying == true)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // çƒê∂ÇµÇΩÇ¢ÉVÅ[ÉìÇÃì«Ç›çûÇ›->çƒê∂
            OpenScene(0);
            EditorApplication.isPlaying = true;
        }
        catch
        {
            Debug.Log("Error");
        }
    }

    private static void OpenScene(int sceneNo)
    {
        try {
            //EditorApplication.SaveCurrentSceneIfUserWantsTo();    Å`5.2
            //EditorApplication.OpenScene("Assets/Scenes/" + sceneList[sceneNo] + ".Unity");
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Scenes/" + sceneList[sceneNo] + ".Unity");
        }
        catch
        {

        }
    }

    [MenuItem("Tools/OpenScene(0) &z")]
    public static void OpenScene_0()
    {
        OpenScene(0);
    }

    [MenuItem("Tools/OpenScene(1) &x")]
    public static void OpenScene_1()
    {
        OpenScene(1);
    }

    [MenuItem("Tools/OpenScene(2) &c")]
    public static void OpenScene_2()
    {
        OpenScene(2);
    }
}

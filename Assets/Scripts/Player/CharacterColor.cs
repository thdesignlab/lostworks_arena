using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterColor : MonoBehaviour
{
    [SerializeField]
    private List<Texture> hairTextureList;
    [SerializeField]
    private List<Texture> faceTextureList;
    [SerializeField]
    private List<Texture> bodyTextureList;

    const string TEXTURE_HAIR = "Hair";
    const string TEXTURE_FACE = "Face";
    const string TEXTURE_BODY = "Body";

    private Transform boxTran;

    void Awake()
    {
        boxTran = transform.FindChild(Common.CO.PARTS_BOX);
    }

    public void SetColor(string colorNoStr)
    {
        if (boxTran == null) return;

        int colorNo = int.Parse(colorNoStr);
        Renderer renderer = boxTran.GetComponent<Renderer>();
        Material[] mats = renderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            Texture replaceTexture = null;
            switch (mats[i].name.Replace(" (Instance)", ""))
            {
                case TEXTURE_HAIR:
                    if (0 <= colorNo && colorNo < hairTextureList.Count) replaceTexture = hairTextureList[colorNo];
                    break;

                case TEXTURE_FACE:
                    if (0 <= colorNo && colorNo < faceTextureList.Count) replaceTexture = faceTextureList[colorNo];
                    break;

                case TEXTURE_BODY:
                    if (0 <= colorNo && colorNo < bodyTextureList.Count) replaceTexture = bodyTextureList[colorNo];
                    break;
            }
            if (replaceTexture != null) mats[i].mainTexture = replaceTexture;
        }
        renderer.materials = mats;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissionManager : Photon.MonoBehaviour
{
    //ステージ：NPC
    public static Dictionary<int, int> stageNpcDic = new Dictionary<int, int>()
    {
        { 1, 1000 },
        { 2, 1001 },
        { 3, 1002 },
        { 4, 1003 },
        { 5, 1004 },
        { 6, 1 },
        { 7, 0 },
    };

    void Awake()
    {
    }

    void Start()
    {
    }
}

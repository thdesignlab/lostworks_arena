using UnityEditor;using UnityEngine;using System.Collections.Generic;
/// <summary>/// 現在のターゲットに合わせてBundleIdentifierを更新するクラス/// </summary>
[InitializeOnLoad]
public class BundleIdentifierUpdater
{
    //各ターゲットごとにBundleIdentifierをまとめたディクショナリー
    private static Dictionary<BuildTarget, string> BUNDLE_IDENTIFIER_DICT = new Dictionary<BuildTarget, string>(){        {BuildTarget.iOS,     "com.ThDesignLab.LostworksArena"},        {BuildTarget.Android, "com.ThDesignLab"},    };

    //=================================================================================
    //初期化
    //=================================================================================

    //InitializeOnLoadを付けた事によってUnityエディター起動時に実行される
    static BundleIdentifierUpdater()
    {
        //ビルドターゲットが変わった時のアクションにメソッドを登録
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangedBuildTarget;
    }

    //=================================================================================
    //内部
    //=================================================================================

    //ビルドターゲットが変わった
    private static void OnChangedBuildTarget()
    {
        //現在のターゲットのBundleIdentifierがディクショナリーにない場合は更新しない
        BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
        if (!BUNDLE_IDENTIFIER_DICT.ContainsKey(currentTarget))
        {
            return;
        }

        //BundleIdentifier更新
        PlayerSettings.bundleIdentifier = BUNDLE_IDENTIFIER_DICT[currentTarget];
        Debug.Log("current bundleIdentifier : " + PlayerSettings.bundleIdentifier);
    }}
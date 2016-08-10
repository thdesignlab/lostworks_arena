using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterManager
{
    //キャラ情報取得
    public static string[] GetCharacterInfo(int charaNo)
    {
        string[] charaInfo = null;
        if (Common.Character.characterLineUp.ContainsKey(charaNo))
        {
            charaInfo = Common.Character.characterLineUp[charaNo];
        }
        return charaInfo;
    }

    //選択可能キャラ取得
    public static List<int> GetSelectableCharacter(bool isSelected = true)
    {
        List<int> characters = new List<int>();
        foreach (int charaNo in Common.Character.characterLineUp.Keys)
        {
            //現在設定中キャラチェック
            if (!isSelected && UserManager.userSetCharacter == charaNo) continue;

            //取得条件チェック
            if (!IsSelectableCharacter(charaNo)) continue;

            characters.Add(charaNo);
        }
        return characters;
    }

    //所持キャラチェック
    public static bool IsSelectableCharacter(int charaNo)
    {
        //取得条件チェック
        switch (Common.Character.characterLineUp[charaNo][Common.Character.DETAIL_OBTAIN_TYPE_NO])
        {
            case Common.Character.OBTAIN_TYPE_INIT:
                //初期から所持
                break;

            case Common.Character.OBTAIN_TYPE_NONE:
                //使用不可
                return false;

            default:
                //獲得チェック
                if (!UserManager.userOpenCharacters.Contains(charaNo)) return false;
                break;
        }
        return true;
    }
}

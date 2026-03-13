using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene identifiers stored here.
/// <summary>
public enum SceneID
{
    MainMenu,
    Training,
    InputSelect,
    CharSelect,
    Battle
}

//Hard-typed bleh
public static class SceneDataBank {
    public const string MENU = "CL_MainMenu";
    public const string INPUT_SELECT = "InputSelect";
    public const string TRAINING = "Training";
    public const string CHAR_SELECT = "CharacterSelect";
    public const string BATTLE = "Hypermania";

    public static Dictionary<SceneID, string> SceneMap { get; private set; } = new();

    static SceneDataBank() {
        SceneMap.Add(SceneID.MainMenu, MENU);
        SceneMap.Add(SceneID.InputSelect, INPUT_SELECT);
        SceneMap.Add(SceneID.Training, TRAINING);
        SceneMap.Add(SceneID.CharSelect, CHAR_SELECT);
        SceneMap.Add(SceneID.Battle, BATTLE);
    }
}

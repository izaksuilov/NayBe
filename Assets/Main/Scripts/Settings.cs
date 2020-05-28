using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public enum Events : byte
{
    StartFirstPhase = 1,
    TryStartGame,
    StartSecondPhase,
    ArrangePlayers,
    SwitchPlayerTurn,
    MoveCard,
    PlayerLeftRoom,
    ClearField,
    PlayerWin
}
public static class Settings
{
    static string path = Application.persistentDataPath + "/settings";
    
    #region Color
    public static int colorScheme { get; private set; } = 0;
    public async static void SaveColor(int x)
    {
        if (x < 0 || x > 3) return;
        colorScheme = x;
        using (StreamWriter file = new StreamWriter(path, false))
            await file.WriteLineAsync(colorScheme.ToString());
    }
    #endregion
    #region NickName
    public static string nickName { get; private set; } = "";
    public static void SaveNickName(string newName)
    {
        nickName = newName;
        string[] file = File.ReadAllLines(path);
        file[1] = nickName;
        File.WriteAllLines(path, file);
    }
    #endregion
    public async static void Load()
    {
        if (File.Exists(path))
        {
            string[] file = File.ReadAllLines(path);
            colorScheme = int.Parse(file[0]);
            nickName = file[1];
        }
        else 
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                await file.WriteLineAsync(colorScheme.ToString());
                await file.WriteLineAsync(nickName);
            }
        } 
    }
}
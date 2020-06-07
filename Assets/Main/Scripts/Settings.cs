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
    PlayerWin,
    EnableField
}
public static class Settings
{
    static string path = Application.persistentDataPath + "/settings";
    
    #region Color
    public static int colorScheme { get; private set; } = 0;
    public static void SaveColor(int x)
    {
        if (x < 0 || x > 3) return;
        colorScheme = x;
        string[] file = File.ReadAllLines(path);
        file[0] = colorScheme.ToString();
        File.WriteAllLines(path, file);
    }
    #endregion
    #region NickName
    public static string nickName { get; private set; } = "";
    public static void SaveNickName(string newName)
    {
        nickName = newName;
        string[] file = File.ReadAllLines(path);
        if (file.Length > 1)
        {
            file[1] = nickName;
            File.WriteAllLines(path, file);
        }
        else File.AppendAllText(path, newName);

    }
    #endregion
    public async static void Load()
    {
        if (File.Exists(path))
        {
            string[] file = File.ReadAllLines(path);
            colorScheme = int.Parse(file[0]);
            if (file.Length > 1)
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
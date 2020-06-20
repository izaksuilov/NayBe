using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
    static string path = Application.persistentDataPath + "/settings.dat";

    #region Color
    public static int colorScheme { get; private set; } = 0;
    public static void SaveColor(int x)
    {
        if (x < 0 || x > 3) return;
        colorScheme = x;
        SaveInFile(0, colorScheme.ToString());
    }
    #endregion
    #region NickName
    public static string nickName { get; private set; } = "Игрок";
    public static void SaveNickName(string newName)
    {
        if (newName.Length < 1) return;
        nickName = newName;
        SaveInFile(1, newName);
    }
    #endregion
    #region Money
    public static int money { get; private set; } = 1000;
    public static void AddMoney(int m)
    {
        money += m;
        SaveInFile(2, money.ToString());
    }
    #endregion
    public async static void Load()
    {
        if (File.Exists(path))
        {
            string[] file = File.ReadAllLines(path);
            colorScheme = int.Parse(file[0]);
            nickName = file[1];
            money = int.Parse(file[2]);
        }
        else
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                await file.WriteLineAsync(colorScheme.ToString());
                await file.WriteLineAsync(nickName);
                await file.WriteLineAsync(money.ToString());
            }
        }
    }
    private static void SaveInFile(int index, string obj)
    {
        string[] file = File.ReadAllLines(path);
        file[index] = obj;
        File.WriteAllLines(path, file);
    }
}
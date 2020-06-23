using System;
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
    static string path = Application.persistentDataPath + "/settings.dat";
    static int key = 228069;

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
    public static int money { get; private set; } = 10000;
    public static int lvl { get; private set; } = 1;
    public static int nextLvlExp { get; private set; } = 100;
    public static int progress { get; private set; } = 0;
    public static void AddMoney(int m)
    {
        int prevMoney = money;
        try
        { money = checked(money + m); }
        catch (System.OverflowException)
        { money = int.MaxValue;  }
        money -= money % 10;
        if (money < 0)
            money = 0;
        SaveInFile(2, Encrypt(money));

        if (m > 0)
        {
            try
            { 
                nextLvlExp = checked((int)Math.Pow(lvl, 1.75) * 100);
                progress = checked(progress + 
                    (int)((m > 1000 ? m / 10 : m) * (lvl / 10 + 1.25)));
            }
            catch (System.OverflowException)
            {
                nextLvlExp = int.MaxValue;
                progress = 0;
            }
            if (progress >= nextLvlExp)
            {
                lvl++;
                progress %= nextLvlExp;
                SaveInFile(3, Encrypt(lvl));
                nextLvlExp = (int)Math.Pow(lvl, 1.75) * 100;
                SaveInFile(4, Encrypt(nextLvlExp));
                try
                { money = checked(money + lvl * 1000); }
                catch (System.OverflowException)
                { money = int.MaxValue; }
                SaveInFile(2, Encrypt(money));
            }
            SaveInFile(5, Encrypt(progress));
        }
    }
    #endregion
    public static void Load()
    {
        if (File.Exists(path))
        {
            if (File.ReadAllLines(path).Length != 6)
            {
                File.Delete(path);
                CreateFile();
                return;
            }
            string[] file = File.ReadAllLines(path);
            colorScheme = int.Parse(file[0]);
            nickName = file[1];
            money = Decrypt(file[2]);
            if (money < 100)
            {
                money = 100;
                SaveInFile(2, Encrypt(money));
            }
            if (money % 10 != 0)
            {
                File.Delete(path);
                Load();
            }
            lvl = Decrypt(file[3]);
            nextLvlExp = Decrypt(file[4]);
            progress = Decrypt(file[5]);
        }
        else
            CreateFile();

    }
    private static void SaveInFile(int index, string obj)
    {
        string[] file = File.ReadAllLines(path);
        file[index] = obj;
        File.WriteAllLines(path, file);
    }
    private async static void CreateFile()
    {
        using (StreamWriter stream = new StreamWriter(path, true))
        {
            await stream.WriteLineAsync(colorScheme.ToString());
            await stream.WriteLineAsync(nickName);
            await stream.WriteLineAsync(Encrypt(money));
            await stream.WriteLineAsync(Encrypt(lvl));
            await stream.WriteLineAsync(Encrypt(nextLvlExp));
            await stream.WriteLineAsync(Encrypt(progress));
        }
    }
    private static string Encrypt(int num)
    {
        string s = (key ^ num).ToString(), ch = "", chars = "";
        for (int i = 0; i < s.Length; i += 1)
        {
            ch += s[i];
            if ((i + 1) % 2 == 0)
            {
                chars += (char)int.Parse(ch);
                ch = "";
            }
            while ((i + 1 < s.Length) && int.Parse(s[i + 1].ToString()) == 0)
            {
                i++;
                if (ch.Length > 0)
                {
                    chars += (char)int.Parse(ch);
                    ch = "";
                }
                chars += (char)int.Parse(s[i].ToString());
            }            

        }
        if (!ch.Equals(""))
            chars += (char)int.Parse(ch);
        return chars;
    }
    private static int Decrypt(string encryptedObj)
    {
        string s = "";
        for (int i = 0; i < encryptedObj.Length; i += 1)
            s += (int)encryptedObj[i];
        return key ^ int.Parse(s);
    }
}
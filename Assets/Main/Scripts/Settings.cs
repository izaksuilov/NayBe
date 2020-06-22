using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
    static string key = "",
        path = Application.persistentDataPath + "/settings.dat",
        macAddress = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                   nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault();

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
        money -= money % 10;
        SaveInFile(2, EncryptMoney());
    }
    #endregion
    public async static void Load()
    {
        key = "";
        for (int i = 0; i < macAddress.Length; i++)
            if (int.TryParse(macAddress[i].ToString(), out int result))
                key += result;

        if (File.Exists(path))
        {

            string[] file = File.ReadAllLines(path);
            colorScheme = int.Parse(file[0]);
            nickName = file[1];
            money = DecryptMoney(file[2]);
            if (money % 10 != 0)
            {
                money = -100000;
                SaveInFile(2, EncryptMoney());
                return;
            }
            if (money < 100)
            {
                money = 100;
                SaveInFile(2, EncryptMoney());
            }
        }
        else
        {
            using (StreamWriter file = new StreamWriter(path, true))
            {
                await file.WriteLineAsync(colorScheme.ToString());
                await file.WriteLineAsync(nickName);
                await file.WriteLineAsync(EncryptMoney());
            }
        }
    }
    private static void SaveInFile(int index, string obj)
    {
        string[] file = File.ReadAllLines(path);
        file[index] = obj;
        File.WriteAllLines(path, file);
    }
    private static string EncryptMoney()
    {
        string s = (int.Parse(key) ^ money).ToString(), ch = "", chars = "";
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
    private static int DecryptMoney(string encryptedMoney)
    {
        string s = "";
        for (int i = 0; i < encryptedMoney.Length; i += 1)
            s += (int)encryptedMoney[i];
        return int.Parse(key) ^ int.Parse(s);
    }
}
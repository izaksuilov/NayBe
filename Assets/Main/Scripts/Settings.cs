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
    #region Переменные
    static string path = Application.persistentDataPath + "/settings.dat",
                  _nickName = "Игрок";
    static int key = 228069, _colorScheme = 0, _money = 1000, _progress = 0, _lvl = 1;
    public static int ColorScheme 
    {
        get { return _colorScheme; }
        set
        {
            if (value < 0 || value > 3) throw new ArgumentOutOfRangeException("Color scheme should be >= 0 and <= 3");
            _colorScheme = value;
            SaveFile();
        }
    }
    public static string NickName
    {
        get { return _nickName; }
        set
        {
            if (value.Length < 1) throw new ArgumentException("Nickname can't be empty");
            _nickName = value;
            SaveFile();
        }
    }
    public static int Money
    {
        get { return _money; }
        set 
        {
            try
            { _money = checked(value); }
            catch (System.OverflowException)
            { _money = int.MaxValue; }
            _money -= _money % 10;
            if (_money < 0)
                _money = 0;
            SaveFile();
        }
    }
    public static int Lvl 
    {
        get { return _lvl; } 
        private set
        {
            _lvl = value;
            Step = NextLvlExp / _lvl;
        }
    }
    public static int NextLvlExp { get; private set; } = 100;
    public static int Step { get; private set; } = 50;
    public static int Progress 
    {
        get { return _progress; } 
        set
        {
            try
            {
                NextLvlExp = checked((int)Math.Pow(Lvl, 1.75) * 100);
                _progress = checked(value);
            }
            catch (System.OverflowException)
            {
                NextLvlExp = int.MaxValue;
                _progress = 0;
            }
            if (_progress >= NextLvlExp)
            {
                Lvl++;
                _progress %= NextLvlExp;
                NextLvlExp = (int)Math.Pow(Lvl, 1.75) * 100;

                try
                { _money = checked(_money + _lvl * 1000); }
                catch (System.OverflowException)
                { _money = int.MaxValue; }
                Notification.Show($"Новый уровень!\nВы получаете {_lvl * 1000} рублей!", 2, Notification.Position.bottom, Notification.Color.good);
            }
            SaveFile();
        }
    }
    static string[] objToSave = { ColorScheme.ToString(),
                                  NickName,
                                  Encrypt(Money),
                                  Encrypt(Lvl),
                                  Encrypt(NextLvlExp),
                                  Encrypt(Progress) };
    #endregion
    public static void Load()
    {
        if (File.Exists(path))
        {
            if (File.ReadAllLines(path).Length != objToSave.Length)
            {
                File.Delete(path);
                CreateFile();
                return;
            }
            string[] file = File.ReadAllLines(path);
            ColorScheme = int.Parse(file[0]);
            NickName = file[1];
            Money = Decrypt(file[2]);
            Lvl = Decrypt(file[3]);
            NextLvlExp = Decrypt(file[4]);
            Progress = Decrypt(file[5]);
        }
        else
            CreateFile();
    }
    private static void SaveFile()
    {
        objToSave = new string[] { ColorScheme.ToString(),
                                   NickName,
                                   Encrypt(Money),
                                   Encrypt(Lvl),
                                   Encrypt(NextLvlExp),
                                   Encrypt(Progress) };

        string[] newFile = new string[objToSave.Length];
        for (int i = 0; i < objToSave.Length; i++)
            newFile[i] = objToSave[i];
        File.WriteAllLines(path, newFile);
    }
    private async static void CreateFile()
    {
        using (StreamWriter stream = new StreamWriter(path, true))
            foreach (string obj in objToSave)
                await stream.WriteLineAsync(obj);
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
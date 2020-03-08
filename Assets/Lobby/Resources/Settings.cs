using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public enum Events : byte
{
    CreateCards = 1,
    RemoveCards,
    RemovePlayer, 
    AddPlayer
}
public static class Settings
{
    public static int colorScheme = 0;
    static string path = Application.persistentDataPath + "/settings.gd";
    public static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(path))
            bf.Serialize(file, colorScheme);
    }
    public static void Load()
    {
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(path, FileMode.Open))
                colorScheme = (int)bf.Deserialize(file);
        }
        else colorScheme = 0;
    }
    public static void ChangeColorScheme(int x)
    {
        if (x < 0 || x > 3) return;
        colorScheme = x;
        Save();
    }
}
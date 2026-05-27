using System;
using System.IO;
using UnityEngine;

namespace WPG.Core
{
    public static class SaveSystem
    {
        private const string FileName = "wpg_save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public static void Save(SaveData data)
        {
            try
            {
                data.saveTimestamp = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] Zapisano grę: {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Błąd zapisu: {e.Message}");
            }
        }

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    Debug.Log("[SaveSystem] Brak pliku zapisu.");
                    return null;
                }

                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveSystem] Wczytano save: {data?.saveTimestamp}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Błąd odczytu: {e.Message}");
                return null;
            }
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                    Debug.Log("[SaveSystem] Usunięto zapis.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Błąd usuwania zapisu: {e.Message}");
            }
        }
    }
}

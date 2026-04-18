using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class LeaderboardSystem
{
    private static string Path =>
        Application.persistentDataPath + "/leaderboard.json";

    public static List<RunData> Load()
    {
        if (!File.Exists(Path))
            return new List<RunData>();

        string json = File.ReadAllText(Path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        return data?.leaderboard ?? new List<RunData>();
    }

    public static void SaveRun(int score, float time)
    {
        List<RunData> leaderboard = Load();

        leaderboard.Add(new RunData(score, time));

        leaderboard = leaderboard
            .OrderByDescending(r => r.score)
            .ThenByDescending(r => r.time)
            .Take(5)
            .ToList();

        SaveData wrapper = new SaveData();
        wrapper.leaderboard = leaderboard;

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(Path, json);

        Debug.Log("Leaderboard Saved: " + leaderboard.Count);
    }
}
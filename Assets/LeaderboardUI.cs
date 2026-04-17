using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MainMenuLeaderboardUI : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;

    void Start()
    {
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        SaveData data = SaveSystem.LoadGame();

        if (data == null || data.leaderboard == null || data.leaderboard.Count == 0)
        {
            if (leaderboardText != null)
                leaderboardText.text = "No runs yet";

            return;
        }

        // Sort by highest score first
        List<RunData> sorted = data.leaderboard
            .OrderByDescending(r => r.score)
            .ThenByDescending(r => r.time)
            .ToList();

        // Take top 5 runs
        int count = Mathf.Min(5, sorted.Count);

        string output = "Last Runs (Top 5)\n\n";

        for (int i = 0; i < count; i++)
        {
            RunData run = sorted[i];

            output +=
                $"{i + 1}. Score: {run.score} | Time: {FormatTime(run.time)}\n";
        }

        if (leaderboardText != null)
            leaderboardText.text = output;
    }

    string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
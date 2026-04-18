using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LeaderBoardUI : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;

    void Start()
    {
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        List<RunData> leaderboard = LeaderboardSystem.Load();

        if (leaderboard == null || leaderboard.Count == 0)
        {
            leaderboardText.text = "No runs yet";
            return;
        }

        List<RunData> sorted = leaderboard
            .OrderByDescending(r => r.score)
            .ThenByDescending(r => r.time)
            .Take(5)
            .ToList();

        string output = "TOP 5 RUNS\n\n";

        for (int i = 0; i < sorted.Count; i++)
        {
            RunData run = sorted[i];
            output += $"{i + 1}. Score: {run.score} | Time: {FormatTime(run.time)}\n";
        }

        leaderboardText.text = output;
    }

    string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
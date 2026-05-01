using UnityEngine;
using TMPro;

public class DifficultyUI : MonoBehaviour
{
    public TextMeshProUGUI difficultyText;

    private EnemySpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<EnemySpawner>();
    }

    void Update()
    {
        if (spawner == null || difficultyText == null) return;

        float hp = spawner.GetHPModifier();
        float speed = spawner.GetSpeedModifier();
        float reward = spawner.GetRewardModifier();

        difficultyText.text =
            $"Enemy HP: x{hp:F2}\n" +
            $"Enemy Speed: x{speed:F2}\n" +
            $"Reward: x{reward:F2}";
    }
}
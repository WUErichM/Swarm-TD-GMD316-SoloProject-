using UnityEngine;
using TMPro;

public class TowerUI : MonoBehaviour
{
    public static TowerUI instance;

    public GameObject panel;

    public TextMeshProUGUI damageText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI upgradeCostText; // ✅ NEW

    public GameObject upgradeButton; // ✅ NEW

    private Tower currentTower;
    private GameObject currentRangeIndicator;

    public GameObject rangeIndicatorPrefab;

    void Awake()
    {
        instance = this;
        Hide();
    }

    public void Show(Tower tower)
    {
        currentTower = tower;

        panel.SetActive(true);

        damageText.text = "Damage: " + tower.damage;

        if (tower.towerType == Tower.TowerType.Sniper)
        {
            rangeText.text = "Range: Unlimited";
        }
        else
        {
            rangeText.text = "Range: " + tower.range.ToString("F1");
        }

        float attacksPerSecond = 1f / tower.fireRate;
        fireRateText.text = "Attack Speed: " + attacksPerSecond.ToString("F2") + "/sec";

        levelText.text = "Level: " + tower.level;

        UpdateUpgradeUI();

        ShowRange(tower);
    }

    void UpdateUpgradeUI()
    {
        int baseCost = BuildManager.instance.GetLastCost();
        int upgradeCost = Mathf.FloorToInt(baseCost * 0.6f);

        upgradeCostText.text = "Upgrade ($" + upgradeCost + ")";
    }

    public void OnUpgradeButton()
    {
        if (currentTower != null)
        {
            currentTower.UpgradeToMatchHighest();
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentTower = null;

        if (currentRangeIndicator != null)
        {
            Destroy(currentRangeIndicator);
        }
    }

    void ShowRange(Tower tower)
    {
        if (tower.towerType == Tower.TowerType.Sniper) return;

        if (currentRangeIndicator != null)
        {
            Destroy(currentRangeIndicator);
        }

        currentRangeIndicator = Instantiate(rangeIndicatorPrefab);
        currentRangeIndicator.transform.position = tower.transform.position;

        RangeIndicator ri = currentRangeIndicator.GetComponent<RangeIndicator>();
        if (ri != null)
        {
            ri.SetRange(tower.range);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Hide();
        }
    }
}
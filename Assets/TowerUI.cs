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
    public TextMeshProUGUI upgradeCostText;

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

        if (tower.towerType == Tower.TowerType.Slow)
        {
            damageText.text = "Slow: " + (tower.GetSlowPercent() * 100f).ToString("F0") + "%";
            fireRateText.text = "AoE Effect";
        }
        else if (tower.towerType == Tower.TowerType.Empower)
        {
            damageText.text = "DMG Buff: +" + (tower.GetDamageBuff() * 100f).ToString("F0") + "%";
            fireRateText.text = "ATK SPD Buff: +" + (tower.GetSpeedBuff() * 100f).ToString("F0") + "%";
        }
        else
        {
            damageText.text = "Damage: " + tower.damage;
            fireRateText.text = "Attack Speed: " + (1f / tower.fireRate).ToString("F2") + "/sec";
        }

        rangeText.text = tower.towerType == Tower.TowerType.Sniper
            ? "Range: Unlimited"
            : "Range: " + tower.range.ToString("F1");

        levelText.text = "Level: " + tower.level;

        UpdateUpgradeUI();

        ShowRange(tower);
    }

    void UpdateUpgradeUI()
    {
        int baseCost = BuildManager.instance.GetLastCost();

        int upgradeCost = (currentTower.towerType == Tower.TowerType.Slow || currentTower.towerType == Tower.TowerType.Empower)
            ? Mathf.FloorToInt(baseCost * 0.5f)
            : Mathf.FloorToInt(baseCost * 0.6f);

        upgradeCostText.text = "Upgrade ($" + upgradeCost + ")";
    }

    public void OnUpgradeButton()
    {
        if (currentTower != null)
            currentTower.UpgradeToMatchHighest();
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentTower = null;

        if (currentRangeIndicator != null)
            Destroy(currentRangeIndicator);
    }

    void ShowRange(Tower tower)
    {
        if (tower.towerType == Tower.TowerType.Sniper) return;

        if (currentRangeIndicator != null)
            Destroy(currentRangeIndicator);

        currentRangeIndicator = Instantiate(rangeIndicatorPrefab);
        currentRangeIndicator.transform.position = tower.transform.position;

        RangeIndicator ri = currentRangeIndicator.GetComponent<RangeIndicator>();
        if (ri != null)
            ri.SetRange(tower.range);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            Hide();
    }
}
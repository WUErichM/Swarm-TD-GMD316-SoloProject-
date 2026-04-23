using UnityEngine;
using TMPro;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;

    public GameObject standardTowerPrefab;
    public GameObject meleeTowerPrefab;
    public GameObject sniperTowerPrefab;

    public GameObject slowTowerPrefab;
    public GameObject empowerTowerPrefab;

    public GameObject rangeIndicatorPrefab;

    public LayerMask groundMask;
    public LayerMask pathMask;

    // ✅ UI TEXTS
    public TextMeshProUGUI standardCostText;
    public TextMeshProUGUI meleeCostText;
    public TextMeshProUGUI sniperCostText;
    public TextMeshProUGUI slowCostText;
    public TextMeshProUGUI empowerCostText;

    private bool isPlacing = false;
    private GameObject selectedTowerPrefab;

    private GameObject previewTower;
    private GameObject rangeIndicator;

    // ✅ SEPARATE COUNTS
    private int offensiveTowersBuilt = 0;
    private int supportTowersBuilt = 0;

    private int highestTowerLevelEver = 1;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        UpdateCostUI();

        if (!isPlacing) return;

        HandlePreview();

        if (Input.GetMouseButtonDown(0))
            TryPlace();

        if (Input.GetMouseButtonDown(1))
            CancelPreview();
    }

    // ---------- PREVIEW ----------
    void HandlePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundMask))
        {
            Vector3 pos = hit.point;
            pos.y = 0.5f;

            if (previewTower == null)
            {
                previewTower = Instantiate(selectedTowerPrefab);
                DisableScripts(previewTower);
                SetLayer(previewTower, LayerMask.NameToLayer("Ignore Raycast"));
                SetTransparent(previewTower);
            }

            previewTower.transform.position = pos;

            Tower prefabTower = selectedTowerPrefab.GetComponent<Tower>();

            if (prefabTower != null)
            {
                if (prefabTower.towerType == Tower.TowerType.Sniper)
                {
                    if (rangeIndicator != null)
                        Destroy(rangeIndicator);
                }
                else
                {
                    if (rangeIndicator == null)
                        rangeIndicator = Instantiate(rangeIndicatorPrefab);

                    rangeIndicator.transform.position = pos;

                    RangeIndicator ri = rangeIndicator.GetComponent<RangeIndicator>();
                    if (ri != null)
                        ri.SetRange(prefabTower.range);
                }
            }
        }
    }

    void TryPlace()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, pathMask)) return;

        if (Physics.Raycast(ray, out hit, 100f, groundMask))
            PlaceTower(hit.point);
    }

    void PlaceTower(Vector3 position)
    {
        int cost = GetCurrentCost();

        if (!GameManager.instance.SpendMoney(cost)) return;

        position.y = 0.5f;

        GameObject tower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);
        SetLayer(tower, LayerMask.NameToLayer("Default"));

        Tower t = tower.GetComponent<Tower>();

        if (t != null)
        {
            int level = GetTotalTowersBuilt() + 1;
            t.SetInitialLevel(level);
            RegisterTowerLevel(level);

            // ✅ TRACK TYPE
            if (IsSupportTower(selectedTowerPrefab))
                supportTowersBuilt++;
            else
                offensiveTowersBuilt++;
        }

        CancelPreview();
    }

    // ---------- COST LOGIC ----------
    int GetCurrentCost()
    {
        if (IsSupportTower(selectedTowerPrefab))
        {
            if (supportTowersBuilt == 0) return 1000;

            float cost = 1000 * Mathf.Pow(1.35f, supportTowersBuilt);
            return Mathf.FloorToInt(cost);
        }
        else
        {
            if (offensiveTowersBuilt == 0) return 30;
            if (offensiveTowersBuilt == 1) return 45;
            if (offensiveTowersBuilt == 2) return 60;
            if (offensiveTowersBuilt == 3) return 90;
            if (offensiveTowersBuilt == 4) return 150;

            float cost = 150 * Mathf.Pow(1.35f, offensiveTowersBuilt - 4);
            return Mathf.FloorToInt(cost);
        }
    }

    bool IsSupportTower(GameObject prefab)
    {
        return prefab == slowTowerPrefab || prefab == empowerTowerPrefab;
    }

    int GetTotalTowersBuilt()
    {
        return offensiveTowersBuilt + supportTowersBuilt;
    }

    // ---------- UI ----------
    void UpdateCostUI()
    {
        int offensiveCost = GetPreviewCost(false);
        int supportCost = GetPreviewCost(true);

        if (standardCostText != null)
            standardCostText.text = "Standard ($" + offensiveCost + ")";

        if (meleeCostText != null)
            meleeCostText.text = "Melee ($" + offensiveCost + ")";

        if (sniperCostText != null)
            sniperCostText.text = "Sniper ($" + offensiveCost + ")";

        if (slowCostText != null)
            slowCostText.text = "Slow ($" + supportCost + ")";

        if (empowerCostText != null)
            empowerCostText.text = "Empower ($" + supportCost + ")";
    }

    int GetPreviewCost(bool support)
    {
        if (support)
        {
            if (supportTowersBuilt == 0) return 1000;
            return Mathf.FloorToInt(1000 * Mathf.Pow(1.35f, supportTowersBuilt));
        }
        else
        {
            if (offensiveTowersBuilt == 0) return 30;
            if (offensiveTowersBuilt == 1) return 45;
            if (offensiveTowersBuilt == 2) return 60;
            if (offensiveTowersBuilt == 3) return 90;
            if (offensiveTowersBuilt == 4) return 150;

            return Mathf.FloorToInt(150 * Mathf.Pow(1.35f, offensiveTowersBuilt - 4));
        }
    }

    // ---------- INPUT ----------
    public void StartPlacingStandard() { selectedTowerPrefab = standardTowerPrefab; isPlacing = true; }
    public void StartPlacingMelee() { selectedTowerPrefab = meleeTowerPrefab; isPlacing = true; }
    public void StartPlacingSniper() { selectedTowerPrefab = sniperTowerPrefab; isPlacing = true; }
    public void StartPlacingSlow() { selectedTowerPrefab = slowTowerPrefab; isPlacing = true; }
    public void StartPlacingEmpower() { selectedTowerPrefab = empowerTowerPrefab; isPlacing = true; }

    // ---------- UTILITY ----------
    void CancelPreview()
    {
        isPlacing = false;

        if (previewTower != null) Destroy(previewTower);
        if (rangeIndicator != null) Destroy(rangeIndicator);
    }

    void DisableScripts(GameObject obj)
    {
        foreach (MonoBehaviour m in obj.GetComponentsInChildren<MonoBehaviour>())
            m.enabled = false;
    }

    void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayer(child.gameObject, layer);
    }

    void SetTransparent(GameObject obj)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                Color c = m.color;
                c.a = 0.4f;
                m.color = c;
            }
        }
    }

    public int GetLastCost() { return GetCurrentCost(); }

    public int GetTowerCount()
    {
        return GetTotalTowersBuilt();
    }

    public void RegisterTowerLevel(int level)
    {
        if (level > highestTowerLevelEver)
            highestTowerLevelEver = level;
    }

    public int GetHighestTowerLevel()
    {
        return highestTowerLevelEver;
    }

    public void LoadData(int savedLastTowerCost, int savedTowersBuilt)
    {
        offensiveTowersBuilt = savedTowersBuilt; // simple fallback
    }
}
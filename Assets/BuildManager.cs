using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

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

    public TextMeshProUGUI standardCostText;
    public TextMeshProUGUI meleeCostText;
    public TextMeshProUGUI sniperCostText;
    public TextMeshProUGUI slowCostText;
    public TextMeshProUGUI empowerCostText;

    private bool isPlacing = false;
    private GameObject selectedTowerPrefab;

    private GameObject previewTower;
    private GameObject rangeIndicator;

    // ✅ ONLY thing that matters now
    private int totalTowersBuilt = 0;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        UpdateCostUI();

        if (!isPlacing) return;

        HandlePreview();

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            TryPlace();

        if (Input.GetMouseButtonDown(1))
            CancelPreview();
    }

    // ---------------- UI BUTTONS ----------------

    public void StartPlacingStandard() { selectedTowerPrefab = standardTowerPrefab; isPlacing = true; }
    public void StartPlacingMelee() { selectedTowerPrefab = meleeTowerPrefab; isPlacing = true; }
    public void StartPlacingSniper() { selectedTowerPrefab = sniperTowerPrefab; isPlacing = true; }
    public void StartPlacingSlow() { selectedTowerPrefab = slowTowerPrefab; isPlacing = true; }
    public void StartPlacingEmpower() { selectedTowerPrefab = empowerTowerPrefab; isPlacing = true; }

    // ---------------- PREVIEW ----------------

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

            if (prefabTower != null && prefabTower.towerType != Tower.TowerType.Sniper)
            {
                if (rangeIndicator == null)
                    rangeIndicator = Instantiate(rangeIndicatorPrefab);

                rangeIndicator.transform.position = pos;

                RangeIndicator ri = rangeIndicator.GetComponent<RangeIndicator>();
                if (ri != null)
                {
                    float displayRange = prefabTower.range;

                    if (prefabTower.towerType == Tower.TowerType.Empower)
                        displayRange = 3f;

                    ri.SetRange(displayRange);
                }
            }
        }
    }

    void TryPlace()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, pathMask))
            return;

        if (Physics.Raycast(ray, out hit, 100f, groundMask))
            PlaceTower(hit.point);
    }

    void PlaceTower(Vector3 position)
    {
        int cost = GetCurrentCost();

        if (!GameManager.instance.SpendMoney(cost))
            return;

        position.y = 0.5f;

        GameObject tower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);
        SetLayer(tower, LayerMask.NameToLayer("Default"));

        Tower t = tower.GetComponent<Tower>();

        if (t != null)
        {
            // ✅ Level = number of towers AFTER placement
            int newLevel = totalTowersBuilt + 1;

            bool isSupport = IsSupportTower(selectedTowerPrefab);

            int maxLevel = totalTowersBuilt + 1;
            newLevel = Mathf.Clamp(newLevel, 1, maxLevel);

            t.SetInitialLevel(newLevel, isSupport);
        }

        totalTowersBuilt++;

        CancelPreview();
    }

    // ---------------- COST ----------------

    int GetCurrentCost()
    {
        if (totalTowersBuilt == 0) return 30;
        if (totalTowersBuilt == 1) return 45;
        if (totalTowersBuilt == 2) return 60;
        if (totalTowersBuilt == 3) return 90;
        if (totalTowersBuilt == 4) return 150;

        return Mathf.FloorToInt(150 * Mathf.Pow(1.35f, totalTowersBuilt - 4));
    }

    bool IsSupportTower(GameObject prefab)
    {
        return prefab == slowTowerPrefab || prefab == empowerTowerPrefab;
    }

    // ---------------- UI ----------------

    void UpdateCostUI()
    {
        int cost = GetCurrentCost();

        standardCostText.text = "Standard ($" + cost + ")";
        meleeCostText.text = "Melee ($" + cost + ")";
        sniperCostText.text = "Sniper ($" + cost + ")";
        slowCostText.text = "Slow ($" + cost + ")";
        empowerCostText.text = "Empower ($" + cost + ")";
    }

    // ---------------- UTIL ----------------

    void CancelPreview()
    {
        isPlacing = false;

        if (previewTower != null)
            Destroy(previewTower);

        if (rangeIndicator != null)
            Destroy(rangeIndicator);
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

    public int GetLastCost()
    {
        return GetCurrentCost();
    }

    public int GetTowerCount()
    {
        return totalTowersBuilt;
    }

    public void LoadData(int savedLastTowerCost, int savedTowersBuilt)
    {
        totalTowersBuilt = savedTowersBuilt;
    }
}
using UnityEngine;
using TMPro;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;

    public GameObject standardTowerPrefab;
    public GameObject meleeTowerPrefab;
    public GameObject sniperTowerPrefab;

    public GameObject rangeIndicatorPrefab;

    public LayerMask groundMask;
    public LayerMask pathMask;

    public TextMeshProUGUI standardCostText;
    public TextMeshProUGUI meleeCostText;
    public TextMeshProUGUI sniperCostText;

    private bool isPlacing = false;
    private GameObject selectedTowerPrefab;

    private GameObject previewTower;
    private GameObject rangeIndicator;

    private int towersBuilt = 0;

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
        {
            TryPlace();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPreview();
        }
    }

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
                // ✅ SNIPER = NO RANGE INDICATOR
                if (prefabTower.towerType == Tower.TowerType.Sniper)
                {
                    if (rangeIndicator != null)
                    {
                        Destroy(rangeIndicator);
                        rangeIndicator = null;
                    }
                }
                else
                {
                    // ✅ CREATE IF NEEDED
                    if (rangeIndicator == null)
                    {
                        rangeIndicator = Instantiate(rangeIndicatorPrefab);
                    }

                    rangeIndicator.transform.position = pos;

                    RangeIndicator ri = rangeIndicator.GetComponent<RangeIndicator>();
                    if (ri != null)
                    {
                        ri.SetRange(prefabTower.range);
                    }
                }
            }
        }
    }

    void TryPlace()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ❌ prevent building on path
        if (Physics.Raycast(ray, out hit, 100f, pathMask)) return;

        if (Physics.Raycast(ray, out hit, 100f, groundMask))
        {
            PlaceTower(hit.point);
        }
    }

    void PlaceTower(Vector3 position)
    {
        int cost = GetCurrentCost();

        if (!GameManager.instance.SpendMoney(cost)) return;

        position.y = 0.5f;

        GameObject tower = Instantiate(selectedTowerPrefab, position, Quaternion.identity);

        // ✅ ENSURE REAL TOWER IS NOT IGNORE RAYCAST
        SetLayer(tower, LayerMask.NameToLayer("Default"));

        Tower t = tower.GetComponent<Tower>();
        if (t != null)
        {
            t.ApplyScaling(towersBuilt);
        }

        towersBuilt++;

        CancelPreview();
    }

    void CancelPreview()
    {
        isPlacing = false;

        if (previewTower != null) Destroy(previewTower);
        if (rangeIndicator != null) Destroy(rangeIndicator);
    }

    void DisableScripts(GameObject obj)
    {
        foreach (MonoBehaviour m in obj.GetComponentsInChildren<MonoBehaviour>())
        {
            m.enabled = false;
        }
    }

    void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
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

    public void StartPlacingStandard()
    {
        selectedTowerPrefab = standardTowerPrefab;
        isPlacing = true;
    }

    public void StartPlacingMelee()
    {
        selectedTowerPrefab = meleeTowerPrefab;
        isPlacing = true;
    }

    public void StartPlacingSniper()
    {
        selectedTowerPrefab = sniperTowerPrefab;
        isPlacing = true;
    }

    int GetCurrentCost()
    {
        if (towersBuilt == 0) return 30;
        if (towersBuilt == 1) return 45;
        if (towersBuilt == 2) return 60;
        if (towersBuilt == 3) return 90;
        if (towersBuilt == 4) return 150;

        float cost = 150 * Mathf.Pow(1.35f, towersBuilt - 4);
        return Mathf.FloorToInt(cost);
    }

    void UpdateCostUI()
    {
        int cost = GetCurrentCost();

        if (standardCostText != null)
            standardCostText.text = "Standard ($" + cost + ")";

        if (meleeCostText != null)
            meleeCostText.text = "Melee ($" + cost + ")";

        if (sniperCostText != null)
            sniperCostText.text = "Sniper ($" + cost + ")";
    }

    // ✅ SAVE SYSTEM SUPPORT

    public int GetLastCost()
    {
        return GetCurrentCost();
    }

    public int GetTowerCount()
    {
        return towersBuilt;
    }

    public void LoadData(int savedLastTowerCost, int savedTowersBuilt)
    {
        towersBuilt = savedTowersBuilt;
    }
}
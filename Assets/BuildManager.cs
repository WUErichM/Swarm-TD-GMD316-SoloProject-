using UnityEngine;
using TMPro;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;

    public GameObject towerPrefab;

    public LayerMask groundMask;
    public LayerMask pathMask;

    public TextMeshProUGUI costText;

    private bool isPlacing = false;

    private int towersBuilt = 0;
    private int lastTowerCost = 25;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        UpdateCostUI();

        if (!isPlacing) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, pathMask)) return;

            if (Physics.Raycast(ray, out hit, 100f, groundMask))
            {
                PlaceTower(hit.point);
            }
        }
    }

    public void StartPlacing()
    {
        isPlacing = true;
    }

    void PlaceTower(Vector3 position)
    {
        int cost = lastTowerCost;

        if (!GameManager.instance.SpendMoney(cost)) return;

        position.y = 0.5f;

        GameObject tower = Instantiate(towerPrefab, position, Quaternion.identity);

        Tower t = tower.GetComponent<Tower>();
        if (t != null)
        {
            t.ApplyScaling(towersBuilt);
        }

        // Increase tower cost for next placement
        lastTowerCost = Mathf.FloorToInt(lastTowerCost * 1.25f);

        towersBuilt++;
        isPlacing = false;
    }

    void UpdateCostUI()
    {
        if (costText != null)
        {
            costText.text = "Build Tower ($" + lastTowerCost + ")";
        }
    }

    public int GetLastCost()
    {
        return lastTowerCost;
    }

    public int GetTowerCount()
    {
        return towersBuilt;
    }

    // ✅ Fully restore tower count AND last tower cost when loading a save
    public void LoadData(int savedLastTowerCost, int savedTowersBuilt)
    {
        lastTowerCost = savedLastTowerCost;
        towersBuilt = savedTowersBuilt;
    }
}
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Maps")]
    [SerializeField] private GameObject cityMap;
    [SerializeField] private GameObject labMap;
    [SerializeField] private GameObject flatMap;
    [SerializeField] private GameObject officeMap;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadCity()
    {
        SetActiveMap(cityMap);
    }

    public void LoadLab()
    {
        SetActiveMap(labMap);
    }

    public void LoadFlat()
    {
        SetActiveMap(flatMap);
    }

    public void LoadOffice()
    {
        SetActiveMap(officeMap);
    }

    private void SetActiveMap(GameObject activeMap)
    {
        cityMap.SetActive(false);
        labMap.SetActive(false);
        flatMap.SetActive(false);
        officeMap.SetActive(false);

        activeMap.SetActive(true);
    }
}
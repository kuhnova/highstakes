using UnityEngine;

public class BuildingEntrance : MonoBehaviour, IInteractable
{
    public enum BuildingType
    {
        Lab,
        Flat,
        Office,
        City
    }

    [SerializeField] private BuildingType buildingType;
    [SerializeField] private Transform spawnPoint;

    public void Interact()
    {
        switch (buildingType)
        {
            case BuildingType.Lab:
                MapManager.Instance.LoadLab();
                break;

            case BuildingType.Flat:
                MapManager.Instance.LoadFlat();
                break;

            case BuildingType.Office:
                MapManager.Instance.LoadOffice();
                break;
            case BuildingType.City:
                MapManager.Instance.LoadCity();
                break;
        }

        MovePlayer();
    }

    private void MovePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = spawnPoint.position;
    }
}
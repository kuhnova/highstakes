using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance;

    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private Image statusImage;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateQuest(QuestData quest)
    {
        statusText.text = "ACTIVE";
        objectiveText.text = quest.description;
        statusImage.color = Color.yellow;
    }

    public void CompleteQuest(QuestData quest)
    {
        statusText.text = "COMPLETED";
        statusImage.color = Color.green;
    }

    public void SetLockedObjective(string lockedMessage)
    {
        statusText.text = "LOCKED";
        objectiveText.text = lockedMessage;
        statusImage.color = Color.gray;
    }
}
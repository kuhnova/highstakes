using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [SerializeField] private List<QuestData> quests;

    [SerializeField]public int currentQuestIndex { get; private set; } = 0;

    public bool isActive = false;

    [HideInInspector]
    public string npcName = "Lily";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartQuest();
    }

    
    public QuestData CurrentQuest
    {
        get
        {
            if (currentQuestIndex >= 0 && currentQuestIndex < quests.Count)
                return quests[currentQuestIndex];
            return null;
        }
    }

    
    public void StartQuest()
    {
        if (currentQuestIndex >= quests.Count) return;

        QuestData quest = quests[currentQuestIndex];

        if (quest.requiresAcquisition)
        {
            isActive = false;
            
            MessageUI.Instance?.ShowMessage("SYSTEM", "Find " + quest.acquiringNPCName);
            QuestUI.Instance?.SetLockedObjective("Find " + quest.acquiringNPCName);
        }
        else
        {
            isActive = true;
            MessageUI.Instance?.ShowMessage("SYSTEM", "New objective received");
            QuestUI.Instance?.UpdateQuest(quest);
        }
    }

    
    public bool AcquireQuestFromNPC(string acquiringName)
    {
        QuestData quest = CurrentQuest;
        if (quest == null) return false;

        if (!quest.requiresAcquisition)
        {
            return false;
        }

        if (!string.Equals(quest.acquiringNPCName, acquiringName))
        {
            return false;
        }

        
        isActive = true;
        npcName = acquiringName;
        MessageUI.Instance?.ShowMessage("SYSTEM", "New objective received");
        QuestUI.Instance?.UpdateQuest(quest);
        return true;
    }

    public void CompleteQuest(QuestData quest)
    {
        MessageUI.Instance?.ShowMessage("SYSTEM", "Hack successful");

        if (currentQuestIndex >= quests.Count) return;

        if (quests[currentQuestIndex] != quest)
            return;

        Debug.Log("Completed: " + quest.questTitle);

        QuestUI.Instance?.CompleteQuest(quest);

        currentQuestIndex++;

        Invoke(nameof(StartQuest), 1.5f);

        DoorUnlock();
    }

    void DoorUnlock()
    {
        if (currentQuestIndex == 6)
        {
            var go = GameObject.Find("DoorLabQuest");
            if (go != null) Destroy(go);
        }
    }
}
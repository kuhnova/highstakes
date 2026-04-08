using System.Reflection;
using UnityEngine;

public class NPCQuestGiver : MonoBehaviour, IInteractable
{
    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointReachThreshold = 0.1f;

    [Header("Player reference (assignable)")]
    [SerializeField] private Transform playerTransform;

    [Header("Interaction")]
    [SerializeField] private string npcName = "QuestNPC";
    [SerializeField] private string interactMessage = "Hello there!";
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private bool stopOnInteract = true;

    [Header("Quest / Reward")]
    [SerializeField] private int minimumRequiredQuestNumber = 0;

    [SerializeField] private string questOfferMessage = "I have a quest for you!";
    [SerializeField] private string notReadyMessage = "Come back later.";
    [SerializeField] private string alreadyCompletedMessage = "Thank you!";
    [SerializeField] private int rewardAmount = 25;
    [SerializeField] private bool rewardGiven = false;

    private int currentIndex = 0;
    private bool stoppedForInteraction;
    public string npcNames = "Lily";

    private void Start()
    {


        if (playerTransform == null)
        {
            var foundByTag = GameObject.FindGameObjectWithTag("Player");
            if (foundByTag != null)
                playerTransform = foundByTag.transform;
        }

        if (playerTransform == null)
        {
            var foundByName = GameObject.Find("Player");
            if (foundByName != null)
                playerTransform = foundByName.transform;
        }

    }

    private void Update()
    {
        
        if (stoppedForInteraction)
        {
            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist > interactRange + 0.25f) 
                {
                    stoppedForInteraction = false;
                    HideMessage();
                    ResumePatrol();
                }
            }
            return;
        }

        PatrolUpdate();
    }

    private void PatrolUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        transform.position = Vector2.MoveTowards(transform.position, target.position, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < waypointReachThreshold)
            currentIndex = (currentIndex + 1) % waypoints.Length;
    }

    private int FindNearestWaypointIndex()
    {
        if (waypoints == null || waypoints.Length == 0) return 0;

        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            float d = Vector2.Distance(transform.position, waypoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    public void Interact()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist > interactRange) return;

        
        if (stopOnInteract)
        {
            stoppedForInteraction = true;
            
            currentIndex = FindNearestWaypointIndex();
        }

        
        if (QuestManager.Instance == null)
        {
            ShowMessage(interactMessage);
            return;
        }

        int playerQuestIndex = QuestManager.Instance.currentQuestIndex;

        
        if (playerQuestIndex < minimumRequiredQuestNumber)
        {
            ShowMessage(notReadyMessage);
            return;
        }

        
        if (playerQuestIndex == minimumRequiredQuestNumber)
        {
           
            var currentQuest = QuestManager.Instance.CurrentQuest;
            if (currentQuest != null && currentQuest.requiresAcquisition && currentQuest.acquiringNPCName == npcNames)
            {
                
                bool acquired = QuestManager.Instance.AcquireQuestFromNPC(npcNames);
                if (acquired)
                {
                    ShowMessage(questOfferMessage);
                    return;
                }
                else
                {
                    ShowMessage(notReadyMessage);
                    return;
                }
            }
            else
            {
                
                ShowMessage(interactMessage);
                return;
            }
        }

        
        if (playerQuestIndex > minimumRequiredQuestNumber && !rewardGiven)
        {
            if (PlayerStats.Instance != null && rewardAmount > 0)
            {
                PlayerStats.Instance.AddMoney(rewardAmount);
            }
            rewardGiven = true;
            ShowMessage(alreadyCompletedMessage);
            return;
        }

        
        ShowMessage(interactMessage);
    }

    private void ResumePatrol()
    {
        
        if (waypoints != null && waypoints.Length > 0)
            currentIndex = (currentIndex + 1) % waypoints.Length;
    }

    private void ShowMessage(string text)
    {
        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(npcName, text);
        else
            Debug.Log($"{npcName}: {text}");
    }

    private void HideMessage()
    {
        
        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(string.Empty, string.Empty);
    }

}
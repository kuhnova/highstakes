using UnityEngine;

public class NPCPatrol : MonoBehaviour, IInteractable
{
    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointReachThreshold = 0.1f;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float chaseTriggerDistance = 2f;
    [SerializeField] private float chaseLoseDistance = 4f;
    [SerializeField] private float catchDistance = 0.5f;
    [SerializeField] private Transform chaseAnchor;

    [Header("Player reference (assignable)")]
    [SerializeField] private Transform playerTransform;
    private GameObject player;

    [Header("Bribe / Interaction")]
    [SerializeField] private int bribeAmount = 10;
    [SerializeField] private float caughtCooldown = 2f;
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string interactMessage = "Hello there!";
    [SerializeField] private float interactRange = 1.5f;

    private int currentIndex = 0;
    public enum State { Patrol, Chasing }
    public State state = State.Patrol;

    private float caughtTimer;
    private bool pausedForBribe;

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

        player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player != null ? player.transform : null;

        
        if (pausedForBribe) return;

        
        if (caughtTimer > 0f)
        {
            caughtTimer -= Time.deltaTime;
            return;
        }

        
        if (playerTransform == null)
        {
            PatrolUpdate();
            return;
        }

        Vector2 playerPos = playerTransform.position;

        bool shouldChase = false;
        if (chaseAnchor != null)
        {
            float distToAnchor = Vector2.Distance(playerTransform.position, chaseAnchor.position);
            shouldChase = distToAnchor <= chaseTriggerDistance;
        }
        else
        {
            float distToNpc = Vector2.Distance(transform.position, playerTransform.position);
            shouldChase = distToNpc <= chaseTriggerDistance;
        }

        

        switch (state)
        {
            case State.Patrol:
                PatrolUpdate();
                if (shouldChase)
                {
                    state = State.Chasing;
                }
                break;

            case State.Chasing:
                
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);

                
                if (Vector2.Distance(transform.position, playerPos) <= catchDistance)
                {
                    OnPlayerCaught();
                    break;
                }

                float loseDist = chaseAnchor != null
                    ? Vector2.Distance(playerTransform.position, chaseAnchor.position)
                    : Vector2.Distance(playerTransform.position, transform.position);

                if (loseDist > chaseLoseDistance)
                    state = State.Patrol;
                break;
        }
    }

    private void PatrolUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector2.MoveTowards(transform.position, target.position, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < waypointReachThreshold)
            currentIndex = (currentIndex + 1) % waypoints.Length;
    }

    private void OnPlayerCaught()
    {
        
        if (pausedForBribe) return;

        Debug.Log($"{name}: Player caught!");
        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(npcName, "You were caught!");

        
        caughtTimer = caughtCooldown;

        
        if (playerTransform != null)
        {
            var pm = playerTransform.GetComponent<PlayerMovement>();
            if (pm != null) pm.SetMovementEnabled(false);
        }

        
        if (BribeUI.Instance != null)
        {
            pausedForBribe = true;
            BribeUI.Instance.Show(this, bribeAmount);
        }
        else
        {
            
            GameManager.Instance?.GameOver();
        }

        Resume();
    }

    
    public void OnBribeAccepted()
    {
        
        if (playerTransform != null)
        {
            var pm = playerTransform.GetComponent<PlayerMovement>();
            if (pm != null) pm.SetMovementEnabled(true);
        }

        pausedForBribe = false;

        Resume();
    }

   
    public void OnBribeRefused()
    {
        pausedForBribe = false;
       
        if (playerTransform != null)
        {
            var pm = playerTransform.GetComponent<PlayerMovement>();
            if (pm != null) pm.SetMovementEnabled(true);
        }

        GameManager.Instance?.GameOver();

        Resume();
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

        if (MessageUI.Instance != null)
            MessageUI.Instance.ShowMessage(npcName, interactMessage);
        else
            Debug.Log($"{npcName}: {interactMessage}");
    }

    void Resume()
    {

        currentIndex = FindNearestWaypointIndex();
        if (waypoints != null && waypoints.Length > 0)
            currentIndex = (currentIndex + 1) % waypoints.Length;

        state = State.Patrol;
    }
}
using System;
using UnityEngine;

public class ObjectiveArrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform arrowTransform;

    [SerializeField] private Camera cam;

    [Header("Behavior")]
    [SerializeField] private float hideDistance = 2f;

    [SerializeField] private float rotationOffsetZ = 180f;

    private QuestData lastQuest = null;
    private Transform target = null;
    private QuestManager questManager = null;

    private void Start()
    {
        questManager = QuestManager.Instance;

        if (cam == null) cam = Camera.main;

        if (arrowTransform == null)
        {
            var child = transform.Find("Arrow");
            if (child == null && transform.childCount > 0)
                child = transform.GetChild(0);
            if (child != null) arrowTransform = child;
        }

        if (arrowTransform != null)
            arrowTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (questManager == null) questManager = QuestManager.Instance;
        if (questManager == null) return;

        var quest = questManager.CurrentQuest;

        bool shouldShow = quest != null && quest.requiresAcquisition && !questManager.isActive;

        if (shouldShow)
        {
            if (!ReferenceEquals(quest, lastQuest))
            {
                UpdateTarget(quest);
                lastQuest = quest;
            }

            if (target == null)
                UpdateTarget(quest);

            if (target != null && arrowTransform != null)
            {
                float sqrDist = (target.position - transform.position).sqrMagnitude;
                bool closeEnough = sqrDist <= hideDistance * hideDistance;

                if (!closeEnough)
                {
                    if (!arrowTransform.gameObject.activeSelf) arrowTransform.gameObject.SetActive(true);
                    PointToTarget();
                }
                else
                {
                    if (arrowTransform.gameObject.activeSelf) arrowTransform.gameObject.SetActive(false);
                }

                return;
            }

            if (arrowTransform != null && arrowTransform.gameObject.activeSelf)
                arrowTransform.gameObject.SetActive(false);
        }
        else
        {
            if (arrowTransform != null && arrowTransform.gameObject.activeSelf)
                arrowTransform.gameObject.SetActive(false);
            lastQuest = null;
            target = null;
        }
    }

    private void UpdateTarget(QuestData quest)
    {
        target = null;
        if (quest == null) return;

        string desiredName = quest.acquiringNPCName;
        if (string.IsNullOrEmpty(desiredName)) return;

        var markers = FindObjectsOfType<QuestTargetMarker>();
        foreach (var m in markers)
        {
            if (string.Equals(m.npcName, desiredName, StringComparison.OrdinalIgnoreCase))
            {
                target = m.transform;
                return;
            }
        }

        var go = GameObject.Find(desiredName);
        if (go != null) target = go.transform;
    }

    private void PointToTarget()
    {
        if (target == null || arrowTransform == null || cam == null) return;

        Vector2 playerScreen = cam.WorldToScreenPoint(transform.position);
        Vector2 targetScreen = cam.WorldToScreenPoint(target.position);

        Vector2 screenDir = targetScreen - playerScreen;

        if (screenDir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(screenDir.y, screenDir.x) * Mathf.Rad2Deg;

        Vector3 local = arrowTransform.localEulerAngles;
        local.x = 0f;
        local.y = 0f;
        local.z = angle + rotationOffsetZ;
        arrowTransform.localEulerAngles = local;
    }
}
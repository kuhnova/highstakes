using System;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour, IInteractable
{
    private enum HackType { Pattern, Sequence, Logic, CellSequence, Finish }

    [SerializeField] private QuestData quest;
    [SerializeField] private HackType hackType = HackType.Pattern;

    [Header("Pattern (PatternPuzzle)")]
    [SerializeField] private int numberOfPatternCells = 7;

    [Header("Sequence (HackingSequence)")]
    [SerializeField] private List<int> customSequence = new List<int>();

    [Header("Logic (LogicPuzzle)")]
    [SerializeField] private int logicGridRows = 6;
    [SerializeField] private int logicGridCols = 6;
    [SerializeField] private int logicNumberOfPairs = 3;
    [SerializeField] private List<Color> logicNodeColors = new List<Color> { Color.red, Color.green, Color.blue };

    [Header("Cell Sequence / Grid (GridSequence)")]
    [SerializeField] private int gridRows = 3;
    [SerializeField] private int gridColumns = 3;
    [SerializeField] private Vector2 gridCellSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 gridSpacing = new Vector2(5f, 5f);
    [SerializeField] private bool gridStartAscending = true;
    [SerializeField] private float timerTimeLimit = 10f;

    private Action onHackSuccess;
    private Action onHackClosed;

    void Update()
    {
        if (quest == null || QuestManager.Instance == null) return;

       
        bool shouldShow = (QuestManager.Instance.currentQuestIndex == quest.questOrder) && QuestManager.Instance.isActive;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = shouldShow;
    }

    public void Interact()
    {
        if (QuestManager.Instance == null)
        {
            return;
        }

        if (quest == null)
        {
            return;
        }

        
        if (!(QuestManager.Instance.currentQuestIndex == quest.questOrder && QuestManager.Instance.isActive))
            return;

        if (HackingManager.Instance == null)
        {
            return;
        }

        onHackSuccess = () =>
        {
            QuestManager.Instance.CompleteQuest(quest);
            Unsubscribe();
        };

        onHackClosed = () =>
        {
            Unsubscribe();
        };

        HackingManager.Instance.HackSucceeded += onHackSuccess;
        HackingManager.Instance.HackClosed += onHackClosed;

        switch (hackType)
        {
            case HackType.Pattern:
                HackingManager.Instance.StartPatternHack(numberOfPatternCells);
                break;
            case HackType.Sequence:
                HackingManager.Instance.StartSequenceHack(customSequence != null && customSequence.Count > 0 ? customSequence : null);
                break;
            case HackType.Logic:
                HackingManager.Instance.StartLogicHack(logicGridRows, logicGridCols, logicNumberOfPairs, logicNodeColors, null);
                break;
            case HackType.CellSequence:
                HackingManager.Instance.StartCellSequenceHack(gridRows, gridColumns, gridCellSize, gridSpacing, gridStartAscending, timerTimeLimit);
                break;
            case HackType.Finish:
                HackingManager.Instance.FinishGame();
                break;
            default:
                HackingManager.Instance.StartPatternHack(numberOfPatternCells);
                break;
        }
    }

    private void Unsubscribe()
    {
        if (HackingManager.Instance == null) return;

        if (onHackSuccess != null)
            HackingManager.Instance.HackSucceeded -= onHackSuccess;

        if (onHackClosed != null)
            HackingManager.Instance.HackClosed -= onHackClosed;

        onHackSuccess = null;
        onHackClosed = null;
    }

    private void OnDisable()
    {
        Unsubscribe();
    }
}
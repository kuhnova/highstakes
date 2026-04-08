
using System;
using System.Collections.Generic;
using UnityEngine;

public class HackingManager : MonoBehaviour
{
    public static HackingManager Instance;

    [SerializeField] private GameObject hackingCanvas;

    [Header("Panels")]
    [SerializeField] private GameObject sequencePanel;
    [SerializeField] private GameObject patternPanel;
    [SerializeField] private GameObject logicPanel;
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private GameObject cellSequencePanel;

    public event Action HackSucceeded;
    public event Action HackClosed;


    private void Awake()
    {
        Instance = this;
        CloseAll();
    }

    public void StartSequenceHack(List<int> customSequence = null)
    {
        OpenPanel(sequencePanel);

        var seq = this.gameObject.GetComponent<HackingSequence>();
        if (seq != null && customSequence != null && customSequence.Count > 0)
        {
            seq.SetCorrectSequence(customSequence);
        }
    }

    public void StartPatternHack(int number)
    {
        OpenPanel(patternPanel);
        var p = this.gameObject.GetComponent<PatternPuzzle>();
        if (p != null)
            p.DelayedStart(number);
    }

    public void StartLogicHack(int rows = -1, int cols = -1, int pairs = -1, List<Color> colors = null, Vector2? cellSize = null)
    {
        OpenPanel(logicPanel);

        var logic = this.gameObject.GetComponent<LogicPuzzle>();
        if (logic == null) return;

        if (rows > 0 && cols > 0) logic.SetGridSize(rows, cols);
        if (pairs > 0) logic.SetNumberOfPairs(pairs);
        if (colors != null && colors.Count > 0) logic.SetNodeColors(colors);
        if (cellSize.HasValue) logic.SetCellSize(cellSize.Value);

        logic.DelayedStart();
    }

    public void StartCellSequenceHack(int rows = -1, int columns = -1, Vector2? cellSize = null, Vector2? spacing = null, bool? startAscending = null, float? timeLimit = null)
    {
        OpenPanel(cellSequencePanel);

        var gridSeq = this.gameObject.GetComponent<GridSequence>();
        if (gridSeq == null) return;

        if (rows > 0 && columns > 0) gridSeq.SetDimensions(rows, columns);
        if (cellSize.HasValue) gridSeq.SetCellSize(cellSize.Value);
        if (spacing.HasValue) gridSeq.SetSpacing(spacing.Value);
        if (startAscending.HasValue) gridSeq.SetStartAscending(startAscending.Value);

        gridSeq.SetTimerOptions(timeLimit);

        gridSeq.DelayedStart();
    }

    public void FinishGame()
    {
        OpenPanel(finishPanel);
    }

    private void OpenPanel(GameObject panel)
    {
        hackingCanvas.SetActive(true);
        CloseAll();
        panel.SetActive(true);
    }

    public void NotifyHackSuccess()
    {
        HackSucceeded?.Invoke();
        CloseHack();
    }

    public void CloseHack()
    {
        hackingCanvas.SetActive(false);
        CloseAll();
        HackClosed?.Invoke();
    }

    public void CloseAll()
    {
        sequencePanel.SetActive(false);
        patternPanel.SetActive(false);
        logicPanel.SetActive(false);
        cellSequencePanel.SetActive(false);
        finishPanel.SetActive(false);
    }
}
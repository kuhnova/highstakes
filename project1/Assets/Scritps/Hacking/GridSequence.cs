using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;


public class GridSequence : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int rows = 3;
    [SerializeField] private int columns = 3;

    [Header("Layout")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private Vector2 cellSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 spacing = new Vector2(5f, 5f);

    [Header("Prefabs")]
    [SerializeField] private Button cellButtonPrefab;
    [SerializeField] private GameObject cellBackgroundPrefab;

    [Header("Sequence")]
    [SerializeField] private bool startAscending = true;

    [Header("Behavior")]
    [SerializeField] private bool autoGenerateOnStart = true;
    [SerializeField] private bool hideButtonsWhenClicked = true;
    [SerializeField] private float wrongFlashDuration = 0.15f;
    [SerializeField] private Color wrongFlashColor = Color.red;

    [Header("Timer")]
    [SerializeField] private bool useTimer = false;
    [SerializeField] private float timeLimit = 10f;
    [SerializeField] private bool resetOnTimeout = true;
    [SerializeField] private float timeoutFlashDuration = 0.15f;
    [SerializeField] private TextMeshProUGUI timerText;


    private List<Button> spawnedButtons = new List<Button>();
    private int totalCells;
    private int nextNumber;
    private bool ascending;

    private Coroutine timerCoroutine;

    public void DelayedStart()
    {
        ascending = startAscending;
        totalCells = rows * columns;

        if (autoGenerateOnStart)
        {
            GenerateGrid();
        }
    }

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        if (cellButtonPrefab == null)
        {
            return;
        }

        if (gridParent == null)
        {
            return;
        }

        StopTimer();

        ClearGrid();

        EnsureGridLayoutConfigured();

        totalCells = Mathf.Max(1, rows * columns);
        List<int> numbers = CreateShuffledNumbers(totalCells);

        
        nextNumber = ascending ? 1 : totalCells;

        for (int i = 0; i < totalCells; i++)
        {
            int number = numbers[i];
            
            GameObject cellRoot;
            if (cellBackgroundPrefab != null)
            {
                cellRoot = Instantiate(cellBackgroundPrefab, gridParent);
                
                cellRoot.name = $"Cell_{i + 1}_BG";
            }
            else
            {
                
                cellRoot = new GameObject($"Cell_{i + 1}_Container", typeof(RectTransform));
                cellRoot.transform.SetParent(gridParent, false);
            }

            
            Button btn = Instantiate(cellButtonPrefab, cellRoot.transform);
            btn.name = $"CellButton_{number}";
            
            RectTransform btnRt = btn.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0, 0);
            btnRt.anchorMax = new Vector2(1, 1);
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;

            SetButtonText(btn.gameObject, number.ToString());

            
            int capturedNumber = number;
            btn.onClick.AddListener(() => OnCellClicked(capturedNumber, btn));

            spawnedButtons.Add(btn);
        }

        StartTimerIfNeeded();
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject.transform.parent ? btn.gameObject.transform.parent.gameObject : btn.gameObject);
        }

        spawnedButtons.Clear();

        
        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                var child = gridParent.GetChild(i);
                
                if (child.name.StartsWith("Cell_") || child.name.StartsWith("CellButton_"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        ClearTimerText();
    }

    public void SetSequenceDirection(bool ascendingOrder)
    {
        ascending = ascendingOrder;
        startAscending = ascendingOrder;
        
        totalCells = Mathf.Max(1, rows * columns);
        nextNumber = ascending ? 1 : totalCells;
    }

    private void EnsureGridLayoutConfigured()
    {
        GridLayoutGroup gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = gridParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.cellSize = cellSize;
        gridLayout.spacing = spacing;
        
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = Mathf.Max(1, columns);
        
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
    }

    private List<int> CreateShuffledNumbers(int count)
    {
        List<int> list = new List<int>(count);
        for (int i = 1; i <= count; i++)
            list.Add(i);

        
        System.Random rnd = new System.Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            int tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }

        return list;
    }

    private void OnCellClicked(int number, Button button)
    {
        if (ascending)
        {
            if (number != nextNumber)
            {
                StartCoroutine(FlashWrong(button));
                return;
            }

            HandleCorrectClick(button);
            nextNumber++;
        }
        else
        {
            if (number != nextNumber)
            {
                StartCoroutine(FlashWrong(button));
                return;
            }

            HandleCorrectClick(button);
            nextNumber--;
        }

        
        if ((ascending && nextNumber > totalCells) || (!ascending && nextNumber < 1))
        {
            StopTimer();

            if (HackingManager.Instance != null)
                HackingManager.Instance.NotifyHackSuccess();

        }
    }

    private void HandleCorrectClick(Button button)
    {
        
        if (hideButtonsWhenClicked)
        {
            button.interactable = false;
            
            var canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }
        else
        {
            button.interactable = false;
        }
    }

    private IEnumerator FlashWrong(Button button)
    {
        if (button == null)
            yield break;

        Graphic gfx = button.GetComponent<Graphic>();
        if (gfx == null)
            gfx = button.GetComponentInChildren<Graphic>();

        if (gfx == null)
        {
            
            yield break;
        }

        Color original = gfx.color;
        gfx.color = wrongFlashColor;
        yield return new WaitForSeconds(wrongFlashDuration);
        gfx.color = original;
    }

    private IEnumerator TimerCoroutine()
    {
        float remaining = Mathf.Max(0f, timeLimit);

        UpdateTimerText(remaining);

        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            UpdateTimerText(Mathf.Max(0f, remaining));
            yield return null;
        }

        timerCoroutine = null;
        UpdateTimerText(0f);
        
        StartCoroutine(FlashAllAndReset());
    }

    private IEnumerator FlashAllAndReset()
    {
        List<Graphic> graphics = new List<Graphic>();
        List<Color> originals = new List<Color>();

        foreach (var btn in spawnedButtons)
        {
            if (btn == null)
                continue;

            Graphic g = btn.GetComponent<Graphic>() ?? btn.GetComponentInChildren<Graphic>();
            if (g != null)
            {
                graphics.Add(g);
                originals.Add(g.color);
                g.color = wrongFlashColor;
            }
        }

        yield return new WaitForSeconds(timeoutFlashDuration);

        for (int i = 0; i < graphics.Count; i++)
        {
            if (graphics[i] != null)
                graphics[i].color = originals[i];
        }

        if (resetOnTimeout)
        {
            GenerateGrid();
        }
        else
        {
            ResetBoardState();
            
            StartTimerIfNeeded();
        }
    }

    private void ResetBoardState()
    {
        totalCells = Mathf.Max(1, rows * columns);
        nextNumber = ascending ? 1 : totalCells;

        foreach (var btn in spawnedButtons)
        {
            if (btn == null)
                continue;

            btn.interactable = true;

            if (hideButtonsWhenClicked)
            {
                var canvasGroup = btn.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = btn.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;
            }
        }
    }

    private void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        ClearTimerText();
    }

    private void StartTimerIfNeeded()
    {
        StopTimer();

        if (!useTimer || timeLimit <= 0f)
            return;

        // Show timer UI if assigned
        if (timerText != null)
            timerText.gameObject.SetActive(true);

        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private void UpdateTimerText(float remaining)
    {
        if (timerText == null)
            return;

        if (remaining <= 0f)
        {
            timerText.text = "0.0s";
            return;
        }

        if (remaining >= 60f)
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{minutes}:{seconds:00}";
        }
        else
        {
            timerText.text = $"{remaining:0.0}s";
        }
    }

    private void ClearTimerText()
    {
        if (timerText == null)
            return;

        timerText.text = string.Empty;
        
    }

    private void SetButtonText(GameObject buttonGameObject, string text)
    {
        

        
        var tmpText = buttonGameObject.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }

        
    }


    public void SetDimensions(int newRows, int newColumns)
    {
        rows = Mathf.Max(1, newRows);
        columns = Mathf.Max(1, newColumns);
    }

    public void SetCellSize(Vector2 newCellSize)
    {
        cellSize = newCellSize;
    }

    public void SetSpacing(Vector2 newSpacing)
    {
        spacing = newSpacing;
    }

    public void SetStartAscending(bool ascendingOrder)
    {
        startAscending = ascendingOrder;
    }

    public void SetTimerOptions(float? timeLimitOverride = null)
    {
        if (timeLimitOverride.HasValue) timeLimit = timeLimitOverride.Value;
    }
}
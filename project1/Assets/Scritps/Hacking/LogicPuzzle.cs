using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LogicPuzzle : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int gridRows = 6;
    [SerializeField] private int gridCols = 6;
    [SerializeField] private Vector2 cellSize = new Vector2(80f, 80f);
    [SerializeField] private RectTransform gridContainer; 
    [SerializeField] private GameObject gridCellPrefab; 

    [Header("Nodes")]
    [SerializeField] private GameObject nodePrefab; 
    [SerializeField] private RectTransform nodeContainer; 
    [SerializeField] private int numberOfPairs = 3;
    [SerializeField] private List<Color> nodeColors = new List<Color> { Color.red, Color.green, Color.blue, Color.yellow };

    [Header("Connection visuals")]
    [SerializeField] private GameObject connectionSegmentPrefab; 

    [Header("Options")]
    [SerializeField] private bool allowBacktracking = true;

   
    private int[,] gridOccupied; 
    private readonly List<NodeBehaviour> nodes = new List<NodeBehaviour>();
    private readonly List<EstablishedConnection> established = new List<EstablishedConnection>();

   
    private bool drawing;
    private int drawingPairId;
    private NodeBehaviour drawingStartNode;
    private readonly List<Vector2Int> tempCells = new List<Vector2Int>();
    private readonly List<GameObject> tempVisuals = new List<GameObject>();

    private void Awake()
    {
        if (gridContainer == null) gridContainer = GetComponent<RectTransform>();
        if (nodeContainer == null) nodeContainer = gridContainer;
    }

    public void DelayedStart()
    {
        StartCoroutine(DelayedStartCoroutine());
    }

    private IEnumerator DelayedStartCoroutine()
    {
        yield return null;
        StartPuzzle();
    }

    public void StartPuzzle()
    {
        ClearExisting();
        BuildGridVisual();
        InitializeGrid();

        
        int maxPairs = (gridCols * gridRows) / 2;
        numberOfPairs = Mathf.Clamp(numberOfPairs, 1, maxPairs);

        
        const int restartAttempts = 5;
        bool success = false;
        for (int attempt = 0; attempt < restartAttempts && !success; attempt++)
        {
            
            foreach (var n in nodes) if (n != null) Destroy(n.gameObject);
            nodes.Clear();
            for (int x = 0; x < gridCols; x++)
                for (int y = 0; y < gridRows; y++)
                    gridOccupied[x, y] = -1;
            established.Clear();

            success = TrySpawnAllPairs(numberOfPairs);
        }

    }

    public void ResetPuzzle()
    {
        ClearExisting();
    }

    private void InitializeGrid()
    {
        gridOccupied = new int[gridCols, gridRows];
        for (int x = 0; x < gridCols; x++)
            for (int y = 0; y < gridRows; y++)
                gridOccupied[x, y] = -1;
    }

    private void BuildGridVisual()
    {
        if (gridCellPrefab == null || gridContainer == null) return;

        
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
        {
            var child = gridContainer.GetChild(i);
            if (child.name.StartsWith("GridCell_"))
                Destroy(child.gameObject);
        }

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridCols; x++)
            {
                var go = Instantiate(gridCellPrefab, gridContainer, false);
                go.name = $"GridCell_{x}_{y}";
                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = GridToLocalPosition(new Vector2Int(x, y));
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);
            }
        }
    }

    
    private bool TrySpawnAllPairs(int pairs)
    {
        var available = new List<Vector2Int>();
        for (int y = 0; y < gridRows; y++)
            for (int x = 0; x < gridCols; x++)
                available.Add(new Vector2Int(x, y));

        int colorCount = Mathf.Max(1, nodeColors.Count);

        for (int p = 0; p < pairs; p++)
        {
            bool placed = false;
            Color col = nodeColors[p % colorCount];
            int pairId = p;

            
            const int pairTries = 400;
            for (int t = 0; t < pairTries && !placed; t++)
            {
                if (available.Count < 2) break;

                
                int i1 = Random.Range(0, available.Count);
                Vector2Int start = available[i1];
                available.RemoveAt(i1);

                int i2 = Random.Range(0, available.Count);
                Vector2Int goal = available[i2];
                
                available.Insert(i1, start);

               
                var path = FindPath(start, goal);
                if (path != null && path.Count > 0)
                {
                   
                    foreach (var c in path)
                    {
                        gridOccupied[c.x, c.y] = pairId;
                    }

                    
                    for (int k = available.Count - 1; k >= 0; k--)
                    {
                        var v = available[k];
                        if (gridOccupied[v.x, v.y] != -1)
                            available.RemoveAt(k);
                    }

                   
                    var go1 = Instantiate(nodePrefab, nodeContainer, false);
                    go1.name = $"Node_P{pairId}_A";
                    var nb1 = go1.GetComponent<NodeBehaviour>();
                    if (nb1 == null) nb1 = go1.AddComponent<NodeBehaviour>();
                    nb1.Initialize(this, pairId, GridToLocalPosition(start), start, col);
                    nodes.Add(nb1);

                    var go2 = Instantiate(nodePrefab, nodeContainer, false);
                    go2.name = $"Node_P{pairId}_B";
                    var nb2 = go2.GetComponent<NodeBehaviour>();
                    if (nb2 == null) nb2 = go2.AddComponent<NodeBehaviour>();
                    nb2.Initialize(this, pairId, GridToLocalPosition(goal), goal, col);
                    nodes.Add(nb2);

                    
                    placed = true;
                }
            }

            if (!placed)
            {
                
                return false;
            }
        }

        return true;
    }

    
    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (start == goal) return new List<Vector2Int> { start };

        var q = new Queue<Vector2Int>();
        q.Enqueue(start);

        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        var visited = new bool[gridCols, gridRows];
        visited[start.x, start.y] = true;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var n in GetNeighbors(cur))
            {
                if (visited[n.x, n.y]) continue;

                
                if (!(n == goal) && gridOccupied[n.x, n.y] != -1) continue;

                visited[n.x, n.y] = true;
                cameFrom[n] = cur;

                if (n == goal)
                {
                    
                    var path = new List<Vector2Int>();
                    var p = goal;
                    while (!(p == start))
                    {
                        path.Add(p);
                        p = cameFrom[p];
                    }
                    path.Add(start);
                    path.Reverse();
                    return path;
                }

                q.Enqueue(n);
            }
        }

        return null;
    }

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int v)
    {
        
        if (v.x > 0) yield return new Vector2Int(v.x - 1, v.y);
        if (v.x < gridCols - 1) yield return new Vector2Int(v.x + 1, v.y);
        if (v.y > 0) yield return new Vector2Int(v.x, v.y - 1);
        if (v.y < gridRows - 1) yield return new Vector2Int(v.x, v.y + 1);
    }

    private Vector2 GridToLocalPosition(Vector2Int gridPos)
    {
        float totalWidth = gridCols * cellSize.x;
        float totalHeight = gridRows * cellSize.y;
        Vector2 origin = new Vector2(-totalWidth / 2f + cellSize.x / 2f, -totalHeight / 2f + cellSize.y / 2f);
        return origin + new Vector2(gridPos.x * cellSize.x, gridPos.y * cellSize.y);
    }

    
    private Vector2Int ScreenPointToGrid(Vector2 screenPoint, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridContainer, screenPoint, cam, out Vector2 local);
        float totalWidth = gridCols * cellSize.x;
        float totalHeight = gridRows * cellSize.y;
        Vector2 origin = new Vector2(-totalWidth / 2f + cellSize.x / 2f, -totalHeight / 2f + cellSize.y / 2f);

        float localX = (local.x - origin.x) / cellSize.x;
        float localY = (local.y - origin.y) / cellSize.y;

        int gx = Mathf.RoundToInt(localX);
        int gy = Mathf.RoundToInt(localY);

        if (gx < 0 || gx >= gridCols || gy < 0 || gy >= gridRows)
            return new Vector2Int(-1, -1);

        return new Vector2Int(gx, gy);
    }

   
    internal void OnNodePointerDown(NodeBehaviour node, PointerEventData eventData)
    {
        if (IsPairAlreadyConnected(node.PairId)) return; 

        drawing = true;
        drawingPairId = node.PairId;
        drawingStartNode = node;
        tempCells.Clear();
        ClearTempVisuals();

       
        tempCells.Add(node.GridPosition);
        CreateTempSegmentAt(node.GridPosition, node.Color);
    }

    
    internal void OnNodeDrag(NodeBehaviour node, PointerEventData eventData)
    {
        if (!drawing || node.PairId != drawingPairId) return;

        Vector2Int gridPos = ScreenPointToGrid(eventData.position, eventData.pressEventCamera);
        if (gridPos.x < 0) return;

        
        if (tempCells.Count > 0 && tempCells[tempCells.Count - 1] == gridPos) return;

       
        if (allowBacktracking && tempCells.Count >= 2 && tempCells[tempCells.Count - 2] == gridPos)
        {
            
            RemoveLastTempSegment();
            return;
        }

        
        if (tempCells.Count > 0)
        {
            Vector2Int last = tempCells[tempCells.Count - 1];
            if (Mathf.Abs(last.x - gridPos.x) + Mathf.Abs(last.y - gridPos.y) != 1) return;
        }

       
        int occupied = gridOccupied[gridPos.x, gridPos.y];
        if (occupied != -1 && occupied != drawingPairId) return;

        
        tempCells.Add(gridPos);
        CreateTempSegmentAt(gridPos, node.Color);
    }

    
    internal void OnNodePointerUp(NodeBehaviour node, PointerEventData eventData)
    {
        if (!drawing || node.PairId != drawingPairId) return;

        
        NodeBehaviour hitNode = FindNodeUnderPointer(eventData);
        if (hitNode != null && hitNode.PairId == drawingPairId && hitNode != drawingStartNode)
        {
            
            Vector2Int last = tempCells[tempCells.Count - 1];
            if (Mathf.Abs(last.x - hitNode.GridPosition.x) + Mathf.Abs(last.y - hitNode.GridPosition.y) <= 1)
            {
                
                int pairId = drawingPairId;
                foreach (var c in tempCells)
                    gridOccupied[c.x, c.y] = pairId;

                established.Add(new EstablishedConnection { pairId = pairId, cells = new List<Vector2Int>(tempCells), visuals = new List<GameObject>(tempVisuals) });
                tempVisuals.Clear();
                tempCells.Clear();
                drawing = false;
                drawingStartNode = null;
                drawingPairId = -1;

                if (CheckWin())
                {
                    StartCoroutine(OnPuzzleSuccess());
                }

                return;
            }
        }

        
        ClearTempVisuals();
        tempCells.Clear();
        drawing = false;
        drawingStartNode = null;
        drawingPairId = -1;
    }

    private NodeBehaviour FindNodeUnderPointer(PointerEventData eventData)
    {
        foreach (var n in nodes)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(n.Rect, eventData.position, eventData.pressEventCamera))
                return n;
        }

        return null;
    }

    private void CreateTempSegmentAt(Vector2Int gridPos, Color color)
    {
        if (connectionSegmentPrefab == null || gridContainer == null) return;

        var go = Instantiate(connectionSegmentPrefab, gridContainer, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = GridToLocalPosition(gridPos);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

        var img = go.GetComponent<Image>();
        if (img != null) img.color = color;

        tempVisuals.Add(go);
    }

    private void RemoveLastTempSegment()
    {
        if (tempVisuals.Count == 0) return;
        var last = tempVisuals[tempVisuals.Count - 1];
        tempVisuals.RemoveAt(tempVisuals.Count - 1);
        Destroy(last);
        tempCells.RemoveAt(tempCells.Count - 1);
    }

    private void ClearTempVisuals()
    {
        for (int i = 0; i < tempVisuals.Count; i++)
            if (tempVisuals[i] != null) Destroy(tempVisuals[i]);
        tempVisuals.Clear();
    }

    private bool IsPairAlreadyConnected(int pairId)
    {
        foreach (var e in established)
            if (e.pairId == pairId) return true;
        return false;
    }

    private bool CheckWin()
    {
        
        int required = Mathf.Clamp(numberOfPairs, 1, numberOfPairs);
        int count = 0;
        foreach (var e in established) if (e != null) count++;
        return count >= required;
    }

    private IEnumerator OnPuzzleSuccess()
    {
        
        foreach (var c in established)
        {
            foreach (var v in c.visuals)
            {
                var img = v.GetComponent<Image>();
                if (img != null) img.color = Color.green;
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (HackingManager.Instance != null)
            HackingManager.Instance.NotifyHackSuccess();
    }

    private void ClearExisting()
    {
        
        foreach (var n in nodes) if (n != null) Destroy(n.gameObject);
        nodes.Clear();

        
        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                var c = gridContainer.GetChild(i);
                Destroy(c.gameObject);
            }
        }

        established.Clear();
        tempCells.Clear();
        ClearTempVisuals();
    }

    
    private class EstablishedConnection
    {
        public int pairId;
        public List<Vector2Int> cells;
        public List<GameObject> visuals;
    }

    
    public void SetGridSize(int rows, int cols)
    {
        gridRows = Mathf.Max(1, rows);
        gridCols = Mathf.Max(1, cols);
    }

    public void SetNumberOfPairs(int pairs)
    {
        numberOfPairs = Mathf.Max(1, pairs);
    }

    public void SetNodeColors(List<Color> colors)
    {
        if (colors == null || colors.Count == 0) return;
        nodeColors = new List<Color>(colors);
    }

    public void SetCellSize(Vector2 newCellSize)
    {
        cellSize = newCellSize;
    }
}
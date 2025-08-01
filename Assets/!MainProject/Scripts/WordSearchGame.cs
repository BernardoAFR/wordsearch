using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WordSearchGame : MonoBehaviour
{
    [Header("Configurações do Jogo")]
    [SerializeField] private int gridWidth = 11;
    [SerializeField] private int gridHeight = 15;
    [SerializeField] private GameObject letterPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private Transform wordListContainer;
    [SerializeField] private GameObject wordPrefabItem;

    [Header("Linhas de Seleção")]
    [SerializeField] private LineRenderer selectionLine;

    [Header("Cores")]
    [SerializeField] private Color selectionCellColor = Color.white;
    [SerializeField] private Color correctWordColor = new Color(0.2f, 0.41f, 0.23f);

    private Color originalCellColor;

    public event System.Action OnAllWordsFound;

    private string[] wordList = new[] { "RESPEITO", "ESCUTA", "AMIZADE", "EMPATIA", "DIALOGO", "APOIO", "SORRISO", "DIVERSAO", "REGRAS", "PAZ" };

    private char[,] grid;
    private Dictionary<string, bool> foundWords;
    private List<Vector2Int> currentSelection;
    private bool isSelecting;

    private GameObject[,] letterObjects;
    private Vector3[,] letterPositions;
    private Color[,] originalColors;
    private Dictionary<string, GameObject> wordItems;

    void Start() => InitializeGame();
    void Update() => HandleInput();

    private void InitializeGame()
    {
        if (letterPrefab.TryGetComponent(out Image img)) originalCellColor = img.color;
        else originalCellColor = Color.white;

        grid = new char[gridWidth, gridHeight];
        letterObjects = new GameObject[gridWidth, gridHeight];
        letterPositions = new Vector3[gridWidth, gridHeight];
        originalColors = new Color[gridWidth, gridHeight];
        currentSelection = new List<Vector2Int>();
        foundWords = wordList.ToDictionary(w => w, w => false);
        wordItems = new Dictionary<string, GameObject>();

        selectionLine.positionCount = 0;

        GenerateWordSearch();
        CreateWordList();
    }

    private void GenerateWordSearch()
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                grid[x, y] = ' ';

        var directions = new[] { new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(1,-1) };

        foreach (var word in wordList.OrderByDescending(w => w.Length))
        {
            bool placed = false; int attempts = 0;
            int len = word.Length - 1;
            while (!placed && attempts++ < 500)
            {
                var dir = directions[Random.Range(0, directions.Length)];
                int maxX = gridWidth - Mathf.Abs(dir.x * len) - 1;
                int maxY = gridHeight - Mathf.Abs(dir.y * len) - 1;
                int sx = Random.Range(0, maxX + 1);
                int sy = Random.Range(0, maxY + 1);
                if (CanPlace(word, sx, sy, dir)) { Place(word, sx, sy, dir); placed = true; }
            }
            if (!placed) Debug.LogWarning($"Não foi possível colocar a palavra: {word}");
        }

        FillEmptySpaces();
        CreateLetterObjects();
    }

    private bool CanPlace(string w, int x0, int y0, Vector2Int d)
    {
        for (int i = 0; i < w.Length; i++)
        {
            int x = x0 + d.x * i, y = y0 + d.y * i;
            if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return false;
            if (grid[x,y] != ' ' && grid[x,y] != w[i]) return false;
        }
        return true;
    }

    private void Place(string w, int x0, int y0, Vector2Int d)
    {
        for (int i = 0; i < w.Length; i++)
            grid[x0 + d.x * i, y0 + d.y * i] = w[i];
    }

    private void FillEmptySpaces()
    {
        const string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int x=0; x<gridWidth; x++)
            for (int y=0; y<gridHeight; y++)
                if (grid[x,y] == ' ') grid[x,y] = abc[Random.Range(0,abc.Length)];
    }

    private void CreateLetterObjects()
    {
        RectTransform rtGrid = gridContainer.GetComponent<RectTransform>();
        float cellSize = rtGrid.rect.width / gridWidth;
        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
            {
                var obj = Instantiate(letterPrefab, gridContainer);
                var rt = obj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x * cellSize + cellSize/2, -y * cellSize - cellSize/2);
                rt.sizeDelta = Vector2.one * cellSize * 0.9f;

                obj.GetComponentInChildren<TextMeshProUGUI>().text = grid[x,y].ToString();
                var img = obj.GetComponent<Image>();
                originalColors[x,y] = img.color;

                letterObjects[x,y] = obj;
                letterPositions[x,y] = rt.position;
            }
    }

    private void CreateWordList()
    {
        foreach (var w in wordList)
        {
            var item = Instantiate(wordPrefabItem, wordListContainer);
            item.GetComponentInChildren<TextMeshProUGUI>().text = w;
            wordItems[w] = item;
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) StartSel();
        else if (Input.GetMouseButton(0) && isSelecting) ContinueSel();
        else if (Input.GetMouseButtonUp(0) && isSelecting) EndSel();
    }

    private void StartSel()
    {
        var c = GetCellUnderMouse(); if (!c.HasValue) return;
        isSelecting = true; currentSelection.Clear(); AddCell(c.Value);
    }

        private void ContinueSel()
    {
        var c = GetCellUnderMouse();
        if (!c.HasValue) return;
        Vector2Int pos = c.Value;
        if (currentSelection.Contains(pos)) return;

        Vector2Int last = currentSelection.Last();
        Vector2Int dir = pos - last;
        dir.x = dir.x != 0 ? dir.x / Mathf.Abs(dir.x) : 0;
        dir.y = dir.y != 0 ? dir.y / Mathf.Abs(dir.y) : 0;

        if (currentSelection.Count > 1)
        {
            Vector2Int prev = last - currentSelection[currentSelection.Count - 2];
            prev.x = prev.x != 0 ? prev.x / Mathf.Abs(prev.x) : 0;
            prev.y = prev.y != 0 ? prev.y / Mathf.Abs(prev.y) : 0;
            if (dir != prev) return;
        }

        if (IsAdjacent(last, pos))
        {
            AddCell(pos);
        }
    }

    private void AddCell(Vector2Int c)
    {
        currentSelection.Add(c);
        var pts = currentSelection.Select(p => letterPositions[p.x,p.y]).ToArray();
        selectionLine.positionCount = pts.Length; selectionLine.SetPositions(pts);
        HighlightCell(c,true);
    }

    private void EndSel()
    {
        bool correct = false;
        if (currentSelection.Count >= 3)
        {
            var s = GetWord(currentSelection);
            var sr = new string(s.Reverse().ToArray());
            if (foundWords.ContainsKey(s) && !foundWords[s]) { foundWords[s] = true; correct = true; MarkWord(s); }
            else if (foundWords.ContainsKey(sr) && !foundWords[sr]) { foundWords[sr] = true; correct = true; MarkWord(sr); }
        }
        var clr = correct ? correctWordColor : originalCellColor;
        foreach (var c in currentSelection)
            if (letterObjects[c.x,c.y].GetComponent<Image>().color != correctWordColor)
                letterObjects[c.x,c.y].GetComponent<Image>().color = clr;

        currentSelection.Clear(); isSelecting = false; selectionLine.positionCount = 0;
        if (foundWords.All(kv => kv.Value)) OnAllWordsFound?.Invoke();
    }

    private void MarkWord(string w)
    {
        wordItems[w].GetComponent<Image>().color = correctWordColor;
        foreach (var p in currentSelection)
            letterObjects[p.x,p.y].GetComponent<Image>().color = correctWordColor;
    }

    private Vector2Int? GetCellUnderMouse()
    {
        var mp = Input.mousePosition;
        for (int x=0;x<gridWidth;x++) for(int y=0;y<gridHeight;y++)
        {
            var rt = letterObjects[x,y]?.GetComponent<RectTransform>();
            if (rt!=null && RectTransformUtility.RectangleContainsScreenPoint(rt,mp))
                return new Vector2Int(x,y);
        }
        return null;
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x-b.x), dy = Mathf.Abs(a.y-b.y);
        return (dx<=1 && dy<=1) && !(dx==0 && dy==0);
    }

    private string GetWord(List<Vector2Int> sel)
    {
        return string.Concat(sel.Select(p => grid[p.x,p.y]));
    }

    private void HighlightCell(Vector2Int c, bool on)
    {
        var img = letterObjects[c.x,c.y].GetComponent<Image>();
        if (img.color == correctWordColor) return;
        img.color = on ? selectionCellColor : originalColors[c.x,c.y];
    }
}


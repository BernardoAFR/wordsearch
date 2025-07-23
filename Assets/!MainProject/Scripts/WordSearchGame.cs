using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WordSearchGame : MonoBehaviour
{
    [Header("Configurações do Jogo")]
    [SerializeField] private int gridSize = 15;
    [SerializeField] private GameObject letterPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private Transform wordListContainer;
    [SerializeField] private GameObject wordPrefabItem;

    [Header("Linhas de Seleção")]
    [SerializeField] private LineRenderer selectionLine;

    public event System.Action OnAllWordsFound;

    private string[] wordList = new string[] {
        "RESPEITO",
        "ESCUTA",
        "AMIZADE",
        "EMPATIA",
        "DIÁLOGO",
        "COOPERAÇÃO",
        "SORRISO",
        "BRINCADEIRA",
        "REGRAS",
        "PAZ"
    };

    private char[,] grid;
    private Dictionary<string, bool> foundWords;
    private List<Vector2Int> currentSelection;
    private bool isSelecting;

    private GameObject[,] letterObjects;
    private Vector2[,] letterPositions;
    private Dictionary<string, GameObject> wordItems;

    private readonly Color correctWordColor = new Color(0.2f, 0.41f, 0.23f); // #33693A

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        HandleInput();
    }

    private void InitializeGame()
    {
        grid = new char[gridSize, gridSize];
        letterObjects = new GameObject[gridSize, gridSize];
        letterPositions = new Vector2[gridSize, gridSize];
        currentSelection = new List<Vector2Int>();
        foundWords = new Dictionary<string, bool>();
        wordItems = new Dictionary<string, GameObject>();

        selectionLine.positionCount = 0;

        foreach (string word in wordList)
        {
            foundWords.Add(word, false);
        }

        GenerateWordSearch();
        CreateWordList();
    }

    private void GenerateWordSearch()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = ' ';
            }
        }

        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1)
        };

        List<string> shuffledWords = wordList.OrderBy(x => Random.value).ToList();

        foreach (string word in shuffledWords)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < 100)
            {
                Vector2Int direction = directions[Random.Range(0, directions.Count)];

                int maxX = gridSize - (direction.x * word.Length);
                int maxY = gridSize - (direction.y * word.Length);
                maxX = Mathf.Max(1, maxX);
                maxY = Mathf.Max(1, maxY);

                int startX = Random.Range(0, maxX);
                int startY = Random.Range(0, maxY);

                if (CanPlaceWord(word, startX, startY, direction))
                {
                    PlaceWord(word, startX, startY, direction);
                    placed = true;
                }

                attempts++;
            }

            if (!placed)
            {
                Debug.LogWarning("Não foi possível colocar a palavra: " + word);
            }
        }

        FillEmptySpaces();
        CreateLetterObjects();
    }

    private bool CanPlaceWord(string word, int startX, int startY, Vector2Int direction)
    {
        if (startX < 0 || startY < 0 || startX >= gridSize || startY >= gridSize)
            return false;

        for (int i = 0; i < word.Length; i++)
        {
            int x = startX + (direction.x * i);
            int y = startY + (direction.y * i);

            if (x < 0 || y < 0 || x >= gridSize || y >= gridSize)
                return false;

            if (grid[x, y] != ' ' && grid[x, y] != word[i])
                return false;
        }
        return true;
    }

    private void PlaceWord(string word, int startX, int startY, Vector2Int direction)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int x = startX + (direction.x * i);
            int y = startY + (direction.y * i);
            grid[x, y] = word[i];
        }
    }

    private void FillEmptySpaces()
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y] == ' ')
                {
                    grid[x, y] = alphabet[Random.Range(0, alphabet.Length)];
                }
            }
        }
    }

    private void CreateLetterObjects()
    {
        float cellSize = gridContainer.GetComponent<RectTransform>().rect.width / gridSize;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                GameObject letterObj = Instantiate(letterPrefab, gridContainer);
                RectTransform rectTransform = letterObj.GetComponent<RectTransform>();

                rectTransform.anchoredPosition = new Vector2(x * cellSize + cellSize / 2, -y * cellSize - cellSize / 2);
                rectTransform.sizeDelta = new Vector2(cellSize * 0.9f, cellSize * 0.9f);

                letterObj.GetComponentInChildren<TextMeshProUGUI>().text = grid[x, y].ToString();

                letterObjects[x, y] = letterObj;
                letterPositions[x, y] = rectTransform.position;
            }
        }
    }

    private void CreateWordList()
    {
        foreach (string word in wordList)
        {
            GameObject wordItem = Instantiate(wordPrefabItem, wordListContainer);
            wordItem.GetComponentInChildren<TextMeshProUGUI>().text = word;
            wordItems.Add(word, wordItem);
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) StartSelection();
        else if (Input.GetMouseButton(0) && isSelecting) ContinueSelection();
        else if (Input.GetMouseButtonUp(0) && isSelecting) FinishSelection();
    }

    private void StartSelection()
    {
        Vector2Int? cellPosition = GetCellUnderMouse();
        if (cellPosition.HasValue)
        {
            isSelecting = true;
            currentSelection.Clear();
            currentSelection.Add(cellPosition.Value);

            selectionLine.positionCount = 1;
            selectionLine.SetPosition(0, letterPositions[cellPosition.Value.x, cellPosition.Value.y]);

            HighlightCell(cellPosition.Value, true);
        }
    }

    private void ContinueSelection()
    {
        Vector2Int? cellPosition = GetCellUnderMouse();
        if (cellPosition.HasValue && !currentSelection.Contains(cellPosition.Value))
        {
            Vector2Int lastCell = currentSelection[currentSelection.Count - 1];
            Vector2Int direction = cellPosition.Value - lastCell;

            direction.x = direction.x != 0 ? direction.x / Mathf.Abs(direction.x) : 0;
            direction.y = direction.y != 0 ? direction.y / Mathf.Abs(direction.y) : 0;

            if (currentSelection.Count > 1)
            {
                Vector2Int prevDirection = lastCell - currentSelection[currentSelection.Count - 2];
                prevDirection.x = prevDirection.x != 0 ? prevDirection.x / Mathf.Abs(prevDirection.x) : 0;
                prevDirection.y = prevDirection.y != 0 ? prevDirection.y / Mathf.Abs(prevDirection.y) : 0;

                if (direction != prevDirection) return;
            }

            if (IsAdjacent(lastCell, cellPosition.Value))
            {
                currentSelection.Add(cellPosition.Value);

                selectionLine.positionCount = currentSelection.Count;
                selectionLine.SetPosition(currentSelection.Count - 1, letterPositions[cellPosition.Value.x, cellPosition.Value.y]);

                HighlightCell(cellPosition.Value, true);
            }
        }
    }

    private void FinishSelection()
    {
        if (currentSelection.Count >= 3)
        {
            string selectedWord = GetSelectedWord();

            if (foundWords.ContainsKey(selectedWord) && !foundWords[selectedWord])
            {
                foundWords[selectedWord] = true;

                if (wordItems.ContainsKey(selectedWord))
                {
                    wordItems[selectedWord].GetComponent<Image>().color = correctWordColor;
                }

                foreach (Vector2Int cell in currentSelection)
                {
                    letterObjects[cell.x, cell.y].GetComponent<Image>().color = correctWordColor;
                }

                if (foundWords.All(w => w.Value))
                {
                    Debug.Log("Parabéns! Você encontrou todas as palavras!");
                    OnAllWordsFound?.Invoke();
                }
            }
            else
            {
                foreach (Vector2Int cell in currentSelection)
                {
                    HighlightCell(cell, false);
                }
            }
        }
        else
        {
            foreach (Vector2Int cell in currentSelection)
            {
                HighlightCell(cell, false);
            }
        }

        currentSelection.Clear();
        isSelecting = false;
        selectionLine.positionCount = 0;
    }

    private Vector2Int? GetCellUnderMouse()
    {
        if (letterObjects == null || gridSize <= 0) return null;

        Vector2 mousePosition = Input.mousePosition;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (letterObjects[x, y] == null || letterObjects[x, y].GetComponent<RectTransform>() == null)
                    continue;

                if (RectTransformUtility.RectangleContainsScreenPoint(
                    letterObjects[x, y].GetComponent<RectTransform>(), mousePosition))
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return null;
    }

    private bool IsAdjacent(Vector2Int cell1, Vector2Int cell2)
    {
        int dx = Mathf.Abs(cell1.x - cell2.x);
        int dy = Mathf.Abs(cell1.y - cell2.y);
        return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
    }

    private string GetSelectedWord()
    {
        string word = "";
        foreach (Vector2Int cell in currentSelection)
        {
            word += grid[cell.x, cell.y];
        }
        return word;
    }

    private void HighlightCell(Vector2Int cell, bool highlight)
    {
        if (letterObjects[cell.x, cell.y].GetComponent<Image>().color == correctWordColor) return;

        Color color = highlight ? Color.white: Color.white;
        letterObjects[cell.x, cell.y].GetComponent<Image>().color = color;
    }
}
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
    
    // Lista de palavras para o caça-palavras (tema infantil)
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

    
    // Variáveis para controle do jogo
    private char[,] grid;
    private Dictionary<string, bool> foundWords;
    private List<Vector2Int> currentSelection;
    private bool isSelecting;
    
    // Armazena as posições das letras na grade
    private GameObject[,] letterObjects;
    private Vector2[,] letterPositions;
    
    // Armazena os itens da palavra na UI
    private Dictionary<string, GameObject> wordItems;

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
        // Inicializa estruturas de dados
        grid = new char[gridSize, gridSize];
        letterObjects = new GameObject[gridSize, gridSize];
        letterPositions = new Vector2[gridSize, gridSize];
        currentSelection = new List<Vector2Int>();
        foundWords = new Dictionary<string, bool>();
        wordItems = new Dictionary<string, GameObject>();
        
        // Configura a linha de seleção
        selectionLine.positionCount = 0;
        
        // Inicializa a lista de palavras como não encontradas
        foreach (string word in wordList)
        {
            foundWords.Add(word, false);
        }
        
        // Gera o caça-palavras
        GenerateWordSearch();
        
        // Cria a UI para mostrar a lista de palavras
        CreateWordList();
    }
    
    private void GenerateWordSearch()
    {
        // Inicializa a grade com espaços vazios
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = ' ';
            }
        }
        
        // Lista de direções possíveis (horizontal, vertical, diagonal)
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),   // Direita
            new Vector2Int(0, 1),   // Baixo
            new Vector2Int(1, 1),   // Diagonal direita-baixo
            new Vector2Int(1, -1)   // Diagonal direita-cima
        };
        
        // Embaralha a lista de palavras para posicionamento aleatório
        List<string> shuffledWords = wordList.OrderBy(x => Random.value).ToList();
        
        // Tenta posicionar cada palavra na grade
        foreach (string word in shuffledWords)
        {
            bool placed = false;
            int attempts = 0;
            
            while (!placed && attempts < 100)
            {
                // Escolhe uma direção aleatória
                Vector2Int direction = directions[Random.Range(0, directions.Count)];
                
                // Calcula o ponto de origem máximo para a palavra não sair da grade
                int maxX = gridSize - (direction.x * word.Length);
                int maxY = gridSize - (direction.y * word.Length);
                maxX = Mathf.Max(1, maxX);
                maxY = Mathf.Max(1, maxY);
                
                // Posição inicial aleatória
                int startX = Random.Range(0, maxX);
                int startY = Random.Range(0, maxY);
                
                // Verifica se a palavra pode ser colocada nesta posição
                if (CanPlaceWord(word, startX, startY, direction))
                {
                    PlaceWord(word, startX, startY, direction);
                    placed = true;
                }
                
                attempts++;
            }
            
            // Se não conseguiu colocar após várias tentativas, pode ignorar essa palavra
            if (!placed)
            {
                Debug.LogWarning("Não foi possível colocar a palavra: " + word);
            }
        }
        
        // Preenche os espaços vazios com letras aleatórias
        FillEmptySpaces();
        
        // Cria os objetos de letra na UI
        CreateLetterObjects();
    }
    
    private bool CanPlaceWord(string word, int startX, int startY, Vector2Int direction)
{
    // Verifica se a palavra cabe na posição inicial
    if (startX < 0 || startY < 0 || startX >= gridSize || startY >= gridSize)
        return false;

    // Verifica cada letra da palavra
    for (int i = 0; i < word.Length; i++)
    {
        int x = startX + (direction.x * i);
        int y = startY + (direction.y * i);
        
        // Se a célula estiver fora dos limites, não pode colocar
        if (x < 0 || y < 0 || x >= gridSize || y >= gridSize)
            return false;
        
        // Se a célula já contém uma letra diferente
        if (grid[x, y] != ' ' && grid[x, y] != word[i])
            return false;
    }
    return true;
}
    
    private void PlaceWord(string word, int startX, int startY, Vector2Int direction)
    {
        // Coloca a palavra na grade na posição e direção especificadas
        for (int i = 0; i < word.Length; i++)
        {
            int x = startX + (direction.x * i);
            int y = startY + (direction.y * i);
            grid[x, y] = word[i];
        }
    }
    
    private void FillEmptySpaces()
    {
        // Preenche espaços vazios com letras aleatórias
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
        // Calcula o tamanho de cada célula baseado no tamanho do container
        float cellSize = gridContainer.GetComponent<RectTransform>().rect.width / gridSize;
        
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Cria um objeto para cada letra
                GameObject letterObj = Instantiate(letterPrefab, gridContainer);
                RectTransform rectTransform = letterObj.GetComponent<RectTransform>();
                
                // Posiciona a letra na grade UI
                rectTransform.anchoredPosition = new Vector2(x * cellSize + cellSize/2, -y * cellSize - cellSize/2);
                rectTransform.sizeDelta = new Vector2(cellSize * 0.9f, cellSize * 0.9f);
                
                // Define o texto da letra
                letterObj.GetComponentInChildren<TextMeshProUGUI>().text = grid[x, y].ToString();
                
                // Armazena o objeto e sua posição para uso posterior
                letterObjects[x, y] = letterObj;
                letterPositions[x, y] = rectTransform.position;
            }
        }
    }
    
    private void CreateWordList()
    {
        // Cria a lista de palavras na UI
        foreach (string word in wordList)
        {
            GameObject wordItem = Instantiate(wordPrefabItem, wordListContainer);
            wordItem.GetComponentInChildren<TextMeshProUGUI>().text = word;
            
            // Armazena referência ao item para marcar quando for encontrado
            wordItems.Add(word, wordItem);
        }
    }
    
    private void HandleInput()
    {
        // Controle de seleção com o mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Inicia seleção
            StartSelection();
        }
        else if (Input.GetMouseButton(0) && isSelecting)
        {
            // Continua seleção
            ContinueSelection();
        }
        else if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            // Finaliza seleção
            FinishSelection();
        }
    }
    
    private void StartSelection()
    {
        // Verifica se o clique foi em alguma letra
        Vector2Int? cellPosition = GetCellUnderMouse();
        if (cellPosition.HasValue)
        {
            isSelecting = true;
            currentSelection.Clear();
            currentSelection.Add(cellPosition.Value);
            
            // Configura linha de seleção
            selectionLine.positionCount = 1;
            selectionLine.SetPosition(0, letterPositions[cellPosition.Value.x, cellPosition.Value.y]);
            
            // Destaca a letra selecionada
            HighlightCell(cellPosition.Value, true);
        }
    }
    
    private void ContinueSelection()
    {
        // Atualiza a seleção à medida que o mouse se move
        Vector2Int? cellPosition = GetCellUnderMouse();
        if (cellPosition.HasValue && !currentSelection.Contains(cellPosition.Value))
        {
            // Verifica se a célula é adjacente à última célula selecionada
            Vector2Int lastCell = currentSelection[currentSelection.Count - 1];
            Vector2Int direction = new Vector2Int(
                cellPosition.Value.x - lastCell.x,
                cellPosition.Value.y - lastCell.y
            );
            
            // Normaliza a direção (para obter somente -1, 0 ou 1)
            if (direction.x != 0) direction.x = direction.x / Mathf.Abs(direction.x);
            if (direction.y != 0) direction.y = direction.y / Mathf.Abs(direction.y);
            
            // Só permite seleção em linha reta (mesma direção)
            if (currentSelection.Count > 1)
            {
                Vector2Int prevDirection = new Vector2Int(
                    lastCell.x - currentSelection[currentSelection.Count - 2].x,
                    lastCell.y - currentSelection[currentSelection.Count - 2].y
                );
                
                // Normaliza a direção anterior
                if (prevDirection.x != 0) prevDirection.x = prevDirection.x / Mathf.Abs(prevDirection.x);
                if (prevDirection.y != 0) prevDirection.y = prevDirection.y / Mathf.Abs(prevDirection.y);
                
                // Se a direção mudou, não adiciona essa célula
                if (direction != prevDirection)
                {
                    return;
                }
            }
            
            // Se está adjacente na direção correta, adiciona à seleção
            if (IsAdjacent(lastCell, cellPosition.Value))
            {
                currentSelection.Add(cellPosition.Value);
                
                // Atualiza a linha de seleção
                selectionLine.positionCount = currentSelection.Count;
                selectionLine.SetPosition(currentSelection.Count - 1, 
                    letterPositions[cellPosition.Value.x, cellPosition.Value.y]);
                
                // Destaca a letra selecionada
                HighlightCell(cellPosition.Value, true);
            }
        }
    }
    
    private void FinishSelection()
    {
        if (currentSelection.Count >= 3)
        {
            // Obtém a palavra selecionada
            string selectedWord = GetSelectedWord();
            
            // Verifica se é uma palavra válida
            if (foundWords.ContainsKey(selectedWord) && !foundWords[selectedWord])
            {
                // Marca a palavra como encontrada
                foundWords[selectedWord] = true;
                
                // Destaca a palavra na lista
                if (wordItems.ContainsKey(selectedWord))
                {
                    wordItems[selectedWord].GetComponent<Image>().color = Color.green;
                }
                
                // Mantém o destaque nas letras selecionadas
                foreach (Vector2Int cell in currentSelection)
                {
                    letterObjects[cell.x, cell.y].GetComponent<Image>().color = Color.green;
                }
                
                // Verifica se todas as palavras foram encontradas
                if (foundWords.All(w => w.Value))
                {
                    Debug.Log("Parabéns! Você encontrou todas as palavras!");
                    OnAllWordsFound?.Invoke();
                }
            }
            else
            {
                // Desmarca a seleção incorreta
                foreach (Vector2Int cell in currentSelection)
                {
                    HighlightCell(cell, false);
                }
            }
        }
        else
        {
            // Desmarca a seleção curta
            foreach (Vector2Int cell in currentSelection)
            {
                HighlightCell(cell, false);
            }
        }
        
        // Limpa a seleção
        currentSelection.Clear();
        isSelecting = false;
        selectionLine.positionCount = 0;
    }
    
   private Vector2Int? GetCellUnderMouse()
{
    // Verificação de segurança (se o array de letras não foi inicializado)
    if (letterObjects == null || gridSize <= 0)
    {
        Debug.LogWarning("letterObjects não foi inicializado ou gridSize é inválido!");
        return null;
    }

    Vector2 mousePosition = Input.mousePosition;
    
    // Percorre todas as células da grade
    for (int x = 0; x < gridSize; x++)
    {
        for (int y = 0; y < gridSize; y++)
        {
            // Verifica se o objeto da célula existe
            if (letterObjects[x, y] == null || letterObjects[x, y].GetComponent<RectTransform>() == null)
            {
                Debug.LogWarning($"Célula ({x},{y}) está nula ou sem RectTransform!");
                continue;
            }

            // Verifica se o mouse está sobre a célula
            if (RectTransformUtility.RectangleContainsScreenPoint(
                letterObjects[x, y].GetComponent<RectTransform>(), 
                mousePosition))
            {
                return new Vector2Int(x, y);
            }
        }
    }
    
    return null; // Retorna null se nenhuma célula foi clicada
}
    
    private bool IsAdjacent(Vector2Int cell1, Vector2Int cell2)
    {
        // Verifica se as células são adjacentes (horizontal, vertical ou diagonal)
        int dx = Mathf.Abs(cell1.x - cell2.x);
        int dy = Mathf.Abs(cell1.y - cell2.y);
        
        return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
    }
    
    private string GetSelectedWord()
    {
        // Constrói a palavra a partir das células selecionadas
        string word = "";
        foreach (Vector2Int cell in currentSelection)
        {
            word += grid[cell.x, cell.y];
        }
        
        return word;
    }
    
    private void HighlightCell(Vector2Int cell, bool highlight)
    {
        if (letterObjects[cell.x, cell.y].GetComponent<Image>().color == Color.green)
        return;
        
        Color color = highlight ? Color.yellow : Color.white;
        letterObjects[cell.x, cell.y].GetComponent<Image>().color = color;
    }
}
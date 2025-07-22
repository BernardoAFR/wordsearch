using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private WordSearchGame wordSearchGame;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    
    [Header("Configurações")]
    [SerializeField] private float timeLimit = 300f; // 5 minutos
    [SerializeField] private bool useTimer = true;
    [SerializeField] private string nextLevelName;
    
    
    private float currentTime;
    private bool gameOver = false;
    
    void Start()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
            
        currentTime = timeLimit;
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        
        // Se tivermos wordSearchGame, registre um evento para saber quando todas as palavras foram encontradas
        if (wordSearchGame != null)
        {
            wordSearchGame.OnAllWordsFound += HandleGameComplete;
        }
    }
    
    void Update()
    {
        if (gameOver || !useTimer) return;
        
        // Atualiza o timer
        currentTime -= Time.deltaTime;
        
        if (currentTime <= 0)
        {
            currentTime = 0;
            HandleTimeOut();
        }
        
        // Atualiza o texto do timer (formato MM:SS)
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    private void HandleGameComplete()
    {
        gameOver = true;
        
        // Mostra o painel de vitória
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            // Se não tiver próximo nível, desativa o botão
            if (string.IsNullOrEmpty(nextLevelName) && nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(false);
            }
        }
    }
    
    private void HandleTimeOut()
    {
        gameOver = true;
        // Aqui você pode adicionar lógica para quando o tempo acabar
        // (mostrar mensagem, desabilitar interação, etc.)
    }
    
    public void RestartGame()
    {
        // Recarrega a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadNextLevel()
    {
        // Carrega o próximo nível
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }
    
    public void ReturnToMainMenu()
    {
        // Carrega o menu principal (ajuste o nome da cena conforme necessário)
        SceneManager.LoadScene("MainMenu");
    }
}
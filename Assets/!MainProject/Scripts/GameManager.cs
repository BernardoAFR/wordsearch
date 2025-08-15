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
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button restartButton;
    [SerializeField]private ParticleSystem confettiSystem;

    //[SerializeField] private Button nextLevelButton;
    
    [Header("Configurações")]
    [SerializeField] private float timeLimit = 300f; // 5 minutos
    [SerializeField] private bool useTimer = true;
    [SerializeField] private string nextLevelName;
    
    
    private float currentTime;
    private bool gameOver = false;

   void Start()
    {
        // 1) Desativa o painel se estiver atribuído
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        // 2) Configura o sistema de partículas (confetti)
        if (victoryPanel == null)
        {
            Debug.LogError("VictoryPanel não foi atribuído no Inspector!");
        }
        else
        {
            var emitter = victoryPanel.transform.Find("ConfettiEmitter");
            if (emitter == null)
                Debug.LogError("Não achei o filho 'ConfettiEmitter' em VictoryPanel");
            else
            {
                confettiSystem = emitter.GetComponent<ParticleSystem>();
                if (confettiSystem == null)
                    Debug.LogError("'ConfettiEmitter' não tem ParticleSystem!");
            }
        }

        // 3) Inicializa tempo e botão de restart
        currentTime = timeLimit;
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                Debug.Log("Botão de reinício clicado");
                RestartGame();
            });
        }

        // 4) Assina o evento do wordSearchGame
        if (wordSearchGame != null)
        {
            wordSearchGame.OnAllWordsFound += HandleGameComplete;
        }
        else
        {
            Debug.LogError("wordSearchGame não foi atribuído no Inspector!");
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
            confettiSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiSystem.Play();
            
            // Se não tiver próximo nível, desativa o botão
            /*
            if (string.IsNullOrEmpty(nextLevelName) && nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(false);
            }
            */

        }
    }
    
    private void HandleTimeOut()
    {
        if (gameOverPanel != null) 
            gameOverPanel.SetActive(true);

        // 2) Revela todas as palavras
        if (wordSearchGame != null)
            wordSearchGame.RevealAllWords();
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
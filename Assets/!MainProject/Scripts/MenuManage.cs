using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManage : MonoBehaviour
{
    public string nomeCenaDoJogo;
    public string nomeDaCena;
    public void Jogar()
    {
        SceneManager.LoadScene(nomeCenaDoJogo);
    }
    
    public void Sair()
    {
        Debug.LogWarning("Quit!");
        Application.Quit();
    }
    
    public void changeScene(string nomeDaCena)
    {
         SceneManager.LoadScene(nomeDaCena);
    }
}

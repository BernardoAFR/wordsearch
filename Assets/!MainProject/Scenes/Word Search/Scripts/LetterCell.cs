using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterCell : MonoBehaviour
{
    // Este script pode ser anexado ao prefab da letra para funcionalidades adicionais
    // como animações quando selecionado, efeitos sonoros, etc.
    
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.white;

    
    private Image backgroundImage;
    
    void Awake()
    {
        backgroundImage = GetComponent<Image>();
    }
    
    public void OnPointerEnter()
    {
        // Destaca a letra quando o mouse passa por cima
        if (backgroundImage.color == normalColor)
        {
            backgroundImage.color = hoverColor;
        }
    }
    
    public void OnPointerExit()
    {
        // Retorna à cor normal quando o mouse sai
        if (backgroundImage.color == hoverColor)
        {
            backgroundImage.color = normalColor;
        }
    }
}
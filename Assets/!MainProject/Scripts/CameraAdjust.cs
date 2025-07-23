using UnityEngine;

public class CameraAdjust : MonoBehaviour
{
    public float targetWidth = 20f; // Largura fixa que você quer no mundo

    void Start()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float orthographicSize = targetWidth / (2f * screenAspect);
        Camera.main.orthographicSize = orthographicSize;

        // Centraliza a câmera no centro do mundo
        Camera.main.transform.position = new Vector3(0f, 0f, -10f);
    }
}
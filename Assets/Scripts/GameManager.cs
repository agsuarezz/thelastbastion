using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image imagenTorre;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void crearTorre(GameObject boton)
    {
        boton.GetComponent<Image>().sprite = imagenTorre.sprite;
        // 1. Pillamos los RectTransform de ambos
        RectTransform rectTorre = imagenTorre.GetComponent<RectTransform>();
        RectTransform rectBoton = boton.GetComponent<RectTransform>();

        // 2. Igualamos el ancho y el alto directamente
        rectBoton.sizeDelta = new Vector2(rectTorre.rect.width, rectTorre.rect.height);
        boton.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Sprite imagenTorre;
    public Image imagenBoton;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void crearTorre()
    {
        imagenBoton.GetComponent<Image>().sprite = imagenTorre.GetComponent<Image>().sprite;
    }
}

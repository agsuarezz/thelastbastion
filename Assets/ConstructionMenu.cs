using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionMenu : MonoBehaviour
{
    public GameObject menuTowerSelect;
    public GameObject prefabTowerMedian;
    public Tilemap tilemap;

    // Da permiso para que coloque la torre
    bool isPlacing = false;
    [HideInInspector] public int flagTypeTower = -1;

    private void Update()
    {
        // 1. Entramos solo si estamos en modo colocación
        if (isPlacing)
        {
            // 2. Si pierde dinero a mitad de construcción, abortamos.
            if (GameManager.countMoney < costTower(flagTypeTower))
            {
                tilemap.gameObject.SetActive(false);
                isPlacing = false;
                Debug.Log("Construcción abortada: Te quedaste sin oro.");
                return;
            }

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float x = Mathf.Floor(mousePosition.x) + 0.5f;
            float y = Mathf.Floor(mousePosition.y) + 0.5f;
            Vector2 posicionExacta = new Vector2(x, y);

            // 3. CLIC IZQUIERDO: Intentar plantar
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D hit = Physics2D.OverlapCircle(posicionExacta, 0.2f);

                // Si tocamos algo, Y ese algo es Torre, Camino o Enemigo...
                if (hit != null && (hit.CompareTag("tower") || hit.CompareTag("Path") || hit.CompareTag("Enemy")))
                {
                    Debug.Log("No puedes construir ahí. Obstáculo: " + hit.gameObject.name);
                    return; // Abortamos la lectura del clic
                }

                // Si llegamos aquí, es que la casilla está libre. ¡Construimos!
                PlantTowerOnMap(posicionExacta);
            }

            // 4. CLIC DERECHO: Cancelar construcción si el jugador se arrepiente
            if (Input.GetMouseButtonDown(1))
            {
                tilemap.gameObject.SetActive(false);
                isPlacing = false;
            }
        }
    }

    public void contructionMenu()
    {
        menuTowerSelect.SetActive(true);
    }

    public void BuyTorreMedian()
    {
        flagTypeTower = 0;
        int costTowerToInt = costTower(flagTypeTower);

        if (GameManager.countMoney >= costTowerToInt)
        {
            tilemap.gameObject.SetActive(true);
            isPlacing = true;
            cancelFunction();
        }
    }

    public void PlantTowerOnMap(Vector2 vector2)
    {
        int costTowerToInt = costTower(flagTypeTower);
        GameManager.countMoney -= costTowerToInt;

        Instantiate(prefabTowerMedian, vector2, Quaternion.identity);

        tilemap.gameObject.SetActive(false);
        isPlacing = false;
    }

    public void cancelFunction()
    {
        menuTowerSelect.SetActive(false);
    }

    public int costTower(int typeTower = -1)
    {
        switch (typeTower)
        {
            case 0: return 50;
            case 1: return 25;
            case 2: return 25;
            default: return 999999;
        }
    }
}
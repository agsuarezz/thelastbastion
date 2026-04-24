using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionMenu : MonoBehaviour
{
    public GameObject menuTowerSelect;
    public GameObject prefabTowerMedian;
    public GameObject prefabTowerLight;
    public GameObject prefabTowerHeavy;
    public Tilemap tilemap;

    // Da permiso para que coloque la torre
    bool isPlacing = false;
    [HideInInspector] public int flagTypeTower = -1;
    GameManager gameManager;
    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }
    /// <summary>
    /// Se ejecuta en cada frame. Controla la lógica del "Modo Colocación" de torres:
    /// Sigue el ratón fijándolo a la cuadrícula, comprueba si el jugador sigue teniendo dinero, 
    /// valida mediante físicas (OverlapCircle) que la casilla esté libre al hacer clic izquierdo 
    /// para construir, y permite cancelar la acción con el clic derecho.
    /// </summary>
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
            Vector2 exactPosition = new Vector2(x, y);

            // 3. CLIC IZQUIERDO: Intentar plantar
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D[] hits = Physics2D.OverlapBoxAll(exactPosition, new Vector2(0.1f, 0.1f), 0f);
                bool hasObstacle = false;

                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("tower") || hit.CompareTag("Path") || hit.CompareTag("Enemy"))
                    {
                        hasObstacle = true;
                        Debug.Log("Cannot build there. Obstacle: " + hit.gameObject.name);
                        break;
                    }
                }

                if (hasObstacle)
                {
                    StartCoroutine(gameManager.messageError("Casilla bloqueada"));
                    return;
                }
                PlantTowerOnMap(exactPosition);
            }

            // 4. CLIC DERECHO: Cancelar construcción si el jugador se arrepiente
            if (Input.GetMouseButtonDown(1))
            {
                tilemap.gameObject.SetActive(false);
                isPlacing = false;
            }
        }
    }
    /// <summary>
    /// Activa el panel de la interfaz de usuario que permite al jugador seleccionar 
    /// qué tipo de torre desea comprar y construir.
    /// </summary>
    public void contructionMenu()
    {
        menuTowerSelect.SetActive(true);
    }
    /// <summary>
    /// Método asignado al botón de comprar "Torre Mediana".
    /// Comprueba si hay dinero suficiente, asigna el tipo de torre (0), 
    /// activa el modo de colocación en el mapa y cierra el menú.
    /// </summary>
    public void BuyTorreMedian()
    {
        int costTowerToInt = costTower(0);

        if (GameManager.countMoney >= costTowerToInt)
        {
            SetIsPlacingTilemapFlagTypeTower(0);
            cancelFunction();
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }
    /// <summary>
    /// Método asignado al botón de comprar "Torre Ligera".
    /// Comprueba si hay dinero suficiente, asigna el tipo de torre (1), 
    /// activa el modo de colocación en el mapa y cierra el menú.
    /// </summary>
    public void BuyTorreLight()
    {
        int costTowerToInt = costTower(1);

        if (GameManager.countMoney >= costTowerToInt)
        {
            SetIsPlacingTilemapFlagTypeTower(1);
            cancelFunction();
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }
    /// <summary>
    /// Método asignado al botón de comprar "Torre Pesada".
    /// Comprueba si hay dinero suficiente, asigna el tipo de torre (2), 
    /// activa el modo de colocación en el mapa y cierra el menú.
    /// </summary>
    public void BuyTorreHeavy()
    {
        int costTowerToInt = costTower(2);

        if (GameManager.countMoney >= costTowerToInt)
        {
            SetIsPlacingTilemapFlagTypeTower(2);
            cancelFunction();
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }
    /// <summary>
    /// Instancia el prefab de la torre seleccionada en la coordenada exacta de la cuadrícula.
    /// Oculta la cuadrícula de ayuda (tilemap) y saca al jugador del modo colocación.
    /// (El cobro del dinero se realiza posteriormente desde el script de la propia torre).
    /// </summary>
    public void PlantTowerOnMap(Vector2 vector2)
    {
        int costTowerToInt = costTower(flagTypeTower);
        GameObject prefab = setPrefabType();
        if(prefab != null)
            Instantiate(prefab, vector2, Quaternion.identity);

        tilemap.gameObject.SetActive(false);
        isPlacing = false;
    }
    /// <summary>
    /// Prepara el sistema para el modo de construcción: guarda el tipo de torre elegida,
    /// enciende la cuadrícula visual (tilemap) y activa la bandera (isPlacing) para que el Update empiece a leer el ratón.
    /// </summary>
    public void SetIsPlacingTilemapFlagTypeTower(int type)
    {
        flagTypeTower = type;
        tilemap.gameObject.SetActive(true);
        isPlacing = true;
    }
    /// <summary>
    /// Cierra el menú de selección de torres sin realizar ninguna acción.
    /// </summary>
    public void cancelFunction()
    {
        menuTowerSelect.SetActive(false);
    }
    /// <summary>
    /// Devuelve la referencia al GameObject (Prefab) correspondiente basándose 
    /// en el tipo de torre seleccionada (flagTypeTower).
    /// </summary>
    public GameObject setPrefabType()
    {
        switch(flagTypeTower)
        {
            case 0: return prefabTowerMedian;
            case 1: return prefabTowerLight;
            case 2: return prefabTowerHeavy;
            default: return null;
        }
    }
    /// <summary>
    /// Devuelve el coste base en oro necesario para comprar una torre 
    /// específica según su identificador (0 = Media, 1 = Ligera, 2 = Pesada).
    /// </summary>
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
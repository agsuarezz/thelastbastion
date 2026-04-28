using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ConstructionMenu : MonoBehaviour
{
    public GameObject menuTowerSelect;
    public GameObject prefabTowerMedian;
    public GameObject prefabTowerLight;
    public GameObject prefabTowerHeavy;
    public Tilemap tilemap;

    // Da permiso para que coloque la torre
    [HideInInspector] public bool isPlacing = false;
    [HideInInspector] public int flagTypeTower = -1;
    GameManager gameManager;
    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
            Debug.LogWarning("GameManager no encontrado");
        // Buscamos TODOS los componentes LineRenderer que haya en el juego.
        // Incluimos los que están apagados (Inactive.Include) y le decimos que no pierda tiempo ordenándolos (SortMode.None).
        LineRenderer[] listLineRender = FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (LineRenderer lineRenderer in listLineRender)
        {
            // Le preguntamos al objeto actual si tiene Tilemap
            this.tilemap = lineRenderer.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                break;
            }
        }

    }
    /// <summary>
    /// Se ejecuta en cada frame. Controla la l�gica del "Modo Colocaci�n" de torres:
    /// Sigue el rat�n fij�ndolo a la cuadr�cula, comprueba si el jugador sigue teniendo dinero, 
    /// valida mediante f�sicas (OverlapCircle) que la casilla est� libre al hacer clic izquierdo 
    /// para construir, y permite cancelar la acci�n con el clic derecho.
    /// </summary>
    private void Update()
    {
        if (GameManager.currentState != GameState.Playing) return;

        // --- LÓGICA: Abrir/Cerrar menú con tecla 'Q' ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool isMenuActive = menuTowerSelect.activeSelf;
            menuTowerSelect.SetActive(!isMenuActive);

            if (isMenuActive && isPlacing)
            {
                isPlacing = false;
                tilemap.gameObject.SetActive(false);
            }
        }
        // -----------------------------------------------

        // --- NUEVA LÓGICA: Atajos 1, 2 y 3 para comprar ---
        // Solo comprobamos estas teclas si el menú de selección ESTÁ ABIERTO
        if (menuTowerSelect.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) // Tecla '1'
            {
                BuyTorreMedian();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) // Tecla '2'
            {
                BuyTorreLight();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) // Tecla '3'
            {
                BuyTorreHeavy();
            }
        }
        // --------------------------------------------------

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
    /// qu� tipo de torre desea comprar y construir.
    /// </summary>
    public void contructionMenu()
    {
        if (GameManager.currentState != GameState.Playing) return;
        menuTowerSelect.SetActive(true);
    }
    /// <summary>
    /// M�todo asignado al bot�n de comprar "Torre Mediana".
    /// Comprueba si hay dinero suficiente, asigna el tipo de torre (0), 
    /// activa el modo de colocaci�n en el mapa y cierra el men�.
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
            dontHaveMoney();
        }
    }
    /// <summary>
    /// M�todo asignado al bot�n de comprar "Torre Ligera".
    /// Comprueba si hay dinero suficiente, asigna el tipo de torre (1), 
    /// activa el modo de colocaci�n en el mapa y cierra el men�.
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
            dontHaveMoney();
        }
    }
    /// <summary>
    /// M�todo asignado al bot�n de comprar "Torre Pesada".
    /// Comprueba si hay dinero suficiente, asigna el tipo de torre (2), 
    /// activa el modo de colocaci�n en el mapa y cierra el men�.
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
            dontHaveMoney();
        }
    }
    /// <summary>
    /// Instancia el prefab de la torre seleccionada en la coordenada exacta de la cuadr�cula.
    /// Oculta la cuadr�cula de ayuda (tilemap) y saca al jugador del modo colocaci�n.
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
    /// Prepara el sistema para el modo de construcci�n: guarda el tipo de torre elegida,
    /// enciende la cuadr�cula visual (tilemap) y activa la bandera (isPlacing) para que el Update empiece a leer el rat�n.
    /// </summary>
    public void SetIsPlacingTilemapFlagTypeTower(int type)
    {
        flagTypeTower = type;
        tilemap.gameObject.SetActive(true);
        isPlacing = true;
    }
    /// <summary>
    /// Cierra el men� de selecci�n de torres sin realizar ninguna acci�n.
    /// </summary>
    public void cancelFunction()
    {
        menuTowerSelect.SetActive(false);
    }
    /// <summary>
    /// Devuelve la referencia al GameObject (Prefab) correspondiente bas�ndose 
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
    /// espec�fica seg�n su identificador (0 = Media, 1 = Ligera, 2 = Pesada).
    /// </summary>
    public static int costTower(int typeTower = -1)
    {
        switch (typeTower)
        {
            case 0: return Tower.upgradeCosts[0][0];
            case 1: return Tower.upgradeCosts[1][0];
            case 2: return Tower.upgradeCosts[2][0];
            default: return Tower.upgradeCosts[0][0];
        }
    }
    public void dontHaveMoney()
    {
        StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        GameManager.sound(GameManager.soundError);
    }
}
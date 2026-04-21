using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// Gestiona la detección de objetivos mediante un radio configurable y dispara.
/// También controla la lógica de construcción al hacer clic sobre la casilla vacía.
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Atributos de la torre (Algunos son privados).")]
    [Tooltip("Radio de detección de los enemigos.")]
    public float attackRadius = 1f;

    //Tiempo de recarga en segundos entre cada disparo.
    float fireCooldown = 1f;

    [Header("Referencias.")]
    [Tooltip("Los prefabs de los proyectiles que se va a instanciar.")]
    public GameObject projectilePrefab;
    public GameObject projectilePrefabTower2;
    public GameObject projectilePrefabTower3;

    [Tooltip("GameObject que contiene el Sprite y BoxCollider2D base de la torre.")]
    public List<GameObject> towerImagen1;
    public List<GameObject> towerImagen2;
    public List<GameObject> towerImagen3;
    [Tooltip("Referencia al GameManager principal de la escena.")]
    public GameManager gameManager;
    // ==========================================
    // ESTADO INTERNO DE LA TORRE
    // ==========================================

    /// <summary>
    /// El enemigo actual al que la torre está apuntando y disparando.
    /// </summary>
    private Transform currentTarget;
    /// <summary>
    /// Temporizador interno que cuenta el tiempo restante (en segundos) hasta el próximo disparo.
    /// </summary>
    private float fireTimer = 0f;
    /// <summary>
    /// Indica si la torre ya ha sido comprada y construida en esta casilla.
    /// </summary>
    private bool isBuilt = false;
    // ==========================================
    // INTERFAZ DE USUARIO Y COMPONENTES HIJOS
    // ==========================================

    /// <summary>
    /// Referencia al panel o menú visual que aparece al hacer clic en una casilla vacía para comprar torres.
    /// </summary>
    public GameObject menuTowerSelect;
    /// <summary>
    /// Referencia al componente visual principal de la torre (su dibujo).
    /// </summary>
    SpriteRenderer spriteRenderer;
    /// <summary>
    /// Referencia al script hijo encargado de detectar si el jugador quiere vender/borrar esta torre.
    /// </summary>
    DeleteTower deletetower;
    /// <summary>
    /// Referencia al script hijo encargado de detectar si el jugador quiere mejorar (upgrade) esta torre.
    /// </summary>
    UpdateTower updatetower;
    /// <summary>
    /// El GameObject visual  que contiene el script de borrado y el sprite.
    /// </summary>
    public GameObject deleteTowerGameObject;
    /// <summary>
    /// El GameObject visualque contiene el script de mejora y el sprite.
    /// </summary>
    public GameObject updateTowerGameObject;
    // Flag que indica el tipo de torre elegida por el usuario
    int typeTower = -1;
    [Tooltip("El daño actual de ESTA torre específica.")]
    public int currentDamage;
    /// <summary>
    /// Inicializa las referencias principales de la torre (SpriteRenderer y los scripts de los botones hijos)
    /// y se asegura de que el menú emergente de selección empiece oculto al iniciar la partida.
    /// </summary>
    private void Start()
    {
       
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        menuTowerSelect.SetActive(false);
        deletetower = this.GetComponentInChildren<DeleteTower>(true);
        updatetower = this.GetComponentInChildren<UpdateTower>(true);
    }
    /// <summary>
    /// Se ejecuta frame a frame y controla el ciclo de vida de la torre:
    /// 1. Comprueba si el jugador ha solicitado una mejora (cambiando sprites y niveles).
    /// 2. Comprueba si el jugador ha vendido la torre (restaurando la casilla y devolviendo oro).
    /// 3. Si la torre está construida y activa, busca enemigos y gestiona el temporizador para disparar.
    /// </summary>
    private void Update()
    {
        if(updatetower && updatetower.needUpdateTower && updatetower.typeOfTower != -1)
        {
            if(updatetower.typeOfTower == 0)
            {
                if(updatetower.levelOfTower == 1)
                {
                    towerMedian(towerImagen1[1].GetComponent<SpriteRenderer>().sprite, towerImagen1[1].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }
                else if(updatetower.levelOfTower == 2)
                {
                    towerMedian(towerImagen1[2].GetComponent<SpriteRenderer>().sprite, towerImagen1[2].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }
                
            }
            if (updatetower.typeOfTower == 1)
            {
                if (updatetower.levelOfTower == 1)
                {
                    towerLight(towerImagen2[1].GetComponent<SpriteRenderer>().sprite, towerImagen2[1].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }
                else if (updatetower.levelOfTower == 2)
                {
                    towerLight(towerImagen2[2].GetComponent<SpriteRenderer>().sprite, towerImagen2[2].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }

            }
            if (updatetower.typeOfTower == 2)
            {
                if (updatetower.levelOfTower == 1)
                {
                    towerHeavy(towerImagen3[1].GetComponent<SpriteRenderer>().sprite, towerImagen3[1].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }
                else if (updatetower.levelOfTower == 2)
                {
                    towerHeavy(towerImagen3[2].GetComponent<SpriteRenderer>().sprite, towerImagen3[2].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }

            }
        }
        if (deletetower && deletetower.isDeleteTower)
        {
            spriteRenderer.sprite = Resources.Load<Sprite>("Square");
            this.GetComponent<BoxCollider2D>().size = new Vector2(1f, 1f);
            GameManager.countMoney += 25;
            isBuilt = false;
            GameManager.countTower -= 1;
            typeTower = -1;
            updatetower.typeOfTower = -1;
            if (menuTowerSelect) menuTowerSelect.SetActive(false);
            deletetower.isDeleteTower = false;
            deleteTowerGameObject.SetActive(false);
            updateTowerGameObject.SetActive(false);
            return;
        }
        if (!isBuilt) return;

        UpdateTarget();

        if (currentTarget == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            fireTimer = fireCooldown * GameManager.globalAttackSpeedMultiplier;
        }
    }

    /// <summary>
    /// Busca enemigos dentro del radio y elige al que más adelantado va en el camino.
    /// </summary>
    private void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float bestProgress = -Mathf.Infinity;
        GameObject bestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy <= (attackRadius * GameManager.globalRadiusMultiplier))
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();

                if (enemyScript != null)
                {
                    if (enemyScript.IsDead) continue;

                    float progress = enemyScript.GetPathProgress();

                    if (progress > bestProgress)
                    {
                        bestProgress = progress;
                        bestEnemy = enemy;
                    }
                }
            }
        }

        currentTarget = bestEnemy != null ? bestEnemy.transform : null;
    }

    /// <summary>
    /// Instancia un proyectil en la posición actual de la torre y le asigna el objetivo fijado.
    /// </summary>
    private void Shoot()
    {
        Vector3 startPos = transform.position;
        if (typeTower == 1)
            projectilePrefab = projectilePrefabTower2;
        if (typeTower == 2)
            projectilePrefab = projectilePrefabTower3;
        GameObject projectileGO = Instantiate(projectilePrefab, startPos, Quaternion.identity);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget);
            projectile.SetDamage(currentDamage);
        }
    }

    /// <summary>
    /// Activa el menu de Selector de Torres y Añade a los botones su respectiva funcion que se encarga de todo
    /// </summary>
    private void OnMouseDown()
    {
        if (spriteRenderer.sprite.name == "Square")
        {
            menuTowerSelect.SetActive(true);
            // 1. Buscamos los botones
            Button btnMedian = GameObject.Find("ButtonTowerMedian").GetComponent<Button>();
            Button btnLight = GameObject.Find("ButtonTowerLight").GetComponent<Button>();
            Button btnHeavy = GameObject.Find("ButtonTowerHeavy").GetComponent<Button>();
            Button btnCancel = GameObject.Find("ButtonCancel").GetComponent<Button>();

            // 2. ¡LA MAGIA! Limpiamos la memoria de los botones para que olviden otras torres
            btnMedian.onClick.RemoveAllListeners();
            btnLight.onClick.RemoveAllListeners();
            btnHeavy.onClick.RemoveAllListeners();
            btnCancel.onClick.RemoveAllListeners();

            // 3. Añadimos las funciones de ESTA torre en concreto
            btnMedian.onClick.AddListener(() => towerMedian());
            btnLight.onClick.AddListener(() => towerLight());
            btnHeavy.onClick.AddListener(() => towerHeavy());
            btnCancel.onClick.AddListener(cancelFunction);
        }
        else if (spriteRenderer.sprite.name != "Square")
        {
            StartCoroutine(gameManager.messageError("Lugar ya ocupado"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres
    /// </summary>
    public void cancelFunction()
    {
        menuTowerSelect.SetActive(false);
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Mediana en el lugar seleccionado
    /// </summary>
    public void towerMedian(Sprite sprite = null, BoxCollider2D boxCollider = null)
    {
        if (sprite == null)
            sprite = towerImagen1[0].GetComponent<SpriteRenderer>().sprite;
        if (boxCollider == null)
            boxCollider = towerImagen1[0].GetComponent<BoxCollider2D>();
        int costTower = 50;
        if (GameManager.countMoney >= costTower)
        {

            Projectile projectilePrefabScript = projectilePrefab.GetComponent<Projectile>();
            if(updatetower.levelOfTower > 0 )
            {
                fireCooldown += 0.5f;
                currentDamage += 10;
            }
            deleteTowerGameObject.SetActive(true);
            updateTowerGameObject.SetActive(true);
            spriteRenderer.sprite = sprite;
            BoxCollider2D boxCollider2D = boxCollider;
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
            GameManager.countTower += 1;
            GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
            menuTowerSelect.SetActive(false);
            setTypeTower(0);
            setCurrentDamage();
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Ligera en el lugar seleccionado
    /// </summary>
    public void towerLight(Sprite sprite = null, BoxCollider2D boxCollider = null)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        int costTower = 100;
        if (sprite == null)
            sprite = towerImagen2[0].GetComponent<SpriteRenderer>().sprite;
        if (boxCollider == null)
            boxCollider = towerImagen2[0].GetComponent<BoxCollider2D>();
        if (GameManager.countMoney >= costTower)
        {
            deleteTowerGameObject.SetActive(true);
            updateTowerGameObject.SetActive(true);
            fireCooldown = 0.5f;
            spriteRenderer.sprite = sprite;
            BoxCollider2D boxCollider2D = boxCollider;
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
            GameManager.countTower += 1;
            setTypeTower(1);
            setCurrentDamage();
            GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
            menuTowerSelect.SetActive(false);
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Pesada en el lugar seleccionado
    /// </summary>
    public void towerHeavy(Sprite sprite = null, BoxCollider2D boxCollider = null)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        int costTower = 100;
        if (sprite == null)
            sprite = towerImagen3[0].GetComponent<SpriteRenderer>().sprite;
        if (boxCollider == null)
            boxCollider = towerImagen3[0].GetComponent<BoxCollider2D>();
        if (GameManager.countMoney >= costTower)
        {
         
            deleteTowerGameObject.SetActive(true);
            updateTowerGameObject.SetActive(true);
            fireCooldown = 2f;
            spriteRenderer.sprite = sprite;
            BoxCollider2D boxCollider2D = boxCollider;
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
            GameManager.countTower += 1;
            setTypeTower(2);
            setCurrentDamage();
            GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
            menuTowerSelect.SetActive(false);
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }
    /// <summary>
    /// Dibuja el radio de ataque en color rojo en la vista de escena (Scene) 
    /// cuando la torre está seleccionada para ayudar visualmente en el diseño del nivel.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
    void setCurrentDamage()
    {
        if (updatetower.levelOfTower == 0)
            if (updatetower.typeOfTower == 0)
                currentDamage = 20;
            else if (updatetower.typeOfTower == 1)
                currentDamage = 10;
            else
                currentDamage = 40;
    }
    void setTypeTower(int type)
    {
        typeTower = type;
        updatetower.typeOfTower = type;
    }
}
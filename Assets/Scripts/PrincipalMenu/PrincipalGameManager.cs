using UnityEngine;

public class PrincipalGameManager : MonoBehaviour
{
    public static PrincipalGameManager Instance;

    [Header("Referencias del Menu")]
    public PrincipalSpawner spawner;
    public principalSceneTransition sceneTransition;

    private void Awake()
    {
        // Singleton básico
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("PrincipalGameManager activo");
    }
}
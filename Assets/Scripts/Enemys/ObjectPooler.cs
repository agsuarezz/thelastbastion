using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [Tooltip("Prefab que se va a reutilizar")]
    public GameObject prefab;

    [Tooltip("Cantidad inicial de objetos en el pool")]
    public int initialSize = 10;

    private List<GameObject> pool;

    private void Awake()
    {
        pool = new List<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        // Buscar objeto libre
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // 🔥 IMPORTANTE: pool dinámico (esto evita que se rompa el juego)
        GameObject newObj = Instantiate(prefab);
        newObj.SetActive(false);
        pool.Add(newObj);

        return newObj;
    }
}
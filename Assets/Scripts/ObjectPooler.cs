using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    [Tooltip("El prefab que se va a instanciar y guardar en el pool.")]
    [SerializeField] private GameObject prefab;
    [Tooltip("Cantidad inicial de objetos en el pool.")]
    [SerializeField] private int poolSize = 5;

    private List<GameObject> _pool;

    void Awake()
    {
        _pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        _pool.Add(obj);
        Debug.Log("¡Fabricando clon de emergencia!");
        return obj;
    }

    /// <summary>
    /// Busca un objeto inactivo en la lista. Si todos están en uso, crea uno nuevo (Pool Dinámico).
    /// </summary>
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].activeInHierarchy)
            {
                return _pool[i];
            }
        }

        return CreateNewObject();
    }
}

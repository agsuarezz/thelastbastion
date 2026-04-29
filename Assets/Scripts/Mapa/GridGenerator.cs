using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    // Lista donde se guardan los prefabs de los mapas y sus puntos de ruta
    public List<GameObject> list_grid;
    private void Start()
    {
        // Genera un número entero aleatorio
        int indexRandom = Random.Range(0, list_grid.Count);
        // Encuentra al padre
        GameObject father = GameObject.Find("Grid");
        // Crea una instancia en la escena del objeto seleccionado aleatoriamente de la lista
        GameObject son = Instantiate(list_grid[indexRandom]);
        // Lo metes dentro del padre
        son.transform.SetParent(father.transform);
    }
}

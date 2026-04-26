using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public List<GameObject> list_grid;
    private void Start()
    {
        int indexRandom = Random.Range(0, list_grid.Count);
        Instantiate(list_grid[indexRandom]);
    }
}

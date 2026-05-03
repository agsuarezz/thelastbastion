using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static int selectedGridIndex = -1;

    public List<GameObject> list_grid;

    private void Start()
    {
        if (list_grid == null || list_grid.Count == 0)
        {
            Debug.LogWarning("No hay grids asignados.");
            return;
        }

        int indexToUse;

        if (selectedGridIndex >= 0 && selectedGridIndex < list_grid.Count)
        {
            indexToUse = selectedGridIndex;
        }
        else
        {
            indexToUse = Random.Range(0, list_grid.Count);
            selectedGridIndex = indexToUse;
        }

        GameObject father = GameObject.Find("Grid");

        GameObject son = Instantiate(list_grid[indexToUse]);

        if (father != null)
        {
            son.transform.SetParent(father.transform);
        }
    }
}
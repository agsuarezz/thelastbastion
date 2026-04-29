using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SearchandMovetotheMap());
    }
    public IEnumerator SearchandMovetotheMap()
    {
        yield return new WaitForEndOfFrame();
        CastleSpawnPoint castleSpawnPoint = FindAnyObjectByType<CastleSpawnPoint>();
        if (castleSpawnPoint != null)
        {
            transform.position = castleSpawnPoint.PortalCalculatedCenter;
            transform.rotation = castleSpawnPoint.portalSpawnRotation;
        }
        else
        {
            Debug.LogWarning("El Portal no encontró ningún MapConfig en la escena.");
        }
    }
}

using System.Collections;
using UnityEngine;

public class coinScript : MonoBehaviour
{
    public void Start()
    {
        StartCoroutine(destroyCoin());
    }
    private void OnMouseDown()
    {
        GameManager.countMoney += 10;
        Destroy(gameObject);
    }
    IEnumerator destroyCoin()
    {
        yield return new WaitForSeconds(20f);
        Destroy(gameObject);
    }
}

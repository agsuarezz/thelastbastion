using System.Collections;
using UnityEngine;

public class coinScript : MonoBehaviour
{
    public void Start()
    {
        StartCoroutine(destroyCoin());
    }
    private void FixedUpdate()
    {
        this.GetComponent<Transform>().Rotate(0f, 10f, 0f);
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

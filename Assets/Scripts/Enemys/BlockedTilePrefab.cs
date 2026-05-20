using UnityEngine;

public class BlockedTile : MonoBehaviour
{
    private int roundToDestroy;

    public void Init(int roundsDuration)
    {
        roundToDestroy = GameManager.countRound + roundsDuration;
    }

    private void Update()
    {
        if (GameManager.countRound >= roundToDestroy)
        {
            Destroy(gameObject);
        }
    }
}
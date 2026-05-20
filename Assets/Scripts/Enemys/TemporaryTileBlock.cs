using UnityEngine;

public class TemporaryTileBlock : MonoBehaviour
{
    public int roundsLeft = 2;

    public void ReduceRound()
    {
        roundsLeft--;

        if (roundsLeft <= 0)
        {
            Destroy(gameObject);
        }
    }
}
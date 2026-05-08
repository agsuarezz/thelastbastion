using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "NewSpawnAds", menuName = "Bastion/Events/Spawn Ads")]

public class EventSpawnAds : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameObject[] adPrefabs = new GameObject[4];
        adPrefabs[0] = Resources.Load<GameObject>("prefabNew");
        adPrefabs[1] = Resources.Load<GameObject>("prefabNew1");
        adPrefabs[2] = Resources.Load<GameObject>("prefabNew2");
        adPrefabs[3] = Resources.Load<GameObject>("prefabNew3");

        Transform parentCanvas = GameObject.Find("Canvas_General").transform;

        for (int i = 0; i < 4; i++)
        {
            GameObject selectedPrefab = adPrefabs[i];
            GameObject spawnedAd = Instantiate(selectedPrefab, parentCanvas, false);
            RectTransform adRect = spawnedAd.GetComponent<RectTransform>();

            float randomX = UnityEngine.Random.Range(-700f, 700f);
            float randomY = UnityEngine.Random.Range(-200f, 200f);

            adRect.anchoredPosition = new Vector2(randomX, randomY);
            yield return new WaitForSeconds(0.15f);
        }

    }
}

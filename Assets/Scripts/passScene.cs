using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class passScene : MonoBehaviour
{
    public string nameScene;

    void Start()
    {
        StartCoroutine(waitTime());
    }


    public IEnumerator waitTime()
    {
        yield return new WaitForSeconds(5f);
        loadScene();
    }

    public void loadScene()
    {
        SceneManager.LoadScene(nameScene);
    }
}
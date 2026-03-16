using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void PlayGame()
    {
        ScreenFader.Instance.FadeToScene("Level");
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        yield return StartCoroutine(ScreenFader.Instance.FadeOut());
        Application.Quit();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1); 
    }

    public void QuitGame()
    {
        Debug.Log("Игра закрыта.");
        Application.Quit();
    }
}
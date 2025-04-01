using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void StartGame()
    {
        // Загружает игровую сцену
        SceneManager.LoadScene("GameZombi");
        Debug.Log("Start Game нажата");
    }

    public void ExitGame()
    {
        // Выход из игры
        Debug.Log("Выход из игры");
        Application.Quit();
    }
}
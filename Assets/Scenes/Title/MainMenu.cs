using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour{

    public void PlayGame ()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1f;
    }

    public void QuitGame ()
    {
        
        Application.Quit();
    }

    public void BackGame ()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Time.timeScale = 1f;
    }

    public void InteriorGame()
    {
        SceneManager.LoadScene(3);
        Time.timeScale = 1f;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    public void CityGame()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1f;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }

    public void Neighborhood()
    {
        SceneManager.LoadScene(2);
        Time.timeScale = 1f;

    }

}


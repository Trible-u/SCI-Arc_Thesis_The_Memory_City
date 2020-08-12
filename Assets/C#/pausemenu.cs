using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pausemenu : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    private bool pausegame = false;
    //Input.GetKeyDown(KeyCode.Escape)

    // Update is called once per frame
    void Update()
    {
        if (pausegame)
        {
            if(GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
                pausegame = false;
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void instruction()
    {
        pausegame = true;
    }
}

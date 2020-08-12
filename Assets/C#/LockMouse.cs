using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockMouse : MonoBehaviour
{
    private bool _mouselookEnabled = false;
    private bool _shifted = false;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftShift) & _shifted)
            _shifted = false;

        if ((Input.GetKeyDown(KeyCode.LeftShift) & !_shifted) |
            (Input.GetKeyDown(KeyCode.Escape) & _mouselookEnabled))
        {
            _shifted = true;

            if (!_mouselookEnabled)
            {
                _mouselookEnabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    _shifted = false;

                _mouselookEnabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
        }

        if (!_mouselookEnabled)
            return;
    }
}

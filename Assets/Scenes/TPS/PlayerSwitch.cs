using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwitch : MonoBehaviour
{

    // referenses to controlled game objects
    public GameObject avatar1, avatar2, avatar3, avatar4;

    // variable contains which avatar is on and active
 

    // Use this for initialization
    void Start()
    {

        // anable first avatar and disable another one
        avatar1.gameObject.SetActive(true);
        avatar2.gameObject.SetActive(false);
        avatar3.gameObject.SetActive(false);
        avatar4.gameObject.SetActive(false);
    }

    // public method to switch avatars by pressing UI button
    void Update()
    {
        if (Input.GetButtonDown("1Key"))
        {
            avatar1.gameObject.SetActive(true);
            avatar2.gameObject.SetActive(false);
            avatar3.gameObject.SetActive(false);
            avatar4.gameObject.SetActive(false);
        }
        if (Input.GetButtonDown("2Key"))
        {
            avatar1.gameObject.SetActive(false);
            avatar2.gameObject.SetActive(true);
            avatar3.gameObject.SetActive(false);
            avatar4.gameObject.SetActive(false);
        }
        if (Input.GetButtonDown("3Key"))
        {
            avatar1.gameObject.SetActive(false);
            avatar2.gameObject.SetActive(false);
            avatar3.gameObject.SetActive(true);
            avatar4.gameObject.SetActive(false);
        }
        if (Input.GetButtonDown("4Key"))
        {
            avatar1.gameObject.SetActive(false);
            avatar2.gameObject.SetActive(false);
            avatar3.gameObject.SetActive(false);
            avatar4.gameObject.SetActive(true);
        }
    }
}

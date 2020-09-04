using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Remember : MonoBehaviour
{
    public GameObject show;
    void Start()
    {
        show.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            show.gameObject.SetActive(true);
    }

    
}

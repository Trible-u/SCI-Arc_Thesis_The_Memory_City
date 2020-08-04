using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anima : MonoBehaviour
{
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey (KeyCode.W))
        {
            anim.SetInteger ("walk", 1);
        }
        if (Input.GetKeyUp (KeyCode.W))
        {
            anim.SetInteger ("walk", 0);
        }
    }
}

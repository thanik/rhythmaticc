using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyBindingsMenu : MonoBehaviour
{
    private PlayerInput input;
    void Start()
    {
        input = GetComponent<PlayerInput>();
        input.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

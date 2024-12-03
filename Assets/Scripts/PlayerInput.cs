using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private TireController tireController;
    private float accelerationInput;
    private float horizontalInput;
    private bool handbrakeOn;

    private void Awake()
    {
        tireController = GetComponent<TireController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!tireController.IsOwner || !Application.isFocused) { return; }

        accelerationInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        handbrakeOn = Input.GetKey(KeyCode.Space);
        tireController.SetInputs(accelerationInput, horizontalInput, handbrakeOn);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AvatarMovement : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    private Vector2 inputAxis;

    void Update()
    {
        // Get input from the left controller's joystick
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);

        // Calculate movement
        Vector3 movement = new Vector3(inputAxis.x, 0, inputAxis.y) * moveSpeed * Time.deltaTime;

        // Apply movement to the avatar
        transform.Translate(movement);
    }
}
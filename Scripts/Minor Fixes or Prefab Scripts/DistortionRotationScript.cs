using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistortionRotationScript : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 0, 2); // degrees per second

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}

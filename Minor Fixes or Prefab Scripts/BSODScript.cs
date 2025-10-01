using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSODScript : MonoBehaviour
{
    public Canvas thisCanvas; 

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.SetSiblingIndex(thisCanvas.transform.childCount - 1);
    }
}

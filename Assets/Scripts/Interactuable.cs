using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactuable : MonoBehaviour
{
    public bool activated = false;

    public virtual void Activate()
    {
        Debug.Log("Activate interactuable");
        activated = true;
    }
}

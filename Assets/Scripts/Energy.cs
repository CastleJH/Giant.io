using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    public int power;

    void Awake()
    {
        InitializeEnergy(3);    
    }

    public void InitializeEnergy(int _power)
    {
        power = _power;
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 360 * Time.deltaTime, 0));
    }
}

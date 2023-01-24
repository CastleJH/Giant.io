using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    public int id;
    public int power;       //에너지 흡수 시 받을 점수
    public Player myOwner;  //에너지를 생성한(소유한) 플레이어

    public void InitializeEnergy(int _id, int _power, Player _owner)
    {
        id = _id;
        power = _power;
        myOwner = _owner;
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 360 * Time.deltaTime, 0));
    }

    public void RemoveThisFromScene()
    {
        myOwner.RemoveEnergy(this);
    }
}

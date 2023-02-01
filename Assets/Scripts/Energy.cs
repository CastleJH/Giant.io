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

        float size = Mathf.Lerp(0.5f, 3.0f, (Mathf.Clamp(_power, 1, 1000) - 1.0f) / 999.0f);
        transform.localScale = Vector3.one * size;
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 360 * Time.deltaTime, 0));
        if (myOwner == null) Destroy(gameObject);
    }

    public void RemoveThisFromScene()
    {
        myOwner.RemoveEnergy(this);
    }
}

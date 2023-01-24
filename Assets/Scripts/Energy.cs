using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    public int id;
    public int power;       //������ ��� �� ���� ����
    public Player myOwner;  //�������� ������(������) �÷��̾�

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

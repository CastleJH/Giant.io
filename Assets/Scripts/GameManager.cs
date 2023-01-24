using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    //public Joystick joystickCam;
    public GameObject energyPrefab;

    Player myPlayer;

    //�Ʒ��� �� �÷��̾��� ��ų ���� ������
    [HideInInspector] public float jumpCool;
    float boostCool;
    float kickCool;

    //ī�޶� ���� ����
    [HideInInspector] public Vector3 camOffset;

    void Awake()
    {
        //�̰� ���߿� ���� ��!
        //Screen.SetResolution(900, 500, false);

        if (instance == null) instance = this;
        camOffset = new Vector3(0, 4, -8);
    }

    void Update()
    {
        //�Ź� ��Ÿ�� Ȯ��
        CoolTimeDown();
    }

    //�� �÷��̾ �����Ѵ�.
    public void SpawnMyPlayer()
    {
        if (myPlayer == null)
        {
            myPlayer = PhotonNetwork.Instantiate("Player", new Vector3(0, 2, 0), Quaternion.identity).GetComponent<Player>();
            Camera.main.transform.parent = myPlayer.transform;
            Camera.main.transform.localPosition = camOffset;
        }
        jumpCool = 0;
        boostCool = 0;
        kickCool = 0;
    }

    //������ư ������ ��
    public void ButtonJump()
    {
        if (jumpCool <= 0)
        {
            myPlayer.Jump();
        }
    }

    //�ν�Ʈ ��ư ������ ��
    public void ButtonBoost()
    {
        if (boostCool <= 0)
        {
            myPlayer.Boost();
            boostCool = 30;
        }
    }

    //������ ��ư ������ ��
    public void ButtonKick()
    {
        if (kickCool <= 0)
        {
            myPlayer.Kick();
            kickCool = 5;
        }
    }

    //��Ÿ�� ����
    void CoolTimeDown()
    {
        if (jumpCool > 0) jumpCool -= Time.deltaTime;
        if (boostCool > 0) boostCool -= Time.deltaTime;
        if (kickCool > 0) kickCool -= Time.deltaTime;
    }
/*
    //������ ������Ʈ�� �������� �����Ѵ�.
    IEnumerator FixEnergyOwnership()
    {
        while (true)
        {
            Vector3 myPlayerPos = new Vector3(myPlayer.transform.position.x, 0, myPlayer.transform.position.z);
            for (int i = myPlayer.energyList.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(myPlayer.energyList[i].transform.position, myPlayerPos) > 20.0f)
                {
                    myPlayer.energyList[i].RemoveThisFromScene();
                }
            }
            yield return new WaitForSeconds(10.0f);
        }
    }*/

}

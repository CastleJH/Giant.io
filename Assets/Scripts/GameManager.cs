using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    //public Joystick joystickCam;
    public GameObject energyPrefab;
    public GameObject mobileUI;
    public GameObject deadPanel;
    public Text killerNameText;
    public Text topScoreText;

    Player myPlayer;

    //�Ʒ��� �� �÷��̾��� ��ų ���� ������
    [HideInInspector] public float jumpCool;
    float kickCool;

    //ī�޶� ���� ����
    [HideInInspector] public Vector3 camOffset;

    void Awake()
    {
        //�̰� ���߿� ���� ��!
        //Screen.SetResolution(900, 500, false);
        if (instance == null) instance = this;
        if (!Application.isMobilePlatform) mobileUI.SetActive(false);
    }

    void Update()
    {
        //�Ź� ��Ÿ�� Ȯ��
        CoolTimeDown();
    }

    //�� �÷��̾ �����Ѵ�.
    public void SpawnMyPlayer()
    {
        Vector3 spawnPos = Vector3.zero;
        if (myPlayer == null) myPlayer = PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity).GetComponent<Player>();
        else myPlayer.pv.RPC("Respawn", RpcTarget.All, spawnPos);

        jumpCool = 0;
        kickCool = 0;

        deadPanel.SetActive(false);
    }

    //������ư ������/���� ��
    public void ButtonJump()
    {
        if (jumpCool <= 0)
        {
            myPlayer.Jump();
        }
    }

    //�ν�Ʈ ��ư ������ ��
    public void ButtonBoost(bool isTrue)
    {
        myPlayer.Boost(isTrue);
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
        if (kickCool > 0) kickCool -= Time.deltaTime;
    }

    public void LeaveThisGame()
    {

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

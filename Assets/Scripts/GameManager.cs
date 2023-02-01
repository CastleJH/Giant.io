using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.Demo.Cockpit;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    //public Joystick joystickCam;
    public GameObject energyPrefab;
    public GameObject mobileUI;
    public GameObject deadPanel;
    public GameObject settingPanel;
    public Slider sensitivitySlider;
    public Text killerNameText;
    public Text topScoreText;

    Player myPlayer;
    public List<Player> players;

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

        StartCoroutine(GiveUpFarEnergyOwnership());
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

    public void ButtonOpenSettingPanel(bool isOpen)
    {
        settingPanel.SetActive(isOpen);
    }

    //��Ÿ�� ����
    void CoolTimeDown()
    {
        if (jumpCool > 0) jumpCool -= Time.deltaTime;
        if (kickCool > 0) kickCool -= Time.deltaTime;
    }

    public void LeaveThisGame()
    {
        StopAllCoroutines();
        players.Clear();
    }

    IEnumerator GiveUpFarEnergyOwnership()
    {
        while (true)
        {
            yield return new WaitForSeconds(10.0f);
            Vector3 myPlayerPos = new Vector3(myPlayer.transform.position.x, 1, myPlayer.transform.position.z);
            Vector3 otherPlayerPos;
            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i] == this) continue;
                otherPlayerPos = new Vector3(players[i].transform.position.x, 1, players[i].transform.position.z);
                for (int j = myPlayer.energyList.Count - 1; j >= 0; j--)
                    if (Vector3.Distance(myPlayer.energyList[j].transform.position, myPlayerPos) > Vector3.Distance(myPlayer.energyList[j].transform.position, otherPlayerPos))
                        myPlayer.GiveEnergyOwnership(players[i], myPlayer.energyList[j].id);
            }
        }
    }
}

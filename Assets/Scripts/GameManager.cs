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

    //아래는 내 플레이어의 스킬 관련 변수들
    [HideInInspector] public float jumpCool;
    float kickCool;

    //카메라 관련 변수
    [HideInInspector] public Vector3 camOffset;

    void Awake()
    {
        //이건 나중에 지울 것!
        //Screen.SetResolution(900, 500, false);
        if (instance == null) instance = this;
        if (!Application.isMobilePlatform) mobileUI.SetActive(false);
    }

    void Update()
    {
        //매번 쿨타임 확인
        CoolTimeDown();
    }

    //내 플레이어를 생성한다.
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

    //점프버튼 눌렸을/뗐을 때
    public void ButtonJump()
    {
        if (jumpCool <= 0)
        {
            myPlayer.Jump();
        }
    }

    //부스트 버튼 눌렸을 때
    public void ButtonBoost(bool isTrue)
    {
        myPlayer.Boost(isTrue);
    }

    //발차기 버튼 눌렸을 때
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

    //쿨타임 감소
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

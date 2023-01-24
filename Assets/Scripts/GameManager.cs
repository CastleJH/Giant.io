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

    //아래는 내 플레이어의 스킬 관련 변수들
    [HideInInspector] public float jumpCool;
    float boostCool;
    float kickCool;

    //카메라 관련 변수
    [HideInInspector] public Vector3 camOffset;

    void Awake()
    {
        //이건 나중에 지울 것!
        //Screen.SetResolution(900, 500, false);

        if (instance == null) instance = this;
        camOffset = new Vector3(0, 4, -8);
    }

    void Update()
    {
        //매번 쿨타임 확인
        CoolTimeDown();
    }

    //내 플레이어를 생성한다.
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

    //점프버튼 눌렸을 때
    public void ButtonJump()
    {
        if (jumpCool <= 0)
        {
            myPlayer.Jump();
        }
    }

    //부스트 버튼 눌렸을 때
    public void ButtonBoost()
    {
        if (boostCool <= 0)
        {
            myPlayer.Boost();
            boostCool = 30;
        }
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

    //쿨타임 감소
    void CoolTimeDown()
    {
        if (jumpCool > 0) jumpCool -= Time.deltaTime;
        if (boostCool > 0) boostCool -= Time.deltaTime;
        if (kickCool > 0) kickCool -= Time.deltaTime;
    }
/*
    //에너지 오브젝트의 소유권을 조정한다.
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject playerPrefab;
    public Joystick joystick;

    Player myPlayer;
    [HideInInspector]
    public float jumpCool;
    float boostCool;
    float kickCool;
    [HideInInspector]
    public Vector3 camOffset;

    void Awake()
    {
        if (instance == null) instance = this;
        camOffset = new Vector3(0, 4, -8);
        SpawnPlayer();
    }

    void Update()
    {
        CoolTimeDown();
    }

    void SpawnPlayer()
    {
        myPlayer = Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity).GetComponent<Player>();
        myPlayer.isMine = true;
        jumpCool = 0;
        boostCool = 0;
        kickCool = 0;
        //Camera.main.transform.parent = myPlayer.transform;
        //Camera.main.transform.localPosition = camOffset;
    }

    public void ButtonJump()
    {
        if (jumpCool <= 0)
        {
            myPlayer.Jump();
        }
    }

    public void ButtonBoost()
    {
        if (boostCool <= 0)
        {
            myPlayer.Boost();
            boostCool = 30;
        }
    }

    public void ButtonKick()
    {
        if (kickCool <= 0)
        {
            myPlayer.Kick();
            kickCool = 5;
        }
    }
    void CoolTimeDown()
    {
        if (jumpCool > 0) jumpCool -= Time.deltaTime;
        if (boostCool > 0) boostCool -= Time.deltaTime;
        if (kickCool > 0) kickCool -= Time.deltaTime;
    }
}

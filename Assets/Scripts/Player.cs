using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public PhotonView pv;
    public GameObject landPointGreen;
    public GameObject landPointRed;

    Rigidbody rigid;
    Animator anim;

    public int score;
    float baseSpeed;
    float jumpPower;
    float speed = 20.0f;
    Vector3 moveVec;
    float prevInputX, curInputX;
    float lookDir;
    float movedDist;
    bool isFalling;

    float boostTime;

    public List<Energy> energyList;
    int energyGenID;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        boostTime = 99;
        score = 50;
        isFalling = false;
        ChangeSize();

        energyList = new List<Energy>();
        energyGenID = 0;

        lookDir = 90.0f;
    }

    void Update()
    {
        if (pv.IsMine)
        {
            Move();
            GenerateEnergyNearThis();
            if (anim.GetBool("IsJump") && rigid.velocity.y < 0) isFalling = true;

            landPointGreen.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
        else landPointRed.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    void Move()
    {
        if (boostTime < 5)
        {
            speed = baseSpeed * 2.0f;
            boostTime += Time.deltaTime;
        }
        else speed = baseSpeed;

        if (Input.touchCount > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                prevInputX = Input.touches[0].position.x;
                anim.SetBool("IsRun", true);
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                anim.SetBool("IsRun", false);
            }
            else
            {
                curInputX = Input.touches[0].position.x;
                //추후 감도 조절 기능을 넣는것이 좋겠다.
                lookDir += (prevInputX - curInputX) / Screen.width * 360.0f * Time.deltaTime;
                moveVec = new Vector3(Mathf.Cos(lookDir), 0, Mathf.Sin(lookDir));
                transform.LookAt(transform.position + moveVec);
                transform.position += moveVec * speed * Time.deltaTime;
                prevInputX = curInputX;
            }
        }
    }

    void GenerateEnergyNearThis()
    {
        if (moveVec != Vector3.zero) movedDist += speed * Time.deltaTime;
        if (movedDist > 10 && energyList.Count < 96)
        {
            for (int i = 0; i < 5; i++)
            {
                float deg = Random.Range(0.0f, 360.0f);
                Vector3 pos = new Vector3(
                    Mathf.Cos(deg),
                    0,
                    Mathf.Sin(deg)) * Random.Range(80.0f, 120.0f);
                pos = new Vector3(pos.x + transform.position.x, 1, pos.z + transform.position.z);
                pv.RPC("RPCGenerateEnergy", RpcTarget.AllBuffered, pos, energyGenID);
                energyGenID = (energyGenID + 1) % 10000;
            }
            movedDist = 0;
        }
    }

    //에너지 생성
    [PunRPC]
    void RPCGenerateEnergy(Vector3 pos, int id)
    {
        Energy newEnergy = Instantiate(GameManager.instance.energyPrefab, pos, Quaternion.identity).GetComponent<Energy>();
        newEnergy.InitializeEnergy(id, 3, this);
        energyList.Add(newEnergy);
    }

    public void RemoveEnergy(Energy energy)
    {
        pv.RPC("RPCRemoveEnergy", RpcTarget.AllBuffered, energy.id);
    }

    [PunRPC]
    void RPCRemoveEnergy(int id)
    {
        for (int i = energyList.Count - 1; i >= 0; i--)
        {
            if (energyList[i].id == id)
            {
                GameObject removeObj = energyList[i].gameObject;
                energyList.RemoveAt(i);
                Destroy(removeObj);
                break;
            }
        }
    }

    public void Jump()
    {
        if (anim.GetBool("IsJump")) return;
        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        anim.SetBool("IsJump", true);
        anim.SetTrigger("Jump");
        pv.RPC("RPCTurnOnLandPoint", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPCTurnOnLandPoint()
    {
        if (pv.IsMine) landPointGreen.SetActive(true);
        else landPointRed.SetActive(true);
    }

    public void Boost()
    {
        boostTime = 0;
    }

    public void Kick()
    {

    }

    void ChangeSize()
    {
        float clampedScore = (Mathf.Clamp(score, 50, 1000) - 50.0f) / 950.0f;
        float size = Mathf.Lerp(1.0f, 10.0f, clampedScore);

        gameObject.transform.localScale = new Vector3(size, size, size);
        baseSpeed = Mathf.Lerp(8.0f, 6.0f, clampedScore);
        jumpPower = Mathf.Lerp(7.0f, 35.0f, clampedScore);

        if (pv.IsMine) 
        {
            //GameManager.instance.camDist = new Vector3(size * 8.0f, size * 4.0f, size * 8.0f);
        }
    }

    [PunRPC]
    void Die()
    {
        gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") //바닥에 닿았다.
        {
            if (pv.IsMine)
            {
                anim.SetBool("IsJump", false);
                isFalling = false;
                GameManager.instance.jumpCool = 3;
            }
            landPointGreen.SetActive(false);
            landPointRed.SetActive(false);
        }
        else if (collision.gameObject.tag == "Thorn")
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pv.IsMine)
        {
            if (other.gameObject.tag == "Head" && isFalling) //하강 도중 상대 머리를 밟았다.
            {
                Debug.Log("Attack!");
                other.gameObject.transform.parent.gameObject.GetComponent<Player>().Die();
            }
            else if (other.gameObject.tag == "Energy")
            {
                Energy energy = other.GetComponent<Energy>();
                score += energy.power;
                energy.RemoveThisFromScene();
                ChangeSize();
            }
        }
    }
}


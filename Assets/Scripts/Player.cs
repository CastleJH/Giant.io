using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using Photon.Pun;
using Photon.Realtime;
using static UnityEditor.PlayerSettings;

public class Player : MonoBehaviour
{
    Rigidbody rigid;
    Animator anim;
    Vector3 moveVec;

    public PhotonView pv;
    public GameObject landPoint;
    public GameObject head, feet;
    public int score;
    float baseSpeed;
    float jumpPower;
    float speed = 20.0f;
    float movedDist;
    bool isFalling;

    float boostTime;

    public List<Energy> energyList;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        boostTime = 99;
        score = 50;
        isFalling = false;
        ChangeSize();

        energyList = new List<Energy>();
    }

    void Update()
    {
        if (pv.IsMine)
        {
            Move();
            GenerateEnergyNearThis();
            if (anim.GetBool("IsJump"))
            {
                landPoint.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                if (rigid.velocity.y < 0) isFalling = true;
            }
            Camera.main.transform.position = new Vector3(transform.position.x, GameManager.instance.camOffset.y + transform.position.y, GameManager.instance.camOffset.z + transform.position.z);
        }
    }

    void Move()
    {
        if (boostTime < 5)
        {
            speed = baseSpeed * 2.0f;
            boostTime += Time.deltaTime;
        }
        else speed = baseSpeed;
        moveVec = new Vector3(GameManager.instance.joystick.Horizontal, 0, GameManager.instance.joystick.Vertical).normalized;
        transform.LookAt(transform.position + moveVec);
        transform.position += moveVec * speed * Time.deltaTime;
        anim.SetBool("IsRun", moveVec != Vector3.zero);
    }

    void GenerateEnergyNearThis()
    {
        if (moveVec != Vector3.zero) movedDist += speed * Time.deltaTime;
        if (movedDist > 10)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(
                Random.Range(10.0f, 30.0f) * (Random.Range(0, 2) == 0 ? -1 : 1) + transform.position.x,
                1,
                Random.Range(10.0f, 30.0f) * (Random.Range(0, 2) == 0 ? -1 : 1) + transform.position.z
                );
                pv.RPC("RPCGenerateEnergy", RpcTarget.AllBuffered, pos);
            }
            movedDist = 0;
        }
    }

    //에너지 생성
    [PunRPC]
    void RPCGenerateEnergy(Vector3 pos)
    {
        Energy newEnergy = Instantiate(GameManager.instance.energyPrefab, pos, Quaternion.identity).GetComponent<Energy>();
        newEnergy.InitializeEnergy(3, this);
        energyList.Add(newEnergy);
    }

    public void RemoveEnergy(Energy energy)
    {
        pv.RPC("RPCRemoveEnergy", RpcTarget.AllBuffered, energy);
    }

    [PunRPC]
    void RPCRemoveEnergy(Energy energy)
    {
        for (int i = energyList.Count - 1; i >= 0; i--)
        {
            if (energyList[i] == energy)
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
        landPoint.SetActive(true);
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
            GameManager.instance.camOffset = new Vector3(0, size * 4.0f, size * -8.0f);
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("coll : " + collision.gameObject.tag);
        if (collision.gameObject.tag == "Floor") //바닥에 닿았다.
        {
            anim.SetBool("IsJump", false);
            isFalling = false;
            landPoint.SetActive(false);
            GameManager.instance.jumpCool = 3;
        }
        else if (collision.gameObject.tag == "Thorn")
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trig : " + other.gameObject.tag + " y : " + rigid.velocity.y.ToString());
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


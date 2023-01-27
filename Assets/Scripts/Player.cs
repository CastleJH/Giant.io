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

    //이동 관련 변수
    float baseSpeed;
    float speed = 20.0f;
    float lookDir;
    Vector3 moveVec;
    float prevInputX, curInputX;
    float movedDist;

    //점프 관련 변수
    float jumpPower;
    bool isFalling;

    //부스트 관련 변수
    bool isBoost;

    //제어중인 에너지
    public List<Energy> energyList;
    int energyGenID;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        energyList = new List<Energy>();
        energyGenID = 0;
        isFalling = false;

        if (pv.IsMine)
        {
            ChangeScore(50);
            MoveCamera();
        }

    }

    void Update()
    {
        if (pv.IsMine)
        {
            Move();
            MoveCamera();
            SpawnEnergyNearThis();
            if (anim.GetBool("IsJump") && rigid.velocity.y < 0) isFalling = true;

            landPointGreen.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
        else landPointRed.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    void Move()
    {
        if (isBoost) speed = baseSpeed * 2.0f;
        else speed = baseSpeed;

        if (Application.isMobilePlatform) GetMobileInput();
        else GetPCInput();
    }

    void GetMobileInput()
    {
        if (Input.touchCount > 0)
        {
            int moveFinger = 0;
            if (Input.touchCount > 1 && Input.touches[moveFinger].position.x > Input.touches[1].position.x) moveFinger = 1;
            if (Input.touches[moveFinger].phase == TouchPhase.Began)
            {
                prevInputX = Input.touches[moveFinger].position.x;
                if (prevInputX < Screen.width / 2) anim.SetBool("IsRun", true);
            }
            else if (Input.touches[moveFinger].phase == TouchPhase.Ended || Input.touches[moveFinger].phase == TouchPhase.Canceled)
            {
                anim.SetBool("IsRun", false);
                moveVec = Vector3.zero;
                if (Input.touchCount != 1) prevInputX = Input.touches[(moveFinger + 1) % 2].position.x;
            }
            else
            {
                curInputX = Input.touches[moveFinger].position.x;

                //추후 감도 조절 기능을 넣는것이 좋겠다.
                lookDir -= (prevInputX - curInputX) / Screen.width * 360.0f;
                if (lookDir < -180) lookDir += 360;
                else if (lookDir > 180) lookDir -= 360;

                transform.rotation = Quaternion.Euler(0, lookDir, 0);

                //방향만 결정하지 않고 이동까지 하는 경우
                if (anim.GetBool("IsRun"))
                {
                    moveVec = transform.rotation * Vector3.forward;
                    transform.position += moveVec * speed * Time.deltaTime;
                }
                prevInputX = curInputX;
            }
        }
    }

    void GetPCInput()
    {
        //이동 명령
        if (Input.GetMouseButtonDown(0))
        {
            prevInputX = Input.mousePosition.x;
            anim.SetBool("IsRun", true);
        }
        else if (Input.GetMouseButton(0))
        {
            curInputX = Input.mousePosition.x;
            
            //추후 감도 조절 기능을 넣는것이 좋겠다.
            lookDir -= (prevInputX - curInputX) / Screen.width * 1080.0f;
            if (lookDir < -180) lookDir += 360;
            else if (lookDir > 180) lookDir -= 360;

            transform.rotation = Quaternion.Euler(0, lookDir, 0);
            moveVec = transform.rotation * Vector3.forward;
            transform.position += moveVec * speed * Time.deltaTime;

            prevInputX = curInputX;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            anim.SetBool("IsRun", false);
            moveVec = Vector3.zero;
        }
        //방향만 변경
        else if (Input.GetMouseButtonDown(1)) prevInputX = Input.mousePosition.x;
        else if (Input.GetMouseButton(1))
        {
            curInputX = Input.mousePosition.x;

            //추후 감도 조절 기능을 넣는것이 좋겠다.
            lookDir -= (prevInputX - curInputX) / Screen.width * 1080.0f;
            if (lookDir < -180) lookDir += 360;
            else if (lookDir > 180) lookDir -= 360;

            transform.rotation = Quaternion.Euler(0, lookDir, 0);

            prevInputX = curInputX;
        }
        else if (Input.GetMouseButtonUp(1)) moveVec = Vector3.zero;

        //스킬 키
        if (Input.GetKeyUp(KeyCode.Q)) GameManager.instance.ButtonJump();
        else if (Input.GetKeyUp(KeyCode.W)) GameManager.instance.ButtonKick();
        else if (Input.GetKeyDown(KeyCode.E)) GameManager.instance.ButtonBoost(true);
        else if (Input.GetKeyUp(KeyCode.E)) GameManager.instance.ButtonBoost(false);
    }

    void MoveCamera()
    {
        Camera.main.transform.rotation = Quaternion.Euler(15.0f, lookDir, 0.0f);
        Camera.main.transform.position = transform.position + Camera.main.transform.rotation * GameManager.instance.camOffset;
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, GameManager.instance.camOffset.y, Camera.main.transform.position.z);
    }

    void SpawnEnergyNearThis()
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
                pv.RPC("RPCSpawnEnergy", RpcTarget.AllBuffered, pos, energyGenID, 3);
                energyGenID = (energyGenID + 1) % 10000;
            }
            movedDist = 0;
        }
    }

    //에너지 생성
    [PunRPC]
    void RPCSpawnEnergy(Vector3 pos, int id, int power)
    {
        Energy newEnergy = Instantiate(GameManager.instance.energyPrefab, pos, Quaternion.identity).GetComponent<Energy>();
        newEnergy.InitializeEnergy(id, power, this);
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

    public void Boost(bool isTrue)
    {
        isBoost = isTrue;
    }

    public void Kick()
    {

    }

    //오직 pv.IsMine인 경우에만 불려야 함!!
    public void ChangeScore(int _score)
    {
        score = _score;
        float clampedScore = (Mathf.Clamp(score, 50, 1000) - 50.0f) / 950.0f;
        float size = Mathf.Lerp(1.0f, 10.0f, clampedScore);

        gameObject.transform.localScale = new Vector3(size, size, size);
        baseSpeed = Mathf.Lerp(8.0f, 6.0f, clampedScore);
        jumpPower = Mathf.Lerp(7.0f, 35.0f, clampedScore);

        GameManager.instance.camOffset = new Vector3(0, size * 3.0f, size * -6.0f);
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
                ChangeScore(score + energy.power);
                energy.RemoveThisFromScene();
            }
        }
    }
}


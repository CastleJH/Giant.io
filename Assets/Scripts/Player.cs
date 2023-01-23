using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class Player : MonoBehaviour
{
    Rigidbody rigid;
    Animator anim;
    Vector3 moveVec;

    public GameObject landPoint;
    public GameObject head, feet;
    public bool isMine;
    public int score;
    float baseSpeed;
    float jumpPower;
    float speed = 20.0f;
    bool isFalling;

    float boostTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        boostTime = 99;
        score = 50;
        isFalling = false;
        ChangeSize();
    }

    void Update()
    {
        if (isMine)
        {
            Move();

            if (anim.GetBool("IsJump"))
            {
                landPoint.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                if (rigid.velocity.y < 0) isFalling = true;
            }
            Camera.main.transform.position = new Vector3(transform.position.x, GameManager.instance.camOffset.y + transform.position.y, GameManager.instance.camOffset.z + transform.position.z);
            //Camera.main.gameObject.transform.rotation = Quaternion.Euler(15, 0, Mathf.Atan2(moveVec.z, moveVec.x) * Mathf.Rad2Deg);
            //Camera.main
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
        /*(1~10 -> 50~1000)
         스코어 값을 50~1000사이로 clamp하고, 이를 1~10사이로 조정
         */

        float clampedScore = (Mathf.Clamp(score, 50, 1000) - 50.0f) / 950.0f;
        float size = Mathf.Lerp(1.0f, 10.0f, clampedScore);

        gameObject.transform.localScale = new Vector3(size, size, size);
        baseSpeed = Mathf.Lerp(8.0f, 6.0f, clampedScore);
        jumpPower = Mathf.Lerp(7.0f, 35.0f, clampedScore);

        if (isMine) 
        {
            GameManager.instance.camOffset = new Vector3(0, size * 4.0f, size * -8.0f);
        }
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
            Destroy(other.gameObject.transform.parent.gameObject);
        }
        else if (other.gameObject.tag == "Energy")
        {
            score += other.GetComponent<Energy>().power;
            Destroy(other.gameObject);
            ChangeSize();
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class Player : MonoBehaviour
{
    Rigidbody rigid;
    Animator anim;
    float speed = 3.0f;
    Vector3 moveVec;

    public GameObject landPoint;
    public Vector3 camOffset;
    public bool isPlayer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isPlayer)
        {
            moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            transform.position += moveVec * speed * Time.deltaTime;
            transform.LookAt(transform.position + moveVec);

            if (Input.GetKeyUp(KeyCode.Space) && !anim.GetBool("IsJump"))
            {
                rigid.AddForce(Vector3.up * 7, ForceMode.Impulse);
                anim.SetBool("IsJump", true);
                anim.SetTrigger("Jump");
                landPoint.SetActive(true);
            }

            Camera.main.transform.position = transform.position + camOffset;
            anim.SetBool("IsRun", moveVec != Vector3.zero);

            if (anim.GetBool("IsJump"))
            {
                landPoint.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag != "body")
        {
            anim.SetBool("IsJump", false);
            landPoint.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "head" && rigid.velocity.y < 0)
        {
            Debug.Log("Attack!");
            Destroy(other.gameObject.transform.parent.gameObject);
        }
    }
}


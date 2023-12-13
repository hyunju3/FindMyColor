using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
     
    public enum Type { 
        Color,          // 색깔
        Weapon,         // 무기
        Grenade,        // 수류탄
        Heart,          //  Hp+-
        Coin,            // 코인
        Ammo
    } 
    public Type type;
    public int value;

    float rotateSpeed = 25;

    Rigidbody rigid;
    SphereCollider sphereCollider;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag =="Floor")
        {
            // 외부 물리 효과를 적용 받지 안도록 한다
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }   
    }
}

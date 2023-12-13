using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
     
    public enum Type { 
        Color,          // ����
        Weapon,         // ����
        Grenade,        // ����ź
        Heart,          //  Hp+-
        Coin,            // ����
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
            // �ܺ� ���� ȿ���� ���� ���� �ȵ��� �Ѵ�
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }   
    }
}

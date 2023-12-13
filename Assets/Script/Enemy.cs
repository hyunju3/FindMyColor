using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C ,D}
    public Type enumyType;
    public int maxHealth;
    public int curHealth;

    // �޷��� ��ǥ����
    public Transform target;

    [Header("Melee")]
    // ������ �ڽ� �ݶ��̴�
    public BoxCollider meleeArea;

    [Header("Range")]
    // ���Ÿ� ���� ����
    public GameObject bullet;

    // �߰� ����
    public bool isChase;
    // ���� ���� ���� Ȯ��
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    // ���¿� �´� �ݶ��̴�
    public BoxCollider boxCollider;
    // ����
    public MeshRenderer[] meshs;

    public NavMeshAgent nav;

    public Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        // ���͸����� "MeshRenderer"�� �޾ƿ;����� material�� �����ü� �ִ�.
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (enumyType != Type.D)
        {
            Invoke("ChaseStart", 2f); 
        }
    }


    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {

        
        // �׺� Ȱ��ȭ��
        if (nav.enabled && enumyType != Type.D) 
        {
            nav.SetDestination(target.position);
            // �׺��� ������ ���� �ֱ�
            nav.isStopped = !isChase;
        }
    }
    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; 
        }
    }

    void Targrting()
    {
        if (!isDead && enumyType != Type.D)
        {
            float targetRadius = 0;
            float targetRange = 0;


            switch (enumyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 5f;
                    targetRange = 30f;
                    break;
            }
            RaycastHit[] rayHits =
                Physics.SphereCastAll(
                    transform.position, // �ڱ� ��ġ
                    targetRadius,       // �ݰ汸ü ������(������ �α�)
                    Vector3.forward,    // ���ư��� ���� 
                    targetRange,        // �Ÿ�(������ �ִ� ����)
                    LayerMask.GetMask("Player")); // � ���̾� ����ũ �� �� �����

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            } 
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;

        anim.SetBool("isAttack", true);

        switch (enumyType)
        {
            case Type.A: 
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                // ���� ������
                yield return new WaitForSeconds(1f);
                break;
            case Type.B:// ���� ����
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                // ���� ������
                yield return new WaitForSeconds(2f);
                break;
            case Type.C: // ���Ÿ� ���� ����
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet,transform.position,transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                // ���� ������
                yield return new WaitForSeconds(2f);
                break;
        }

        

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FixedUpdate()
    {
        Targrting();
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec,true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade = false)
    {
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red; 
        }
        yield return new WaitForSeconds(0.1f);

        if(curHealth >0)
        {
            foreach (MeshRenderer mesh in meshs)
            {
                mesh.material.color = Color.white;
            }
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
            {
                mesh.material.color = Color.gray;
            }
            // Enemy Daed ���� ���̾�� ������ �ش�
            gameObject.layer = 14;
            isDead = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");


            // �׾����� �˻��� �ֱ� ����
            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 10;

                // ȸ���������� Ǯ���ش�
                rigid.freezeRotation = false;
                rigid.AddForce(reactVec, ForceMode.Impulse);
                // ���� ���� ȸ�� ���� �ش�
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);  
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 5;
                rigid.AddForce(reactVec, ForceMode.Impulse);
            }

            if (enumyType != Type.D)
            {
                Destroy(gameObject, 4f); 
            }
        }
    }
}

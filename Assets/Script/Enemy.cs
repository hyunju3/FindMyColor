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

    // 달려갈 목표지점
    public Transform target;

    [Header("Melee")]
    // 공격할 박스 콜라이더
    public BoxCollider meleeArea;

    [Header("Range")]
    // 원거리 공격 무기
    public GameObject bullet;

    // 추격 결정
    public bool isChase;
    // 공격 설정 공유 확인
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    // 형태에 맞는 콜라이더
    public BoxCollider boxCollider;
    // 재질
    public MeshRenderer[] meshs;

    public NavMeshAgent nav;

    public Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        // 메터리얼을 "MeshRenderer"를 받아와야지만 material을 가져올수 있다.
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

        
        // 네비 활성화시
        if (nav.enabled && enumyType != Type.D) 
        {
            nav.SetDestination(target.position);
            // 네비의 움직임 정지 넣기
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
                    transform.position, // 자기 위치
                    targetRadius,       // 반경구체 반지름(레이의 두깨)
                    Vector3.forward,    // 나아가는 방향 
                    targetRange,        // 거리(레이의 최대 길이)
                    LayerMask.GetMask("Player")); // 어떤 레이어 마스크 만 검 출될지

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

                // 공격 딜레이
                yield return new WaitForSeconds(1f);
                break;
            case Type.B:// 돌격 몬스터
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                // 공격 딜레이
                yield return new WaitForSeconds(2f);
                break;
            case Type.C: // 원거리 공격 몬스터
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet,transform.position,transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                // 공격 딜레이
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
            // Enemy Daed 값의 레이어로 변경해 준다
            gameObject.layer = 14;
            isDead = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");


            // 죽었을때 넉빽을 넣기 위해
            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 10;

                // 회전값고정을 풀어준다
                rigid.freezeRotation = false;
                rigid.AddForce(reactVec, ForceMode.Impulse);
                // 죽은 적에 회전 값을 준다
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

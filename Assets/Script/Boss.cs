using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    // 미사일 프리펩 저장
    public GameObject missile;
    // 미사일 발사 위치
    public Transform missilePortA;
    public Transform missilePortB;

    // 플레이어 위치 예상 
    Vector3 lookVec;
    // 점프해서 공격 할 위치저장 
    Vector3 taunVec;

    // 점프시 방향 유지를 위한 값
    public bool isLook;


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();
            return;
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3( h, 0, v ) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
        {
            nav.SetDestination(taunVec);
        }
    }

    IEnumerator Think()
    {
        // 대기 시간(난이도 조절에 용의)
        yield return new WaitForSeconds(0.1f);

       int ranAction = Random.Range(0, 5);

        switch (ranAction)
        {
            case 0:
            case 1:
                StartCoroutine(MissileShot());
                break;
            case 2: 
            case 3:
                StartCoroutine(RockShot());
                break;
            case 4:
                StartCoroutine(Taunt());
                break;
        }
    }

    
    IEnumerator MissileShot()
    {// 미사일 발사 패턴
        anim.SetTrigger("doShot");

        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileeA = instantMissileA.GetComponent<BossMissile>();
        bossMissileeA.target = target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileeB = instantMissileB.GetComponent<BossMissile>();
        bossMissileeB.target = target;

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think());
    }

    // Rock 공격 패턴
    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());
    }

    // 점프 공격 패턴
    IEnumerator Taunt()
    {
        taunVec = target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = false;
        
        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());
    } 
    
}

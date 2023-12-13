using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    // �̻��� ������ ����
    public GameObject missile;
    // �̻��� �߻� ��ġ
    public Transform missilePortA;
    public Transform missilePortB;

    // �÷��̾� ��ġ ���� 
    Vector3 lookVec;
    // �����ؼ� ���� �� ��ġ���� 
    Vector3 taunVec;

    // ������ ���� ������ ���� ��
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
        // ��� �ð�(���̵� ������ ����)
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
    {// �̻��� �߻� ����
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

    // Rock ���� ����
    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());
    }

    // ���� ���� ����
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

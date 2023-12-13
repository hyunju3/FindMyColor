using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }
    public Type type;
    public int damage;
    // ������ ������
    public float rate;

    [Header("Melee")]
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    [Header("Range")]
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;
    public float gunSpeed = 50f;

    public int maxAmmo;
    public int curAmmo;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.9f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.2f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        // �Ѿ� �߻�
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * gunSpeed;
        yield return null;

        // ź�� ����
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.right * Random.Range(-3, -2) + Vector3.right * Random.Range(2, 3);
        // ������ �ִ� ���
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        // ȸ���� ��¦ �ֱ�
        caseRigid.AddTorque(Vector3.right * 10, ForceMode.Impulse);
    }
}

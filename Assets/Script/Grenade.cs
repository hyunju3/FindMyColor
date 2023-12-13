using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObjectPf;
    public Rigidbody rigid;

    void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(2f);
        // 이동값 없애주기 ,회전속도 없애주기
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        // 게임오브젝트 안 보이게 하기, 이펙트 보이게 하기
        meshObj.SetActive(false);

        GameObject effectObject = Instantiate(effectObjectPf, new Vector3(transform.position.x, 1, transform.position.z) ,new Quaternion(90,0,0, transform.rotation.w));
        RaycastHit[] rayHits = 
            Physics.SphereCastAll(transform.position, 15, 
            Vector3.up, 1f, LayerMask.GetMask("Enemy"));


        foreach (RaycastHit hitObj in rayHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        // 이펙트 오브젝트 파괴 (원하는 시간 후)
        Destroy(effectObject, 3f);

        Destroy(gameObject, 8f);
    }
}

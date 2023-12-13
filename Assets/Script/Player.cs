using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody rigid;

    [Header("Camera")]
    public Camera followCamera;

    [Header("Move")]
    public float speed;

    float hAxis;
    float vAxis; 
    bool wDown; // 걷기

    // 레이 값이 경계선에 닿았는지
    bool isBorder;

    Vector3 moveVec;


    [Header("Jump")]
    public float jumpForce;

    bool jDown; // 점프 
    bool isJump;


    //[Header("Dodge")]
    bool isDodge;

    Vector3 dodgeVec;

    //[Header("Animator")]
    Animator anim;


    public int score;
    [Header("Item")]

    [Header("Color")]
    public int color;
    public int maxColor;

    [Header("Ammo")]
    public int ammo;
    public int maxAmmo;

    [Header("Coin")]
    public int coin;
    public int maxCoin;

    [Header("Health")]
    public int health;
    public int maxHealth;

    [Header("Grenade")]
    public GameObject[] grenades;
    public GameObject grenadeObj;


    bool gDown;
    public int hasGrenades;
    public int maxHasGrenades;
    // 수류탄 던지는 힘
    public float grenadePower;
    GameObject nearObject;

    [Header("Weapon")]
    public Weapon equipWeapon; // 장착 상태무기
    int equipWeaponIndex = -1;

    public GameObject[] weapons;
    public bool[] hasWeapons;

    //상태값 확인
    bool isSwap;
    bool isFireReady = true;
    // 리로드 중 인가
    bool isReload;
    // 데미지 받는 중~
    bool isDamage;
    // 물건 구매중
    bool isShop;

    // 어떤장비를 장착할지 입력값
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool sDown4;

    // 공격 실행
    bool fDown;
    
    // 리로드 실행
    bool rDown;

    // 아이템 주울 때 누르는 키
    bool iDown;

    float fireDelay;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput();
        Move();
        Trun();
        Jump();
        Grenade();
        Attack();
        Reload(); 
        Dodge();
        Swap();
        Interation();
    }

    // Input 키값 받아오는 곳
    void GetInput()
    {
        // 방향키 A,W,S,D
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        // 걷기키 left shift
        wDown = Input.GetButton("Walk");
        // 점프키 space
        jDown = Input.GetButtonDown("Jump");
        // 공격키 좌 클릭
        fDown = Input.GetButton("Fire1");
        // 공격키 우 클릭
        gDown = Input.GetButton("Fire2");
        // 리로드 r
        rDown = Input.GetButtonDown("Reload");
        // 아이템 줍기 E
        iDown = Input.GetButtonDown("Interation");
        // 손에 쥔 무기 교체 키패드1,2,3,4
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        sDown4 = Input.GetButtonDown("Swap4");
    }
    
    //이동
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        // 회피시 이동방향-> 회피 방향으로
        if (isDodge) { moveVec = dodgeVec;}
        // 이동 금지
        if (isSwap || isReload || !isFireReady ){ moveVec = Vector3.zero; }

        if (!isBorder)
        {
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        }
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Trun()
    {
        // 키보드에 의한 이동축 회전
        transform.LookAt(transform.position + moveVec);

        //마우스에 의한 시점축 회전
        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        if (Physics.Raycast(ray, out rayHit, 100))
        {
            Vector3 nextVec = rayHit.point - transform.position;
            nextVec.y = 0;
            transform.LookAt(transform.position + nextVec);
        }
    }

    void Jump()
    {
        if (jDown && moveVec ==Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Grenade()
    {
        if(hasGrenades == 0) { return; }

        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = grenadePower;
                
                GameObject instantGrenade = 
                    Instantiate(grenadeObj,
                    new Vector3(transform.position.x + 0.5f, transform.position.y + 0.5f, transform.position.z), 
                    transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 1, ForceMode.Impulse);

                hasGrenades--;

                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    private void Attack()
    {
        if (equipWeapon == null) { return; }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge &&!isSwap &&!isShop)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null) { return; }
        
        if (equipWeapon.type == Weapon.Type.Melee) { return; }

        if (ammo == 0) { return; }

        if (rDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");

            isReload = true;

            Invoke("ReloadOut", 0.5f);
        }
    }

    void ReloadOut()
    {
        // 총알이 max 장전 가능한 탄창 개수 보다 작을때 그작은 값으로 장전함
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        // 장전한 총알 만큼 총알의수를 빼준다
        ammo -= reAmmo;

        isReload = false;
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            // Invoke는 시간차 함수(지속시간)
            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    // 무기 교체
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) return;
        if (sDown4 && (!hasWeapons[3] || equipWeaponIndex == 3)) return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;
        if (sDown4) weaponIndex = 3;

        if ((sDown1 || sDown2 || sDown3 || sDown4) && !isJump && !isDodge)
        {
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            weapons[weaponIndex].SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut",0.4f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }

    // e키로 아이템 줍기
    void Interation()
    {
        if (iDown && nearObject != null&& !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    // 지속적으로 회전 속도 값을 0로 입력하여 준다
    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }
    // 벽에 가까이가면 벽을 뚫어서 접근하지 않기 위해
    void StopToWall()
    {
      //Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch(item.type)
            {
                case Item.Type.Color:
                    color += item.value;
                    if (color > maxColor) 
                        color = maxColor;

                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades) 
                        hasGrenades = maxHasGrenades;

                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;

                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;

                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk)); 
            }
            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }

    // 데미지 이펙트 넣기
    // 캔버스에 -hp처리
    IEnumerator OnDamage(bool isBossAtk) 
    {
        isDamage = true;

        if (isBossAtk)
        {
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(1f);

        isDamage = false;

        if (isBossAtk)
        {
            rigid.velocity = Vector3.zero;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
        {
            nearObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
        else if(other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }
}

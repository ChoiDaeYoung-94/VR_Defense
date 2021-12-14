using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class Monster : NetworkBehaviour
{
    #region Declares
    public enum Monsters { Beholder, CrabMonster, FylingDemon, RatAssassin, ChestMonster, LizardWarrior }
    public Monsters monsters;
    public enum MonsterState { Idle, Move, Attack, Dead }
    public MonsterState monsterState = MonsterState.Move;

    [Header("Monster Information")]
    public int HP;
    [SerializeField] GameObject obj_Target;
    [SerializeField] GameObject[] obj_Targets;
    public GameObject[] obj_Target_destinations;
    [SerializeField] GameObject[] obj_Target_Players;
    float attack_;
    public float attack_Range;

    public NetworkAnimator ani;
    public NavMeshAgent agent;
    bool Check_Target;

    [Header("MonstersDeathParticles")]
    GameObject particle;
    public GameObject[] deathParticles;
    public GameObject[] hitParticles;
    public Transform Trm_hitParticle;

    [Header("AudioSource")]
    public AudioSource EffectSound;
    public AudioSource EffectSound_;
    #endregion

    #region UnityCallback
    private void Start()
    {
        PlayEffectSound_(SoundManager.instance_SM.spawn_Monster);
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCount <= 0 || GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCount >= 100)
        {
            agent.isStopped = true;
            monsterState = MonsterState.Idle;
            ani.animator.SetBool("Move", false);
            NetworkServer.Destroy(gameObject);
        }

        if (monsterState == MonsterState.Move)
        {
            FindTarget_Move();
            ani.animator.SetBool("Move", true);
        }
        else ani.animator.SetBool("Move", false);

        if (monsterState == MonsterState.Attack)
            Attack();

        if (Check_Target)
            CheckTarget();

        if (HP <= 0)
            NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == obj_Target)
        {
            Check_Target = true;
            gameObject.transform.LookAt(obj_Target.transform);
            agent.isStopped = true;
            monsterState = MonsterState.Attack;
        }
    }
    #endregion

    #region FunctionCallback   
    //플레이어나 책판 둘중 하나가 타겟이 되고 더 가까운 타겟을 찾아 공격함
    void Target_Distance()
    {
        Vector3 offset_0 = obj_Targets[0].transform.position - transform.position;
        Vector3 offset_1 = obj_Targets[1].transform.position - transform.position;

        if (offset_0.sqrMagnitude > offset_1.sqrMagnitude)
            obj_Target = obj_Targets[1];
        else
            obj_Target = obj_Targets[0];
    }

    public void FindTarget_Move()
    {
        obj_Targets = GameObject.FindGameObjectsWithTag("Target");
        obj_Target_Players = GameObject.FindGameObjectsWithTag("Player");

        if (obj_Targets.Length == 1)
            obj_Target = obj_Targets[0];

        if (obj_Targets.Length == 2)
            Target_Distance();

        if (obj_Targets.Length == 3)
        {
            Target_Distance();
            Vector3 offset_0 = obj_Target.transform.position - transform.position;
            Vector3 offset_1 = obj_Targets[2].transform.position - transform.position;
            if (offset_0.sqrMagnitude > offset_1.sqrMagnitude) obj_Target = obj_Targets[2];
        }

        if (monsters == Monsters.ChestMonster || monsters == Monsters.LizardWarrior)
        {
            agent.stoppingDistance = 2;
            obj_Target = obj_Target_Players[0];

            if (obj_Target_Players.Length == 1)
                agent.SetDestination(obj_Target_Players[0].transform.position);

            if (obj_Target_Players.Length == 2)
            {
                Vector3 offset_0 = obj_Target_Players[0].transform.position - transform.position;
                Vector3 offset_1 = obj_Target_Players[1].transform.position - transform.position;
                if (offset_0.sqrMagnitude > offset_1.sqrMagnitude)
                {
                    obj_Target = obj_Target_Players[1];
                    agent.SetDestination(obj_Target_Players[1].transform.position);
                }
                else
                    agent.SetDestination(obj_Target_Players[0].transform.position);
            }
        }
        else
            agent.SetDestination(obj_Target.transform.position);
    }

    void CheckTarget()
    {
        if (obj_Target.name == "Book Storage_Target(Clone)" && obj_Target.GetComponent<BoxCollider>().enabled == false)
        {
            Check_Target = false;
            agent.isStopped = false;
            monsterState = MonsterState.Move;
        }

        if (obj_Target.name == "Player(Clone)" && obj_Target.GetComponent<SphereCollider>().enabled == false)
        {
            Check_Target = false;
            agent.isStopped = false;
            monsterState = MonsterState.Move;
        }
    }

    //몬스터의 공격의 경우 ani를 실행하고 각 공격 ani에서 데미지가 들어가야할 즈음에 animation에서
    //데미지를 입히는 함수를 불러 공격을 실행함
    public void Attack()
    {
        if (Time.time > attack_)
        {
            attack_ = Time.time + 5f;
            if (monsters == Monsters.LizardWarrior)
            {
                if (isServer)
                {
                    int number = Random.Range(1, 4);
                    RpcLizard_(number);
                }
            }
            else
            {
                if (isServer)
                {
                    string trigger = Random.Range(0, 2) == 0 ? "Attack_1" : "Attack_2";
                    RpcOther_(trigger);
                }
            }
        }
    }

    [ClientRpc]
    void RpcLizard_(int number) => ani.animator.SetTrigger("Attack_" + number);

    [ClientRpc]
    void RpcOther_(string number) => ani.animator.SetTrigger(number);

    public void Attack_Damage_Player()
    {
        HitParticle();

        if (obj_Target.name == "Player(Clone)")
            PlayEffectSound(SoundManager.instance_SM.attacked_Player);
        else
            PlayEffectSound(SoundManager.instance_SM.attack_Monster);

        obj_Target.GetComponent<Controller_Odyssey>().TakeDamage();
    }
    public void Attack_Damage_Target()
    {
        HitParticle();

        if (obj_Target.name == "Player(Clone)")
            PlayEffectSound(SoundManager.instance_SM.attacked_Player);
        else
            PlayEffectSound(SoundManager.instance_SM.attack_Monster);

        obj_Target.GetComponent<Target>().TakeDamage();
    }

    public void DeathParticle() //죽었을때 나오는 파티클
    {
        if (monsters == Monsters.Beholder || monsters == Monsters.LizardWarrior)
            particle = Instantiate(deathParticles[Random.Range(0, deathParticles.Length)], transform.position + new Vector3(0f, 2.5f, 0f), Quaternion.Euler(-90f, 0f, 0f));

        if (monsters == Monsters.ChestMonster)
            particle = Instantiate(deathParticles[Random.Range(0, deathParticles.Length)], transform.position + new Vector3(0f, 1.5f, 0f), Quaternion.Euler(-90f, 0f, 0f));

        if (monsters == Monsters.CrabMonster || monsters == Monsters.RatAssassin)
            particle = Instantiate(deathParticles[Random.Range(0, deathParticles.Length)], transform.position + new Vector3(0f, 2.0f, 0f), Quaternion.Euler(-90f, 0f, 0f));

        if (monsters == Monsters.FylingDemon)
            particle = Instantiate(deathParticles[Random.Range(0, deathParticles.Length)], transform.position + new Vector3(0f, 3.0f, 0f), Quaternion.Euler(-90f, 0f, 0f));

        NetworkServer.Spawn(particle);
    }

    public void HitParticle() //공격시 나오는 파티클
    {
        if (isServer)
        {
            particle = Instantiate(hitParticles[Random.Range(0, hitParticles.Length)], Trm_hitParticle.position, Quaternion.identity);
            NetworkServer.Spawn(particle);
        }
    }

    public void PlayEffectSound(AudioClip clip)
    {
        EffectSound.Stop();
        EffectSound.clip = clip;
        EffectSound.Play();
    }

    public void PlayEffectSound_(AudioClip clip)
    {
        EffectSound_.Stop();
        EffectSound_.clip = clip;
        EffectSound_.Play();
    }
    #endregion
}
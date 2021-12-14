using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Mirror;
using Mirror.Discovery;
using Valve.VR;
using System.Collections;

public class NetworkManager_Defence : NetworkManager
{
    #region Declares
    [Header("PlayerSpawnPosition")]
    public Transform Right_Spawn;
    public Transform Left_Spawn;
    public Transform Single_Spawn;

    [Header("MonstersSpawnPosition")]
    public Transform[] MonsterSpawn;

    [Header("MonstersSpawnParticles")]
    public GameObject[] spawnParticles;

    [Header("PlayerGround_SpawnPosition")]
    public Transform[] PlayerGroundSpawn;

    [Header("Targets")]
    public Transform[] TargetSpawn;

    [Header("OfflineObjects")]
    public GameObject player_Offline;
    public GameObject canvasTitle_Offline;

    [Header("Discovery IP")]
    public List<string> serverIP = new List<string>();
    public NetworkDiscoveryHUD getServerIP;

    [Header("Wave")]
    [SerializeField] int waveCount = 0;
    public int monsterCount_Dead = 0;

    [Header("etc...")]
    public string gameMode = "";
    [SerializeField] public List<Controller_Odyssey> list_ControllerOdyssey = new List<Controller_Odyssey>();
    private Transform Transform_Monsters; // ParentObject of Monsters
    GameObject obj_Ready_State;
    [SerializeField] bool endGame = false;
    public bool joinHanul = false;
    #endregion

    #region UnityCallback    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Application.Quit(); //q버튼 게임 종료

        //게임 웨이브 관리
        if (SceneManager.GetActiveScene().name == "Game" && GameObject.Find("GameManager(Clone)"))
        {
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().MonsterDeath_Sync(monsterCount_Dead);
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount_Sync(waveCount);

            if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCount <= 0 && !endGame)
            {
                endGame = true;
                cancleInvoke_Monster("defeat");
                Invoke(nameof(defeat_exit), 15f);
            }

            if (gameMode == "single")
            {
                Debug.Log(monsterCount_Dead + ": monsterCount_Dead");
                if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 1 && monsterCount_Dead == 12)
                    Debug.Log("Wave_1_Clear");

                if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 2 && monsterCount_Dead == 19)
                    Debug.Log("Wave_2_Clear");

                if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 3 && monsterCount_Dead == 27)
                {
                    Debug.Log("Wave_3_Clear");
                    cancleInvoke_Monster("victory");
                    Invoke(nameof(victory_exit), 13f);
                }
            }

            if (gameMode == "multi")
            {
                Debug.Log(monsterCount_Dead + ": monsterCount_Dead");
                if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 1 && monsterCount_Dead == 18)
                    Debug.Log("Wave_1_Clear");

                if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 2 && monsterCount_Dead == 29)
                    Debug.Log("Wave_2_Clear");

                if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 3 && monsterCount_Dead == 37)
                {
                    Debug.Log("Wave_3_Clear");
                    cancleInvoke_Monster("victory");
                    Invoke(nameof(victory_exit), 13f);
                }
            }
        }
    }
    #endregion

    #region UnityMirrorCallback
    private new void Start() => InvokeRepeatIP(); //FindServerIP ing    
    public void InvokeRepeatIP() => InvokeRepeating(nameof(UpdateIP), 0f, 1f);
    void UpdateIP() => getServerIP.discoveryStart(); //연결된 Ip를 관리

    public void ChooseGameMode(string mode) //Bullet_Main Callback
    {
        gameMode = mode;
        player_Offline.SetActive(false);
        SoundManager.instance_SM.BGM.clip = SoundManager.instance_SM.BGM_2;
        SoundManager.instance_SM.BGM.Play();

        if (mode == "single")
        {
            CancelInvoke(nameof(UpdateIP));
            StartHost(); //호스트로 시작
        }

        if (mode == "multi")
        {
            serverIP.Clear();
            serverIP = getServerIP.discoveryServerIP(); //getServerIP

            Debug.Log(serverIP.Count + ": serverIP.Count");
            for (int i = 0; i < serverIP.Count; i++)
                Debug.Log(serverIP[i] + ": discovered serverIP");

            CancelInvoke(nameof(UpdateIP));

            if (serverIP.Count == 0)
            {
                StartHost();
                getServerIP.networkDiscovery.AdvertiseServer();
            }
            else
            {
                networkAddress = serverIP[0];
                StartClient(); //멀티로 시작
                               //멀티로 시작 시 클라이언트로 시작
            }
        }
    }

    //호스트나 클라이언트로 시작시 진입
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (gameMode == "single")
        {
            GameObject GM = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "GameManager"), transform.position, Quaternion.identity);
            NetworkServer.Spawn(GM);

            obj_Ready_State = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ready_State"), new Vector3(9.5f, 2f, -6.4f), Quaternion.Euler(0, -100, 0));
            NetworkServer.Spawn(obj_Ready_State);

            GameObject player = Instantiate(playerPrefab, Single_Spawn.position, Single_Spawn.rotation);
            list_ControllerOdyssey.Add(player.GetComponent<Controller_Odyssey>());

            NetworkServer.AddPlayerForConnection(conn, player);
        }

        if (gameMode == "multi") //멀티이지만 결국 2명의 플레이어가 있음
        {
            GameObject player;
            Transform start = numPlayers == 0 ? Left_Spawn : Right_Spawn;

            if (numPlayers == 0)
            {
                GameObject GM = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "GameManager"), transform.position, Quaternion.identity);
                NetworkServer.Spawn(GM);

                obj_Ready_State = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ready_State_Multi"), new Vector3(9.5f, 2f, -6.4f), Quaternion.Euler(0, -100, 0));
                NetworkServer.Spawn(obj_Ready_State);

                player = Instantiate(playerPrefab, start.position, start.rotation);
            }
            else
            {
                player = Instantiate(playerPrefab, start.position, start.rotation);
                player.GetComponent<Controller_Odyssey>().IsLeader = false;
                joinHanul = true;
            }

            list_ControllerOdyssey.Add(player.GetComponent<Controller_Odyssey>());
            obj_Ready_State.GetComponent<Ready_State>().ListUpdate(list_ControllerOdyssey);

            NetworkServer.AddPlayerForConnection(conn, player); //이것을 해야 numPlayers가 ++됨
        }
    }

    //씬이 전환될때 진입
    public override void OnServerSceneChanged(string sceneName)
    {
        //게임 씬으로 진입시 기본 세팅들
        if (sceneName.StartsWith("Game"))
        {
            endGame = false;
            GameObject target = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Book Storage_Target"), TargetSpawn[0].position, Quaternion.identity);
            NetworkServer.Spawn(target);
            GameObject canvas_Information = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Canvas_Information"), new Vector3(3.5f, 8.5f, -8.2f), Quaternion.Euler(0, -100, 0));
            NetworkServer.Spawn(canvas_Information);
            Transform_Monsters = new GameObject("Monsters").transform;

            if (gameMode == "single")
            {
                Debug.Log("Play SingleMode");
                GameObject playerGround = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "PlayerGround"), PlayerGroundSpawn[2].position, PlayerGroundSpawn[2].rotation);
                NetworkServer.Spawn(playerGround);
                Invoke(nameof(SpawnMonster_Single_1), 3f);
                Debug.Log("Wave_1_Start");
            }

            if (gameMode == "multi")
            {
                Debug.Log("Play MultiMode");
                GameObject playerGround = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "PlayerGround"), PlayerGroundSpawn[0].position, PlayerGroundSpawn[0].rotation);
                NetworkServer.Spawn(playerGround);
                GameObject playerGround_ = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "PlayerGround"), PlayerGroundSpawn[1].position, PlayerGroundSpawn[1].rotation);
                NetworkServer.Spawn(playerGround_);
                NetworkServer.Destroy(obj_Ready_State);
                Invoke(nameof(SpawnMonster_Multi_1), 3f);
                Debug.Log("Wave_1_Start");
            }
        }
    }

    public void BackButton() //SingleMode CallBack
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            canvasTitle_Offline.SetActive(true);
            Invoke(nameof(activeOfflinePlayer), 1f);
        }

        if (SceneManager.GetActiveScene().name == "Game")
        {
            monsterCount_Dead = 0;
            waveCount = 0;
            if (gameMode == "single")
            {
                CancelInvoke(nameof(SpawnMonster_Single_Wave_1));
                CancelInvoke(nameof(SpawnMonster_Single_Wave_2));
                CancelInvoke(nameof(SpawnMonster_Single_Wave_3));
            }
            if (gameMode == "multi")
            {
                CancelInvoke(nameof(SpawnMonster_Multi_Wave_1));
                CancelInvoke(nameof(SpawnMonster_Multi_Wave_2));
                CancelInvoke(nameof(SpawnMonster_Multi_Wave_3));
            }
            ServerChangeScene("Main");
        }

        InvokeRepeatIP();
        joinHanul = false;
    }

    void activeOfflinePlayer()
    {
        if (!player_Offline.activeSelf)
            player_Offline.SetActive(true);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        if (SceneManager.GetActiveScene().name == "Main")
        {
            if (list_ControllerOdyssey.Count == 1)
                list_ControllerOdyssey.Clear();

            if (list_ControllerOdyssey.Count == 2)
            {
                list_ControllerOdyssey.RemoveAt(list_ControllerOdyssey.Count - 1);
                list_ControllerOdyssey[0].isReady = false;
            }
        }

        if (SceneManager.GetActiveScene().name == "Game")
        {
            list_ControllerOdyssey.Clear();
            CancelInvoke(nameof(SpawnMonster_Single_Wave_1));
            CancelInvoke(nameof(SpawnMonster_Single_Wave_2));
            CancelInvoke(nameof(SpawnMonster_Single_Wave_3));
        }

        Debug.Log("OnServerDisconnect");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        list_ControllerOdyssey.Clear();
        BackButton();

        if (SceneManager.GetActiveScene().name == "Game")
        {
            CancelInvoke(nameof(SpawnMonster_Single_Wave_1));
            CancelInvoke(nameof(SpawnMonster_Single_Wave_2));
            CancelInvoke(nameof(SpawnMonster_Single_Wave_3));
        }

        Debug.Log("OnClientDisconnect");
    }
    #endregion

    #region FunctionCallbacks
    public void SpawnMonster_Single_1() => InvokeRepeating(nameof(SpawnMonster_Single_Wave_1), 0f, 10f);

    public void SpawnMonster_Single_Wave_1()
    {
        ++waveCount;
        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount > 1 && GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount < 4)
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Random_Audio_Narration();

        for (int i = 0; i < MonsterSpawn.Length; i++)
        {
            GameObject monster = Instantiate(spawnPrefabs[Random.Range(3, 7)], MonsterSpawn[i].position, Quaternion.identity);
            monster.transform.SetParent(Transform_Monsters);
            if (i == 0) monster.GetComponent<NavMeshAgent>().speed -= 1f;
            if (i == 1 && monster.name != "Beholder(Clone)") monster.GetComponent<NavMeshAgent>().speed -= 2f;
            NetworkServer.Spawn(monster);
            MonsterSpawnParticles(i);
        }

        if (waveCount == 4)
        {
            CancelInvoke(nameof(SpawnMonster_Single_Wave_1));
            waveCount = 0;
        }
    }

    public void SpawnMonster_Single_2()
    {
        Debug.Log("Wave_2_Start");
        StartCoroutine(Spawn_Target(1));
        InvokeRepeating(nameof(SpawnMonster_Single_Wave_2), 3f, 10f);
    }

    public void SpawnMonster_Single_Wave_2()
    {
        ++waveCount;

        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount > 1 && GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount < 5)
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Random_Audio_Narration();

        for (int i = 0; i < MonsterSpawn.Length; i++)
        {
            GameObject monster = Instantiate(spawnPrefabs[Random.Range(3, 7)], MonsterSpawn[i].position, Quaternion.identity);
            monster.transform.SetParent(Transform_Monsters);
            if (i == 0) monster.GetComponent<NavMeshAgent>().speed -= 1f;
            if (i == 1 && monster.name != "Beholder(Clone)") monster.GetComponent<NavMeshAgent>().speed -= 2f;
            NetworkServer.Spawn(monster);
            MonsterSpawnParticles(i);
        }

        if (waveCount == 1 || waveCount == 3 || waveCount == 4 || waveCount == 5)
        {
            int random_ = Random.Range(0, MonsterSpawn.Length);
            GameObject Monster = Instantiate(spawnPrefabs[7], MonsterSpawn[random_].position, Quaternion.identity);
            Monster.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster);
            MonsterSpawnParticles(random_);
        }

        if (waveCount == 5)
        {
            CancelInvoke(nameof(SpawnMonster_Single_Wave_2));
            waveCount = 0;
        }
    }

    public void SpawnMonster_Single_3()
    {
        Debug.Log("Wave_3_Start");
        StartCoroutine(Spawn_Target(2));
        InvokeRepeating(nameof(SpawnMonster_Single_Wave_3), 3f, 10f);
    }

    public void SpawnMonster_Single_Wave_3()
    {
        ++waveCount;

        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount > 1 && GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount < 7)
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Random_Audio_Narration();

        for (int i = 0; i < MonsterSpawn.Length; i++)
        {
            GameObject monster = Instantiate(spawnPrefabs[Random.Range(3, 7)], MonsterSpawn[i].position, Quaternion.identity);
            monster.transform.SetParent(Transform_Monsters);
            if (i == 0) monster.GetComponent<NavMeshAgent>().speed -= 1f;
            if (i == 1 && monster.name != "Beholder(Clone)") monster.GetComponent<NavMeshAgent>().speed -= 2f;
            NetworkServer.Spawn(monster);
            MonsterSpawnParticles(i);
        }

        if (waveCount == 1 || waveCount == 2 || waveCount == 3 || waveCount == 5 || waveCount == 6 || waveCount == 7)
        {
            int random_ = Random.Range(0, MonsterSpawn.Length);
            GameObject Monster = Instantiate(spawnPrefabs[Random.Range(7, 9)], MonsterSpawn[random_].position, Quaternion.identity);
            Monster.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster);
            MonsterSpawnParticles(random_);
        }

        if (waveCount == 7)
        {
            CancelInvoke(nameof(SpawnMonster_Single_Wave_3));
            waveCount = 0;
        }
    }

    public void SpawnMonster_Multi_1() => InvokeRepeating(nameof(SpawnMonster_Multi_Wave_1), 0f, 10f);

    public void SpawnMonster_Multi_Wave_1()
    {
        ++waveCount;
        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount > 1 && GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount < 6)
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Random_Audio_Narration();

        for (int i = 0; i < MonsterSpawn.Length; i++)
        {
            GameObject monster = Instantiate(spawnPrefabs[Random.Range(3, 7)], MonsterSpawn[i].position, Quaternion.identity);
            monster.transform.SetParent(Transform_Monsters);
            if (i == 0) monster.GetComponent<NavMeshAgent>().speed -= 1f;
            if (i == 1 && monster.name != "Beholder(Clone)") monster.GetComponent<NavMeshAgent>().speed -= 2f;
            NetworkServer.Spawn(monster);
            MonsterSpawnParticles(i);
        }

        if (waveCount == 6) //1 
        {
            CancelInvoke(nameof(SpawnMonster_Multi_Wave_1));
            waveCount = 0;
        }
    }

    public void SpawnMonster_Multi_2()
    {
        Debug.Log("Wave_2_Start");
        StartCoroutine(Spawn_Target(1));
        InvokeRepeating(nameof(SpawnMonster_Multi_Wave_2), 3f, 10f);
    }

    public void SpawnMonster_Multi_Wave_2()
    {
        ++waveCount;

        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount > 1 && GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount < 8)
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Random_Audio_Narration();

        for (int i = 0; i < MonsterSpawn.Length; i++)
        {
            GameObject monster = Instantiate(spawnPrefabs[Random.Range(3, 7)], MonsterSpawn[i].position, Quaternion.identity);
            monster.transform.SetParent(Transform_Monsters);
            if (i == 0) monster.GetComponent<NavMeshAgent>().speed -= 1f;
            if (i == 1 && monster.name != "Beholder(Clone)") monster.GetComponent<NavMeshAgent>().speed -= 2f;
            NetworkServer.Spawn(monster);
            MonsterSpawnParticles(i);
        }

        if (waveCount == 1 || waveCount == 4 || waveCount == 5 || waveCount == 7 || waveCount == 8)
        {
            int random_ = Random.Range(0, MonsterSpawn.Length - 1);
            GameObject Monster = Instantiate(spawnPrefabs[7], MonsterSpawn[random_].position, Quaternion.identity);
            Monster.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster);
            MonsterSpawnParticles(random_);
        }

        if (waveCount == 8) //1
        {
            CancelInvoke(nameof(SpawnMonster_Multi_Wave_2));
            waveCount = 0;
        }
    }

    public void SpawnMonster_Multi_3()
    {
        Debug.Log("Wave_3_Start");
        StartCoroutine(Spawn_Target(2));
        InvokeRepeating(nameof(SpawnMonster_Multi_Wave_3), 3f, 10f);
    }

    public void SpawnMonster_Multi_Wave_3()
    {
        ++waveCount;

        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount > 1 && GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveCount < 9)
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Random_Audio_Narration();

        for (int i = 0; i < MonsterSpawn.Length; i++)
        {
            GameObject monster = Instantiate(spawnPrefabs[Random.Range(3, 7)], MonsterSpawn[i].position, Quaternion.identity);
            monster.transform.SetParent(Transform_Monsters);
            if (i == 0) monster.GetComponent<NavMeshAgent>().speed -= 1f;
            if (i == 1 && monster.name != "Beholder(Clone)") monster.GetComponent<NavMeshAgent>().speed -= 2f;
            NetworkServer.Spawn(monster);
            MonsterSpawnParticles(i);
        }

        if (waveCount == 1 || waveCount == 2 || waveCount == 3 || waveCount == 5 || waveCount == 6 || waveCount == 7)
        {
            int random_ = Random.Range(0, MonsterSpawn.Length);
            GameObject Monster = Instantiate(spawnPrefabs[Random.Range(7, 9)], MonsterSpawn[random_].position, Quaternion.identity);
            Monster.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster);
            MonsterSpawnParticles(random_);
        }

        if (waveCount == 8)
        {
            GameObject Monster = Instantiate(spawnPrefabs[7], MonsterSpawn[0].position, Quaternion.identity);
            Monster.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster);
            MonsterSpawnParticles(0);

            GameObject Monster_ = Instantiate(spawnPrefabs[8], MonsterSpawn[1].position, Quaternion.identity);
            Monster_.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster_);
            MonsterSpawnParticles(1);
        }

        if (waveCount == 9)
        {
            GameObject Monster = Instantiate(spawnPrefabs[7], MonsterSpawn[1].position, Quaternion.identity);
            Monster.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster);
            MonsterSpawnParticles(1);

            GameObject Monster_ = Instantiate(spawnPrefabs[8], MonsterSpawn[0].position, Quaternion.identity);
            Monster_.transform.SetParent(Transform_Monsters);
            NetworkServer.Spawn(Monster_);
            MonsterSpawnParticles(0);

            CancelInvoke(nameof(SpawnMonster_Single_Wave_3));
            waveCount = 0;
        }
    }

    IEnumerator Spawn_Target(int position)
    {
        yield return new WaitForSeconds(3.0f);
        GameObject target = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Book Storage_Target"), TargetSpawn[position].position, Quaternion.identity);
        NetworkServer.Spawn(target);
        GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCountUp();
    }



    public void MonsterSpawnParticles(int position_Spawn)
    {
        GameObject particle = Instantiate(spawnParticles[Random.Range(0, spawnParticles.Length)],
            new Vector3(MonsterSpawn[position_Spawn].position.x, MonsterSpawn[position_Spawn].position.y - 0.2f, MonsterSpawn[position_Spawn].position.z), Quaternion.Euler(-90f, 0f, 0f));
        NetworkServer.Spawn(particle);
    }

    void defeat_exit()
    {
        if (list_ControllerOdyssey.Count > 0)
            list_ControllerOdyssey[0].ExitGame();
    }

    void victory_exit()
    {
        if (list_ControllerOdyssey.Count > 0)
            list_ControllerOdyssey[0].ExitGame();
    }

    public void cancleInvoke_Monster(string result)
    {
        if (result == "victory")
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCount = 100;

        if (result == "defeat")
            GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCount = 0;

        if (gameMode == "single")
        {
            CancelInvoke(nameof(SpawnMonster_Single_Wave_1));
            CancelInvoke(nameof(SpawnMonster_Single_Wave_2));
            CancelInvoke(nameof(SpawnMonster_Single_Wave_3));
        }
        if (gameMode == "multi")
        {
            CancelInvoke(nameof(SpawnMonster_Multi_Wave_1));
            CancelInvoke(nameof(SpawnMonster_Multi_Wave_2));
            CancelInvoke(nameof(SpawnMonster_Multi_Wave_3));
        }
    }
    #endregion
}
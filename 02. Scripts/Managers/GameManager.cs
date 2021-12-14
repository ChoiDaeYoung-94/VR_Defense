using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

/// <summary>
/// 네트워크 메니저와 상호작용함
/// 특히 네트워크 메니저의 웨이브 관리 부분을
/// 최대한 게임메니저로 가져와야함
/// 네트워크메니저 cs의 경우 NetworkManager를 
/// 상속받기에 RPC 혹은 CMD를 사용할 수 없기때문
/// </summary>
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public bool Result = false;

    [Header("Wave")]
    public int waveLevel = 1;
    public int TargetCount = 1;
    public int waveCount = 0;
    public int monsterCount_Dead_ = 0;

    [Header("AudioSources")]
    public AudioSource audio_Narration;

    [Header("AudioClips")]
    public AudioClip D_Doc_6;
    public AudioClip D_Doc_7;
    public AudioClip D_Doc_8;
    public AudioClip D_Doc_9;
    public AudioClip D_Doc_10;
    public AudioClip D_Doc_11;
    public AudioClip D_Doc_12;
    public AudioClip Narin_3_5;
    public AudioClip Narin_4_5;

    [Header("etc...")]
    public string gameMode_ = "";
    bool audio_Switch = false;
    public bool endGame = false;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        gameMode_ = GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().gameMode;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!audio_Switch)
            {
                audio_Switch = true;
                Audio_Narration(D_Doc_6);
            }

            if (TargetCount <= 0 && !endGame)
            {
                endGame = true;
                Audio_Narration(D_Doc_12);
                Invoke(nameof(Audio_defeat), 7f);
            }

            if (gameMode_ == "single")
            {
                if (waveLevel == 1 && monsterCount_Dead_ == 12)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().monsterCount_Dead = 0;
                    monsterCount_Dead_ = 0;
                    WaveLevelUp();
                    Audio_Narration(D_Doc_8);
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().SpawnMonster_Single_2();
                }

                if (waveLevel == 2 && monsterCount_Dead_ == 19)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().monsterCount_Dead = 0;
                    monsterCount_Dead_ = 0;
                    WaveLevelUp();
                    Audio_Narration(D_Doc_8);
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().SpawnMonster_Single_3();
                }

                if (waveLevel == 3 && monsterCount_Dead_ == 27)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().monsterCount_Dead = 0;
                    monsterCount_Dead_ = 0;
                    Result_(true);
                    Audio_Narration(D_Doc_11);
                    Invoke(nameof(Audio_Victory), 5f);
                }
            }

            if (gameMode_ == "multi")
            {
                if (waveLevel == 1 && monsterCount_Dead_ == 18)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().monsterCount_Dead = 0;
                    monsterCount_Dead_ = 0;
                    if (isServer) WaveLevelUp();
                    Audio_Narration(D_Doc_8);
                    if (isServer) GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().SpawnMonster_Multi_2();
                }

                if (waveLevel == 2 && monsterCount_Dead_ == 29)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().monsterCount_Dead = 0;
                    monsterCount_Dead_ = 0;
                    if (isServer) WaveLevelUp();
                    Audio_Narration(D_Doc_8);
                    if (isServer) GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().SpawnMonster_Multi_3();
                }

                if (waveLevel == 3 && monsterCount_Dead_ == 37)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().monsterCount_Dead = 0;
                    monsterCount_Dead_ = 0;
                    Result_(true);
                    Audio_Narration(D_Doc_11);
                    Invoke(nameof(Audio_Victory), 5f);
                }
            }
        }
    }

    public void MonsterDeath_Sync(int su)
    {
        if (isServer) RpcMonsterDeath_Sync(su);
    }
    [ClientRpc]
    void RpcMonsterDeath_Sync(int su) => monsterCount_Dead_ = su;

    public void waveCount_Sync(int su)
    {
        if (isServer) RpcwaveCount_Sync(su);
    }
    [ClientRpc]
    void RpcwaveCount_Sync(int su) => waveCount = su;

    public void WaveLevelUp() => RpcWaveLevelUp();
    [ClientRpc]
    void RpcWaveLevelUp() => ++waveLevel;

    public void TargetCountUp() => RpcTargetCountUp();
    [ClientRpc]
    void RpcTargetCountUp() => ++TargetCount;

    public void TargetCountDown()
    {
        if (isServer) RpcTargetCountDown();
    }
    [ClientRpc]
    void RpcTargetCountDown() => --TargetCount;

    public void Result_(bool result)
    {
        if (isServer) RpcResult_(result);
        if (isClientOnly) CmdResult_(result);
    }

    [ClientRpc]
    void RpcResult_(bool result) => Result = result;

    [Command]
    void CmdResult_(bool result) => Result = result;

    public void Random_Audio_Narration() => Rpc_Random_Audio_Narration();

    [ClientRpc]
    public void Rpc_Random_Audio_Narration()
    {
        int random_ = Random.Range(0, 2);
        if (random_ == 0) Audio_Narration(D_Doc_9);
        if (random_ == 1) Audio_Narration(D_Doc_10);
    }

    public void Audio_Victory()
    {
        GameObject.Find("Canvas_Information(Clone)").transform.GetChild(0).gameObject.SetActive(false);
        GameObject.Find("Canvas_Information(Clone)").transform.GetChild(4).gameObject.SetActive(true);
        Audio_Narration(Narin_3_5);
    }

    public void Audio_defeat()
    {
        GameObject.Find("Canvas_Information(Clone)").transform.GetChild(0).gameObject.SetActive(false);
        GameObject.Find("Canvas_Information(Clone)").transform.GetChild(3).gameObject.SetActive(true);
        Audio_Narration(Narin_4_5);
    }

    public void Audio_Narration(AudioClip clip)
    {
        audio_Narration.Stop();
        audio_Narration.clip = clip;
        audio_Narration.Play();
    }
}
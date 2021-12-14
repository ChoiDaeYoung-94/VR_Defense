using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using Valve.VR;

public class Controller_Odyssey : NetworkBehaviour
{
    #region Declares
    [Header("SteamVR")]
    public InputManager inputManager_R;
    public InputManager inputManager_L;

    [Header("Hands")]
    public GameObject hand_Left;
    public GameObject hand_Right;
    public GameObject L_hand;
    public GameObject R_hand;    

    [Header("Firing")]
    public GameObject Bullet_red;
    public GameObject Bullet_blue;
    public Transform Spawn_Bullet_R;
    public Transform Spawn_Bullet_L;

    [Header("PlayerStates")]
    public bool isReady;
    public bool IsLeader = true; //multiGame Leader(먼저 네트워크에 진입한 경우)

    [Header("PlayerInformations")]
    [SyncVar] public int Score = 0;
    [SyncVar] public int Hp_Player = 100;
    [SerializeField] Text Txt_Score;
    [SerializeField] Text Txt_Hp;
    [SerializeField] Image Img_Hp;
    [SerializeField] Text Txt_Hp_;
    [SerializeField] Image Img_Hp_;
    public static Controller_Odyssey Instance;

    [Header("Txt_Wave")]
    public GameObject wave_1;
    public GameObject wave_2;
    public GameObject wave_3;

    [Header("AudioSources")]    
    public AudioSource audio_Effect;

    [Header("AudioClips")]
    public AudioClip Shoot;    
    public AudioClip Choose;
    public AudioClip Back;
    public AudioClip GameStart;

    [Header("etc...")]
    public Material main_Skybox;
    public Material game_Skybox;
    bool switch_;
    bool switch_Hp;
    public bool gameSceneCanvasOn = false;
    public bool gameSceneCanvasText = false;
    #endregion

    #region UnityCallbacks
    private void Start()
    {           
        if (!isLocalPlayer)
            transform.GetChild(0).GetChild(2).gameObject.SetActive(false); //Camera Active Down => 자기 자신외의 다른 플레이어의 카메라를 다운
        else
        {
            if (Instance == null)
                Instance = this;
        }

        if (isServer && !isLocalPlayer) RpcIsleader();

        Invoke(nameof(Settings), 1f);
    }

    [ClientRpc]
    void RpcIsleader() => IsLeader = false;

    //나린과 한울의 총기를 구별하기 위함
    void Settings()
    {        
        if (IsLeader)
        {
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
            gameObject.transform.GetChild(2).gameObject.SetActive(true);
            gameObject.transform.GetChild(3).gameObject.SetActive(false);
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
        }
        else
        {
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
            gameObject.transform.GetChild(2).gameObject.SetActive(false);
            gameObject.transform.GetChild(3).gameObject.SetActive(true);
            gameObject.transform.GetChild(4).gameObject.SetActive(true);
            L_hand = gameObject.transform.GetChild(3).gameObject;
            R_hand = gameObject.transform.GetChild(4).gameObject;
            Spawn_Bullet_L = gameObject.transform.GetChild(3).GetChild(3);
            Spawn_Bullet_R = gameObject.transform.GetChild(4).GetChild(3);
        }

        SteamVR_Fade.View(Color.clear, 1f);
    }

    public override void OnStartClient() => DontDestroyOnLoad(gameObject);

    private void Update()
    {
        //기존의 컨트롤러가 아닌 새로운 총기를 사용하기위함 (rotation의 경우는 총을 자연스럽게 보이기 위함)
        if (isLocalPlayer)
        {            
            R_hand.transform.position = Controller_Odyssey.Instance.hand_Right.transform.position;
            R_hand.transform.rotation = Controller_Odyssey.Instance.hand_Right.transform.rotation * Quaternion.Euler(30, 0, 0);
            L_hand.transform.position = Controller_Odyssey.Instance.hand_Left.transform.position;
            L_hand.transform.rotation = Controller_Odyssey.Instance.hand_Left.transform.rotation * Quaternion.Euler(30, 0, 0);
        }

        //isLocalPlayer && inputManager_R.gunRight
        if (isLocalPlayer && inputManager_R.grabAction.GetStateDown(inputManager_R.handType) && inputManager_R.handType == SteamVR_Input_Sources.RightHand)
        {
            //연사가 가능했을때와 한발씩 나가는 차이

            //if (Time.time > FireRate_R)
            //{
            //    FireRate_R = Time.time + 0.25f;
            //}            
            CmdFire_R();
        }

        if (isLocalPlayer && inputManager_L.grabAction.GetStateDown(inputManager_L.handType) && inputManager_L.handType == SteamVR_Input_Sources.LeftHand)
        {
            //if (Time.time > FireRate_L)
            //{
            //    FireRate_L = Time.time + 0.25f;
            //}            
            CmdFire_L();
        }

        //체력이 0이되어 게임이 종료
        if (ClientScene.localPlayer.GetComponent<Controller_Odyssey>().Hp_Player <= 0 && !switch_Hp)
        {
            switch_Hp = !switch_Hp;
            if (isLocalPlayer)
            {
                if (isClientOnly)
                {
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().cancleInvoke_Monster("defeat");
                    GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Result_(false);
                    GameManager.instance.Audio_Narration(GameManager.instance.D_Doc_12);
                    Invoke(nameof(GameManager.instance.Audio_defeat), 7f);
                    Invoke(nameof(ExitGame), 15);
                    CmdGameOver();
                }
                if (isServer) RpcGameOver();
            }            
        }
        if (Hp_Player <= 0) Hp_Player = 0;

        if (SceneManager.GetActiveScene().name == "Game" && !switch_)
        {
            switch_ = !switch_;
            SteamVR_Fade.View(Color.clear, 1f);
            gameObject.transform.GetChild(0).GetChild(2).GetComponent<Skybox>().material = game_Skybox;
        }

        if (gameSceneCanvasOn)
        {
            gameSceneCanvasOn = false;

            Txt_Hp = GameObject.Find("Canvas_Information(Clone)").transform.GetChild(1).GetChild(3).gameObject.GetComponent<Text>();
            Img_Hp = GameObject.Find("Canvas_Information(Clone)").transform.GetChild(1).GetChild(1).gameObject.GetComponent<Image>();
            Txt_Hp_ = GameObject.Find("Canvas_Information(Clone)").transform.GetChild(2).GetChild(3).gameObject.GetComponent<Text>();
            Img_Hp_ = GameObject.Find("Canvas_Information(Clone)").transform.GetChild(2).GetChild(1).gameObject.GetComponent<Image>();

            if (IsLeader) Txt_Score = GameObject.Find("Canvas_Information(Clone)").transform.GetChild(1).GetChild(2).gameObject.GetComponent<Text>();
            else Txt_Score = GameObject.Find("Canvas_Information(Clone)").transform.GetChild(2).GetChild(2).gameObject.GetComponent<Text>();

            gameSceneCanvasText = true;
        }

        if (SceneManager.GetActiveScene().name == "Game" && gameSceneCanvasText)
        {
            Txt_Score.text = Score.ToString();         
            
            if (isLocalPlayer)
            {
                if (IsLeader && isServer)
                    RpcHP();
                if (!IsLeader && isClientOnly)
                {
                    Txt_Hp_.text = Hp_Player + "/100";
                    Img_Hp_.fillAmount = Hp_Player / 100f;
                    CmdHP();
                }
            }
        }

        if (SceneManager.GetActiveScene().name == "Game" && GameObject.Find("GameManager(Clone)"))
        {
            if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 1)
                wave_1.SetActive(true);
            if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 2)
                wave_2.SetActive(true);
            if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 3)
                wave_3.SetActive(true);        
        }
    }
    #endregion

    #region FunctionCallbacks
    [Command]
    void CmdGameOver()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().cancleInvoke_Monster("defeat");
        GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Result_(false);
        GameManager.instance.Audio_Narration(GameManager.instance.D_Doc_12);
        Invoke(nameof(GameManager.instance.Audio_defeat), 7f);
        Invoke(nameof(ExitGame), 15);
    }

    [ClientRpc]
    void RpcGameOver()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().cancleInvoke_Monster("defeat");
        GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().Result_(false);
        GameManager.instance.Audio_Narration(GameManager.instance.D_Doc_12);
        Invoke(nameof(GameManager.instance.Audio_defeat), 7f);
        Invoke(nameof(ExitGame), 15);
    }

    [ClientRpc]
    void RpcHP()
    {
        Txt_Hp.text = Hp_Player + "/100";
        Img_Hp.fillAmount = Hp_Player / 100f;
    }

    [Command]
    void CmdHP()
    {
        Txt_Hp_.text = Hp_Player + "/100";
        Img_Hp_.fillAmount = Hp_Player / 100f;
    }

    public void TakeDamage()
    {
        if (IsLeader)
        {
            if (isServer)
                RpcTakeDamage();
        }
        else
        {
            if (isClientOnly)
            {
                Hp_Player -= 10;
                CmdTakeDamage();                
            }
        }
    }
    
    [Command]
    void CmdTakeDamage() => Hp_Player -= 10;

    [ClientRpc]
    void RpcTakeDamage() => Hp_Player -= 10;

    [Command]
    void CmdFire_L()
    {        
        GameObject Bullet_P;
        if (IsLeader) Bullet_P = Instantiate(Bullet_red, Spawn_Bullet_L.position, Spawn_Bullet_L.rotation);
        else Bullet_P = Instantiate(Bullet_blue, Spawn_Bullet_L.position, Spawn_Bullet_L.rotation);
        Bullet_P.GetComponent<Bullet>().player = gameObject;
        NetworkServer.Spawn(Bullet_P);
    }

    [Command]
    void CmdFire_R()
    {        
        GameObject Bullet_P;
        if (IsLeader) Bullet_P = Instantiate(Bullet_red, Spawn_Bullet_R.position, Spawn_Bullet_R.rotation);
        else Bullet_P = Instantiate(Bullet_blue, Spawn_Bullet_R.position, Spawn_Bullet_R.rotation);
        Bullet_P.GetComponent<Bullet>().player = gameObject;
        NetworkServer.Spawn(Bullet_P);
    }

    public void ReadyUp() => RpcReadyUp();
    [ClientRpc]
    void RpcReadyUp()
    {
        SoundPlay_Effect(Choose);
        isReady = !isReady;
    }

    public void gameStart()
    {
        if (GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().gameMode == "single")
        {
            RpcGameStart();
            GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().ServerChangeScene("Game");
        }
        if (GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().gameMode == "multi" && IsLeader && GameObject.Find("Ready_State_Multi(Clone)").GetComponent<Ready_State>().gameStartPossible)
        {
            RpcGameStart();
            GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().ServerChangeScene("Game");
        }
    }

    [ClientRpc]
    void RpcGameStart()
    {
        SteamVR_Fade.View(Color.black, 0.5f);
        SoundManager.instance_SM.BGM.mute = true;
        SoundManager.instance_SM.BGM.clip = SoundManager.instance_SM.BGM_1;
        SoundManager.instance_SM.BGM.Play();
        SoundPlay_Effect(GameStart);
    }

    public void ChooseGame() => RpcChooseGame();

    [ClientRpc]
    void RpcChooseGame()
    {
        SoundManager.instance_SM.BGM.clip = SoundManager.instance_SM.BGM_1;
        SoundManager.instance_SM.BGM.Play();
        SoundManager.instance_SM.EffectSound.clip = Choose;
        SoundManager.instance_SM.EffectSound.Play();

        if (isServer && isLocalPlayer)
        {
            SteamVR_Fade.View(Color.black, 0.3f);
            GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().BackButton();
            GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().StopHost();
        }
        if (isClientOnly && isLocalPlayer)
        {
            SteamVR_Fade.View(Color.black, 0.3f);
            GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().BackButton();
            GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().StopClient();
        }
    }

    public void ExitGame()
    {
        if (isLocalPlayer)
        {
            if (isServer)
                RpcExitGame();

            if (isClientOnly)
            {
                SteamVR_Fade.View(Color.black, 0.5f);
                //SoundManager.instance_SM.EffectSound.clip = Back;
                //SoundManager.instance_SM.EffectSound.Play();
                SoundManager.instance_SM.BGM.mute = false;
                GameObject.Find("Maps").GetComponent<AudioSource>().mute = true;
                Invoke(nameof(ExitGame_), 1f);
                CmdExitGame();
            }
        }
    }

    [ClientRpc]
    void RpcExitGame()
    {
        SteamVR_Fade.View(Color.black, 0.5f);
        //SoundManager.instance_SM.EffectSound.clip = Back;
        //SoundManager.instance_SM.EffectSound.Play();
        SoundManager.instance_SM.BGM.mute = false;
        GameObject.Find("Maps").GetComponent<AudioSource>().mute = true;
        Invoke(nameof(ExitGame_), 1f);
    }

    [Command]
    void CmdExitGame()
    {
        SteamVR_Fade.View(Color.black, 0.5f);
        //SoundManager.instance_SM.EffectSound.clip = Back;
        //SoundManager.instance_SM.EffectSound.Play();
        SoundManager.instance_SM.BGM.mute = false;
        GameObject.Find("Maps").GetComponent<AudioSource>().mute = true;
        Invoke(nameof(ExitGame_), 1f);
    }

    void ExitGame_()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().BackButton();
        GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().StopHost();
    }

    public void SoundPlay_Effect(AudioClip clip)
    {
        audio_Effect.Stop();
        audio_Effect.clip = clip;
        audio_Effect.Play();
    }      
    #endregion
}
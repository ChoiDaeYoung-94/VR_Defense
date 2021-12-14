using UnityEngine;
using Valve.VR;

public class Controller_Odyssey_Main : MonoBehaviour
{
    //네트워크 진입 전의 플레이어의 스크립트 -> (네트워크 진입시 네트워크 속성을 가진 플레이어로 대체)
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
    public GameObject Bullet;
    public Transform Spawn_Bullet;
    public Transform Spawn_Bullet_;

    [Header("State")]
    public bool isSingle;
    public bool isMulti;

    [Header("Lines")]
    public GameObject line_1;
    public GameObject line_2;

    [Header("Narration")]
    public GameObject Image_Narration_1;
    public GameObject Image_Narration_2;

    [Header("AudioSources")]
    public AudioSource audio_Gun;
    public AudioSource audio_Effect;
    public AudioSource Narin_narration;

    [Header("AudioClips")]
    public AudioClip Shoot;
    public AudioClip Shield_Bigger;
    public AudioClip Choose;
    public AudioClip Back;
    public AudioClip GameStart;
    #endregion

    #region UnityCallbacks
    private void OnDisable()
    {
        isSingle = false;
        isMulti = false;
        //Image_Narration_1.SetActive(false);
        //Image_Narration_2.SetActive(false);
    }

    private void OnEnable()
    {
        Invoke(nameof(FadeOut_), 1f); //GameView가 아닌 Mixed Reality 포털, HMD에서 확인할 수 있는 Fade효과
        play_narration();
    }

    public void FadeOut_() => SteamVR_Fade.View(Color.clear, 1f);

    private void Start()
    {
        /*
         * 각 line은 컨트롤러에 붙어있는 레이저 (설명판에서부터 컨트롤러의 트리거 버튼까지 이어있는 레이저)
         * 추후에라도 가능하다면 동글동글하게 변경해야함
         */
        line_1.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        line_2.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        InvokeRepeating(nameof(play_narration), 30f, 30f);
        SteamVR_Fade.View(Color.clear, 1f);
    }

    private void Update()
    {
        if (inputManager_R.grabAction.GetStateDown(inputManager_R.handType) && inputManager_R.handType == SteamVR_Input_Sources.RightHand)
            Fire();

        if (inputManager_L.grabAction.GetStateDown(inputManager_L.handType) && inputManager_L.handType == SteamVR_Input_Sources.LeftHand)
            Fire_();
    }
    #endregion

    #region FunctionCallbacks
    void play_narration() => Narin_narration.Play(); //컨트롤러를 사용하여 ---- 하는 나레이션

    void Fire()
    {
        GameObject bullet = Instantiate(Bullet, Spawn_Bullet.position, Spawn_Bullet.rotation);
        bullet.GetComponent<Bullet_Main>().Player = this;
    }

    void Fire_()
    {
        GameObject bullet = Instantiate(Bullet, Spawn_Bullet_.position, Spawn_Bullet_.rotation);
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        bullet.GetComponent<Bullet_Main>().Player = this;
    }

    //싱글 게임을 시작하든 멀티 게임을 시작하든 네트워크로 진입함
    public void StartGame_Single()
    {
        SteamVR_Fade.View(Color.black, 0.5f);
        if (!isSingle)
        {
            isSingle = !isSingle;
            Invoke(nameof(singleGame), 1f);
        }
    }
    void singleGame() => GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().ChooseGameMode("single");

    public void StartGame_Multi()
    {
        SteamVR_Fade.View(Color.black, 0.5f);
        if (!isMulti)
        {
            isMulti = !isMulti;
            Invoke(nameof(multiGame), 1f);
        }
    }
    void multiGame() => GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().ChooseGameMode("multi");

    public void SoundPlay_Gun(AudioClip clip)
    {
        audio_Gun.Stop();
        audio_Gun.clip = clip;
        audio_Gun.Play();
    }

    public void SoundPlay_Effect(AudioClip clip)
    {
        audio_Effect.Stop();
        audio_Effect.clip = clip;
        audio_Effect.Play();
    }
    #endregion
}
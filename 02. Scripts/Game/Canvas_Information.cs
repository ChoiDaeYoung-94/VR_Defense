using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// 게임씬 진입 후 켄버스의 간단한 세팅
/// 싱글이면 한울켄버스의 Active를 false시키는 등의 간단한 작업들
/// </summary>
public class Canvas_Information : NetworkBehaviour
{
    NetworkManager_Defence networkManager;

    public Text Wave;
    public GameObject Hanul;
    public AudioSource audio_Information;

    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>();

        if (networkManager.gameMode == "single")
            Hanul.SetActive(false);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
            players[i].GetComponent<Controller_Odyssey>().gameSceneCanvasOn = true;
    }

    private void Update()
    {
        if (isServer)
            RpcWaveText();
    }

    [ClientRpc]
    void RpcWaveText() => Wave.text = "스테이지 " + GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel.ToString();
}

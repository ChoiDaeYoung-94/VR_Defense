using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

public class Ready_State : NetworkBehaviour
{
    public enum multiorsingle { single, multi }
    public multiorsingle multisingle;

    public GameObject Narin_;
    public GameObject Hanul_;
    public GameObject Hanul_canvas;
    public GameObject Btn_start_;
    public GameObject Btn_Back_;
    public GameObject Btn_Back_hanul;

    public bool gameStartPossible = false;

    public List<Controller_Odyssey> list_Players = new List<Controller_Odyssey>();

    private void Start()
    {
        if (isClientOnly)
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < Players.Length; i++)
                list_Players.Add(Players[i].GetComponent<Controller_Odyssey>());
        }
    }

    private void Update()
    {
        if (multisingle == multiorsingle.multi)
        {
            if (GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().numPlayers == 1)
            {
                Hanul_.SetActive(false);
                Hanul_canvas.SetActive(false);
                gameStartPossible = false;

                if (isServer)
                {
                    Btn_start_.SetActive(false);
                    Btn_Back_.GetComponent<RectTransform>().anchoredPosition = new Vector3(-2f, 0.25f, 0);
                }
            }

            if (GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().numPlayers == 2)
                RpcEnterRoom();
        }
    }

    [ClientRpc]
    void RpcEnterRoom()
    {
        Hanul_.SetActive(true);
        Hanul_canvas.SetActive(true);
        gameStartPossible = true;

        if (isServer)
        {
            Btn_start_.SetActive(true);
            Btn_Back_.GetComponent<RectTransform>().anchoredPosition = new Vector3(-0.25f, 0.25f, 0);
            Btn_Back_hanul.GetComponent<Image>().enabled = false;
        }

        if (isClientOnly)
        {
            Btn_start_.GetComponent<Image>().enabled = false;
            Btn_Back_.GetComponent<Image>().enabled = false;
        }
    }

    public void ListUpdate(List<Controller_Odyssey> list) => list_Players = list;
}

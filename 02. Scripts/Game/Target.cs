using UnityEngine;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// 타겟은각 웨이브마다 한개씩 추가되며 체력이 각기 다름
/// </summary>
public class Target : NetworkBehaviour
{
    [SyncVar] public float Hp = 0f;
    public GameObject[] cnavas_Hp;
    public GameObject[] Fires;
    public List<GameObject> Img_Hp;
    bool bool_Dead;
    bool bool_switchFire;
    public AudioSource audio_Target;

    private void Start()
    {
        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 1)
        {
            cnavas_Hp[0].SetActive(true);

            Hp = 5;
            for (int i = 0; i < cnavas_Hp[0].transform.childCount; i++)
                Img_Hp.Add(cnavas_Hp[0].transform.GetChild(i).gameObject);
            //cnavas_Hp[1].SetActive(false);
            //cnavas_Hp[2].SetActive(false);
        }

        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 2)
        {
            cnavas_Hp[1].SetActive(true);

            Hp = 4f;
            for (int i = 0; i < cnavas_Hp[1].transform.childCount; i++)
                Img_Hp.Add(cnavas_Hp[1].transform.GetChild(i).gameObject);
            //cnavas_Hp[0].SetActive(false);
            //cnavas_Hp[2].SetActive(false);
        }

        if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().waveLevel == 3)
        {
            cnavas_Hp[2].SetActive(true);

            Hp = 3f;
            for (int i = 0; i < cnavas_Hp[2].transform.childCount; i++)
                Img_Hp.Add(cnavas_Hp[2].transform.GetChild(i).gameObject);
            //cnavas_Hp[0].SetActive(false);
            //cnavas_Hp[1].SetActive(false);
        }
    }

    public void Update()
    {
        //if (Hp <= 4 && !bool_switchFire)
        //{
        //    bool_switchFire = !bool_switchFire;
        //    InvokeRepeating(nameof(Fire_Particles), 0f, Random.Range(1f, 2f));
        //}

        if (!bool_Dead && Hp <= 0)
        {
            bool_Dead = true;

            if (GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCount > 1)
                GameManager.instance.Audio_Narration(GameManager.instance.D_Doc_7);

            Dead();
        }

        if (GameManager.instance.endGame == true)
            gameObject.SetActive(false);
    }

    void Dead()
    {
        GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().TargetCountDown();
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.tag = "Untagged";
        //gameObject.SetActive(false);
    }

    public void TakeDamage()
    {
        if (isServer)
            RpcTakeDamage();
    }

    [ClientRpc]
    void RpcTakeDamage()
    {
        PlayEffectSound(SoundManager.instance_SM.attacked_Target);
        Hp -= 0.5f;
        Destroy(Img_Hp[Img_Hp.Count - 1].gameObject);
        Img_Hp.RemoveAt(Img_Hp.Count - 1);
    }

    public void PlayEffectSound(AudioClip clip)
    {
        audio_Target.Stop();
        audio_Target.clip = clip;
        audio_Target.Play();
    }

    public void Fire_Particles() //타겟이 불타오르는 파티클
    {
        GameObject fire_ = Instantiate(Fires[Random.Range(0, 3)]);
        fire_.transform.SetParent(gameObject.transform);
        fire_.transform.localPosition = new Vector3(0f, Random.Range(0.15f, 3.8f), Random.Range(-1.8f, 1.8f));
        //Destroy(fire_, Random.Range(2f, 5f));
    }
}
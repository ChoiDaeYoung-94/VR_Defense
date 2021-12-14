using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public GameObject player;
    public GameObject particle_explosion;

    public override void OnStartServer() => Invoke(nameof(DestroySelf), 2f);

    private void Start() => gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 6000);

    [Server]
    void DestroySelf() => NetworkServer.Destroy(gameObject);

    [ServerCallback]
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("monster"))
        {
            GameObject particle = Instantiate(particle_explosion, gameObject.transform.position, gameObject.transform.rotation);
            NetworkServer.Spawn(particle);
            col.gameObject.GetComponent<Monster>().HP -= 1;

            if (col.gameObject.GetComponent<Monster>().HP <= 0)
            {
                col.gameObject.tag = "Untagged";
                col.gameObject.GetComponent<Monster>().DeathParticle();
                NetworkServer.Destroy(col.gameObject);
                player.GetComponent<Controller_Odyssey>().Score += 100;
                GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>().monsterCount_Dead += 1;
                GameObject.Find("Canvas_Information(Clone)").GetComponent<Canvas_Information>().audio_Information.Play();
            }
        }

        if (col.name == "Image_Back" || col.name == "ExitGame")
        {
            GameObject particle = Instantiate(particle_explosion, gameObject.transform.position, gameObject.transform.rotation);
            NetworkServer.Spawn(particle);

            if (col.name == "Image_Back")
                player.GetComponent<Controller_Odyssey>().ChooseGame();

            if (col.name == "ExitGame")
                player.GetComponent<Controller_Odyssey>().ExitGame();
        }

        if (player.GetComponent<Controller_Odyssey>().IsLeader && col.name == "Image_Start")
        {
            col.gameObject.GetComponent<BoxCollider>().enabled = false;
            GameObject particle = Instantiate(particle_explosion, gameObject.transform.position, gameObject.transform.rotation);
            NetworkServer.Spawn(particle);
            player.GetComponent<Controller_Odyssey>().gameStart();
        }

        if (col.gameObject.layer != 8 && col.gameObject.layer != 9)
            NetworkServer.Destroy(gameObject);
    }
}
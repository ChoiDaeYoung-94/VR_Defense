using UnityEngine;

public class Bullet_Main : MonoBehaviour
{
    private GameObject canvas_Title;
    public Controller_Odyssey_Main Player;
    public GameObject particle_explosion;

    private void Start()
    {
        canvas_Title = GameObject.Find("Canvas_Title");
        Destroy(gameObject, 2f);
        gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 10000);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name == "Image_Single" || col.name == "Image_Multi")
        {
            canvas_Title.SetActive(false);
            Instantiate(particle_explosion, gameObject.transform.position, gameObject.transform.rotation);
            Player.SoundPlay_Effect(Player.Choose);

            SoundManager.instance_SM.EffectSound.clip = Player.Choose;
            SoundManager.instance_SM.EffectSound.Play();

            if (col.name == "Image_Single")
                Player.StartGame_Single();

            if (col.name == "Image_Multi")
                Player.StartGame_Multi();
        }

        //if (col.name == "Image_Exit")
        //{
        //    Instantiate(particle_explosion, gameObject.transform.position, gameObject.transform.rotation);
        //    Player.SoundPlay_Effect(Player.Choose);
        //    Application.Quit();
        //}

        if (col.name != "Bullet_Main_(Clone)") Destroy(gameObject);
    }
}
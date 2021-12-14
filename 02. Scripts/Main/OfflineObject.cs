using UnityEngine;

public class OfflineObject : MonoBehaviour
{
    public enum OfflineObjects
    {
        player_Offline,
        CanvasTitle
    }

    public OfflineObjects obj;

    NetworkManager_Defence networkManager;

    void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager_Defence>();

        if (obj == OfflineObjects.player_Offline)
            networkManager.player_Offline = gameObject;

        if (obj == OfflineObjects.CanvasTitle)
            networkManager.canvasTitle_Offline = gameObject;
    }
}

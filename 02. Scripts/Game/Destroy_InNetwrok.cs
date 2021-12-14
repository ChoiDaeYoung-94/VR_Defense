using UnityEngine;
using Mirror;

public class Destroy_InNetwrok : NetworkBehaviour
{
    public override void OnStartServer() => Invoke(nameof(DestroySelf), 2f);    

    [Server]
    void DestroySelf() => NetworkServer.Destroy(gameObject);
}

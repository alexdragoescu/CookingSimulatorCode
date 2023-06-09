using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUp : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton) Destroy(NetworkManager.Singleton.gameObject);
        if (KitchenGameMultiplayer.Instance) Destroy(KitchenGameMultiplayer.Instance.gameObject);
    }
}

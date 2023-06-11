using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playSingleplayerButton;
    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        playSingleplayerButton.onClick.AddListener(() => 
        { 
            Loader.Load(Loader.Scene.LobbyScene);
            KitchenGameMultiplayer.playMultiplayer = false;
        });

        playMultiplayerButton.onClick.AddListener(() => 
        { 
            Loader.Load(Loader.Scene.LobbyScene);
            KitchenGameMultiplayer.playMultiplayer = true;
        });

        quitButton.onClick.AddListener(() => { Application.Quit(); });
        Time.timeScale = 1f;
    }
}

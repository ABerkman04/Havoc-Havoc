using UnityEngine;
using Mirror;

public class PlayerSelection : MonoBehaviour
{
    public int chosenIndex = -1;

    public GameObject chooseCharacter;
    public GameObject multiplayerScreen;

    public void OnClickKnight()
    {
        chosenIndex = 0;
        chooseCharacter.SetActive(false);
        multiplayerScreen.SetActive(true);
    }

    public void OnClickPrincess()
    {
        chosenIndex = 1;
        chooseCharacter.SetActive(false);
        multiplayerScreen.SetActive(true);
    }

    // Call this when starting client after character is picked
    public void StartClientAndSendSelection()
    {
        NetworkManager.singleton.StartClient();

        NetworkClient.OnConnectedEvent += () =>
        {
            Debug.Log("Client connected, sending character selection.");
            NetworkClient.Send(new PlayerSelectionMessage
            {
                characterIndex = chosenIndex
            });
        };
    }
}

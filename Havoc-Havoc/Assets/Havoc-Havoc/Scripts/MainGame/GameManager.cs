using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public MainScreenUI mainScreenUI;
    public GameObject gameUI;

    [SyncVar(hook = nameof(OnPlayer1LivesChanged))]
    public int player1Lives = 3;

    [SyncVar(hook = nameof(OnPlayer2LivesChanged))]
    public int player2Lives = 3;

    public Image player1Health1, player1Health2, player1Health3;
    public Image player2Health1, player2Health2, player2Health3;


    private int player1HostID;
    private int player2ClientID;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    [Server]
    public void StartGame(int hostID, int clientID)
    {
        RpcStartGame(hostID, clientID);
    }
    [ClientRpc]
    public void RpcStartGame(int hostID, int clientID)
    {
        player1HostID = hostID;
        player2ClientID = clientID;

        Debug.Log("Start the game!");
        mainScreenUI.UIPanels.SetActive(false);
        gameUI.SetActive(true);
    }
    [Server]
    public void DamagePlayer(int playerNumber)
    {
        if (playerNumber == 1)
            player1Lives = Mathf.Max(0, player1Lives - 1);
        else if (playerNumber == 2)
            player2Lives = Mathf.Max(0, player2Lives - 1);
    }


    void OnPlayer1LivesChanged(int oldValue, int newValue)
    {
        Debug.Log("Player 1 Lives: " + newValue);

        player1Health1.enabled = newValue >= 3;
        player1Health2.enabled = newValue >= 2;
        player1Health3.enabled = newValue >= 1;
    }

    void OnPlayer2LivesChanged(int oldValue, int newValue)
    {
        Debug.Log("Player 2 Lives: " + newValue);

        player2Health1.enabled = newValue >= 3;
        player2Health2.enabled = newValue >= 2;
        player2Health3.enabled = newValue >= 1;
    }

}

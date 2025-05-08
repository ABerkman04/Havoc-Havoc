using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkManagerGame : NetworkManager
{
    private GameManager gameManager;
    public Transform[] spawnPoints;


    public static string disconnectInfo;

    public int player1HostID;
    public int player2ClientID;

    private bool isStopping = false;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        int index = numPlayers; // 0 for first, 1 for second

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && index < spawnPoints.Length)
        {
            spawnPosition = spawnPoints[index].position;
            spawnRotation = spawnPoints[index].rotation;
        }
        else
        {
            Debug.LogWarning("Not enough spawn points! Spawning at Vector3.zero.");
        }

        GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        if (numPlayers == 1)
        {
            player1HostID = 1;
        }

        if (numPlayers == 2)
        {
            player2ClientID = 2;
            Debug.Log("Both players have connected");

            // THIS is the correct way
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }
            gameManager.StartGame();
        }

    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (isStopping) return;

        disconnectInfo = "server";

        base.OnServerDisconnect(conn);
        BackToMainScreen();
        Debug.Log("OnServerDisconnect: Client disconnected.");
    }
    public override void OnClientDisconnect()
    {
        if (isStopping) return;

        disconnectInfo = "client";

        base.OnClientDisconnect();
        BackToMainScreen();
        Debug.Log("OnClientDisconnect: Server disconnected.");
    }


    public void BackToMainScreen()
    {
        if (isStopping) return;

        isStopping = true;

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }

        SceneManager.LoadScene(0);
        Time.timeScale = 1;

        isStopping = false;
    }

}

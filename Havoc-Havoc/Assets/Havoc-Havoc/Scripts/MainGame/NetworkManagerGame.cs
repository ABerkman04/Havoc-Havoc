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

    public PlayerSelection playerSelection;

    public GameObject knightPrefab;
    public GameObject princessPrefab;
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Choose distinct spawn points
        Transform spawnPoint = null;
        if (numPlayers == 0)
        {
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        }
        else if (numPlayers == 1)
        {

            (Transform p1, Transform p2) = GetTwoDistinctSpawnPoints();

            foreach (var connEntry in NetworkServer.connections)
            {
                if (connEntry.Value.identity != null)
                {
                    var p = connEntry.Value.identity.GetComponent<PlayerMovement>();
                    if (p != null && p.playerID == 1)
                    {
                        p.RpcRespawnAt(p1.position);
                    }
                }
            }

            spawnPoint = p2;
        }
        GameObject prefabToSpawn = numPlayers % 2 == 0 ? knightPrefab : princessPrefab;

        GameObject player = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        if (numPlayers == 1)
        {
            player1HostID = 1;
            playerMovement.playerID = 1;
        }
        else if (numPlayers == 2)
        {
            player2ClientID = 2;
            playerMovement.playerID = 2;

            foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
            {
                if (connection.identity != null)
                {
                    PlayerMovement pm = connection.identity.GetComponent<PlayerMovement>();
                    if (pm.playerID == 1)
                        pm.opponentID = 2;
                    else if (pm.playerID == 2)
                        pm.opponentID = 1;
                }
            }

            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }

            gameManager.StartGame(player1HostID, player2ClientID);
            FindObjectOfType<MapManager>().ActivateRandomChests();
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

        // Stop networking
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            StopClient();
        }
        else if (NetworkServer.active)
        {
            StopServer();
        }

        // Reset static/shared state
        player1HostID = 0;
        player2ClientID = 0;
        disconnectInfo = null;

        // Destroy singleton or persistent objects (if any are marked DontDestroyOnLoad)
        DestroyPersistentObjects();

        // Reset timescale and load main scene
        Time.timeScale = 1;
        SceneManager.LoadScene(0);

        isStopping = false;
    }
    void DestroyPersistentObjects()
    {
        // Example cleanup - modify based on your actual persistent objects
        var networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager != null)
            Destroy(networkManager.gameObject);

        // Add any other persistent managers here
    }




    private (Transform, Transform) GetTwoDistinctSpawnPoints()
    {
        if (spawnPoints.Length < 2)
        {
            Debug.LogError("Need at least 2 spawn points!");
            return (spawnPoints[0], spawnPoints[0]);
        }

        int firstIndex = Random.Range(0, spawnPoints.Length);
        int secondIndex;

        do
        {
            secondIndex = Random.Range(0, spawnPoints.Length);
        } while (secondIndex == firstIndex);

        return (spawnPoints[firstIndex], spawnPoints[secondIndex]);
    }


}

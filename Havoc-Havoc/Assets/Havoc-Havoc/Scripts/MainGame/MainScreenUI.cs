using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Net;
using UnityEngine.SceneManagement;

public class MainScreenUI : MonoBehaviour
{
    public GameObject UIPanels;
    public GameObject multiplayerScreen;
    public GameObject serverScreen;

    public Text serverText;
    public Text clientText;
    public Text ipText;

    public void TestGame()
    {
        OnClickHost();
        UIPanels.SetActive(false);
    }

    private void Start()
    {
        string localIPv4 = null;
        try
        {
            localIPv4 = GetLocalIPv4();
            Debug.Log("Local IPv4 Address: " + localIPv4);
            NetworkManager.singleton.networkAddress = localIPv4;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching IPv4 address: " + ex.Message);
        }

        //Update the canvas text if you have manually changed network managers address from the game object before starting the game scene
        if (NetworkManager.singleton.networkAddress != "localhost") { ipText.text = NetworkManager.singleton.networkAddress; }

        SetupCanvas();
    }

    private string GetLocalIPv4()
    {
        string localIP = "Not found";
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene(0);
    }

    public void OnClickHost()
    {
        NetworkManager.singleton.StartHost();
        SetupCanvas();
    }

    public void OnClickClient()
    {
        NetworkManager.singleton.StartClient();
        SetupCanvas();
    }

    public void OnClickClientStop()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
        SetupCanvas();
    }

    public void SetupCanvas()
    {
        // Here we will dump majority of the canvas UI that may be changed.

        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (NetworkClient.active)
            {
                multiplayerScreen.SetActive(false);
                serverScreen.SetActive(true);
                serverText.enabled = false;
                clientText.enabled = true;
                clientText.text = "Connecting to " + NetworkManager.singleton.networkAddress + "..";
            }
            else
            {
                multiplayerScreen.SetActive(true);
                serverScreen.SetActive(false);
            }
        }
        else
        {
            multiplayerScreen.SetActive(false);
            serverScreen.SetActive(true);
            serverText.enabled = true;
            clientText.enabled = true;

            // server / client status message
            if (NetworkServer.active)
            {
                serverText.text = "Server: " + Transport.active;
            }
            if (NetworkClient.isConnected)
            {
                clientText.text = "Client: " + NetworkManager.singleton.networkAddress;
            }
        }
    }
}
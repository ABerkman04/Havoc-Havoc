using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public MainScreenUI mainScreenUI;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    [Server]
    public void StartGame()
    {
        RpcStartGame();
    }
    [ClientRpc]
    public void RpcStartGame()
    {
        Debug.Log("Start the game!");
        mainScreenUI.UIPanels.SetActive(false);
    }
}

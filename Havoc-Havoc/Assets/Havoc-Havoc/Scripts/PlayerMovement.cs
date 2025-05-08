using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    //Player
    public float moveSpeed;
    public Rigidbody2D rb;

    private Vector2 moveDirection;

    //MapManager
    private MapManager mapManager;

    void Start()
    {
        if (isLocalPlayer)
        {
            mapManager = FindObjectOfType<MapManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            ProcessInputs();
            CheckForNearbyChests();
            CheckForNearbyPlayers();

        }
    }

    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            Move();
        }
    }

    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }

    void CheckForNearbyChests()
    {
        Vector3 playerWorldPosition = transform.position;
        Vector3Int cellPosition = mapManager.map.WorldToCell(playerWorldPosition);

        TileBase tile = mapManager.map.GetTile(cellPosition);

        if (tile != null && mapManager.dataFromTiles.TryGetValue(tile, out TileData tileData))
        {
            mapManager.openButton.SetActive(tileData.chest);
        }
        else
        {
            mapManager.openButton.SetActive(false);
        }
    }

    public float detectionRange = 2f; // Set how close players must be

    void CheckForNearbyPlayers()
    {
        foreach (var playerObj in FindObjectsOfType<PlayerMovement>())
        {
            if (playerObj != this) // skip self
            {
                float distance = Vector2.Distance(transform.position, playerObj.transform.position);
                if (distance <= detectionRange)
                {
                    Debug.Log("Player nearby!");
                    // You can trigger a UI prompt, outline effect, etc. here
                    return; // optional: exit early if just need one detection
                }
            }
        }
    }





}

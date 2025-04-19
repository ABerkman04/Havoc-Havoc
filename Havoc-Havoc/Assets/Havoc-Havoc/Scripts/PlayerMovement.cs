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
        if (mapManager == null) return;

        int range = mapManager.chestDetectionRange;
        Vector3Int playerGridPos = mapManager.map.WorldToCell(transform.position);

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int checkPos = new Vector3Int(playerGridPos.x + x, playerGridPos.y + y, playerGridPos.z);
                TileBase tile = mapManager.map.GetTile(checkPos);

                if (tile != null && mapManager.dataFromTiles.TryGetValue(tile, out TileData tileData))
                {
                    if (tileData.chest)
                    {
                        mapManager.openButton.SetActive(true);
                        return;
                    }
                    else
                    {
                        mapManager.openButton.SetActive(false);
                        return;
                    }
                }
            }
        }
    }



}

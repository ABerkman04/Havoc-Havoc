using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Mirror;

public class MapManager : NetworkBehaviour
{

    //Refrences
    public GameObject openButton;

    public GameObject attackButton;

    //MapManager
    [SerializeField]
    public Tilemap map;

    [SerializeField]
    private List<TileData> tileDatas;

    public Dictionary<TileBase, TileData> dataFromTiles;

    private List<Vector3Int> chestTilePositions = new List<Vector3Int>();
    private Vector3Int? currentChestTile = null;

    private Dictionary<Vector3Int, TileBase> originalChestTiles = new Dictionary<Vector3Int, TileBase>();




    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }

        // Cache all chest tile positions on the map
        BoundsInt bounds = map.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = map.GetTile(pos);
            if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData) && tileData.chest)
            {
                chestTilePositions.Add(pos);
                originalChestTiles[pos] = tile; // Store the original tile
                print(pos);
            }
        }
        foreach (Vector3Int pos in chestTilePositions)
        {
            TileBase tile = map.GetTile(pos);
            if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData))
            {
                tileData.chest = false;
            }
        }
    }
    [Server]
    public void ActivateRandomChest()
    {
        RpcActivateRandomChest();
    }

    [ClientRpc]
    public void RpcActivateRandomChest()
    {
        print(chestTilePositions.Count);
        if (chestTilePositions.Count == 0) return;

        // Hide all chests (visually and logically)
        foreach (Vector3Int pos in chestTilePositions)
        {
            if (map.HasTile(pos))
            {
                map.SetTile(pos, null); // Hide
            }

            if (originalChestTiles.TryGetValue(pos, out TileBase tile) &&
                dataFromTiles.TryGetValue(tile, out TileData tileData))
            {
                tileData.chest = false;
            }
        }

        // Pick a new chest
        int index = Random.Range(0, chestTilePositions.Count);
        Vector3Int chosenPos = chestTilePositions[index];

        // Restore visual and logic state for the active chest
        if (originalChestTiles.TryGetValue(chosenPos, out TileBase newTile) &&
            dataFromTiles.TryGetValue(newTile, out TileData newData))
        {
            map.SetTile(chosenPos, newTile); // Show only this tile
            newData.chest = true;
            currentChestTile = chosenPos;

            print("New chest is true at: " + chosenPos);
        }
    }

    public void DisableChests()
    {
        if (chestTilePositions.Count == 0) return;

        // Hide all chests (visually and logically)
        foreach (Vector3Int pos in chestTilePositions)
        {
            if (map.HasTile(pos))
            {
                map.SetTile(pos, null); // Hide
            }

            if (originalChestTiles.TryGetValue(pos, out TileBase tile) &&
                dataFromTiles.TryGetValue(tile, out TileData tileData))
            {
                tileData.chest = false;
            }
        }
    }



        public bool IsChestOpenableAt(Vector3Int pos)
    {
        return currentChestTile.HasValue && currentChestTile.Value == pos;
    }

}
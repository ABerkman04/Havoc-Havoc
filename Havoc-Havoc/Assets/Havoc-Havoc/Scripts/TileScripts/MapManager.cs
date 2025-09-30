using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Mirror;

public class MapManager : NetworkBehaviour
{
    [Header("Chest Settings")]
    [SerializeField]
    private int chestsToSpawn = 1; // Number of chests to activate


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
    private HashSet<Vector3Int> activeChestTiles = new HashSet<Vector3Int>();

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
    public void ActivateRandomChests()
    {
        RpcActivateRandomChests();
    }

    [ClientRpc]
    public void RpcActivateRandomChests()
    {
        if (chestTilePositions.Count == 0) return;

        activeChestTiles.Clear();

        // Hide all chests first
        foreach (Vector3Int pos in chestTilePositions)
        {
            map.SetTile(pos, null);

            if (originalChestTiles.TryGetValue(pos, out TileBase tile) &&
                dataFromTiles.TryGetValue(tile, out TileData tileData))
            {
                tileData.chest = false;
            }
        }

        // Pick N random unique positions
        int spawnCount = Mathf.Min(chestsToSpawn, chestTilePositions.Count);
        List<Vector3Int> shuffledPositions = new List<Vector3Int>(chestTilePositions);
        for (int i = 0; i < shuffledPositions.Count; i++)
        {
            Vector3Int temp = shuffledPositions[i];
            int randIndex = Random.Range(i, shuffledPositions.Count);
            shuffledPositions[i] = shuffledPositions[randIndex];
            shuffledPositions[randIndex] = temp;
        }

        // Activate selected chests
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3Int chosenPos = shuffledPositions[i];

            if (originalChestTiles.TryGetValue(chosenPos, out TileBase newTile) &&
                dataFromTiles.TryGetValue(newTile, out TileData newData))
            {
                map.SetTile(chosenPos, newTile);
                newData.chest = true;
                activeChestTiles.Add(chosenPos);
            }
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
        return activeChestTiles.Contains(pos);
    }


}
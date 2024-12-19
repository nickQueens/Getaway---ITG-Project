using System.Collections.Generic;
using UnityEngine;

public struct Cell
{
    public Tile tile;
    public GameObject tilePrefab;
    public List<GameObject> possibleTilePrefabs;

    public Cell(Tile tile, GameObject tilePrefab, List<GameObject> possibleTilePrefabs)
    {
        this.tile = tile;
        this.tilePrefab = tilePrefab;
        this.possibleTilePrefabs = possibleTilePrefabs;
    }
}

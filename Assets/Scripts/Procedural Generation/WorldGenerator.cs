using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Tile;

public class WorldGenerator : MonoBehaviour
{
    public float tileLength = 32.5f;
    public List<GameObject> originalTilePrefabs = new List<GameObject>();
    private List<GameObject> rotatedTilePrefabs = new List<GameObject>();

    public Vector2 gridSize;
    public Vector2 startingCell;

    private int cellsFilled = 0;

    void Start()
    {
        int numberOfCells = (int)(gridSize.x * gridSize.y);
        Cell?[][] cells = new Cell?[(int)gridSize.x][];
        for (int i = 0; i < gridSize.x; i++)
        {
            cells[i] = new Cell?[(int)gridSize.y];
        }

        Debug.Log("Rotating prefabs");
        rotatedTilePrefabs = RotateTilePrefabs(originalTilePrefabs);

        Debug.Log("Populating empty cells");
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                cells[i][j] = null;
            }
        }

        Debug.Log("Creating starting tile");
        int startingObjectIndex = Random.Range(0, rotatedTilePrefabs.Count);

        Debug.Log(rotatedTilePrefabs.Count);
        Debug.Log(startingObjectIndex);

        GameObject initialTilePrefab = rotatedTilePrefabs[startingObjectIndex];
        int startingCellX = (int)startingCell.x;
        int startingCellZ = (int)startingCell.y;
        Vector3 startingCellPosition =
                new Vector3(transform.position.x + (startingCellX * tileLength), 0, transform.position.z + (startingCellZ * tileLength));

        cells[(int)startingCell.x][(int)startingCell.y] =
            new Cell(initialTilePrefab.GetComponent<Tile>(), initialTilePrefab, new List<GameObject>());
        GameObject startingTile = Instantiate(initialTilePrefab, transform);
        startingTile.transform.position = startingCellPosition;
        startingTile.SetActive(true);
        cellsFilled++;

        Debug.Log(cells[(int)startingCell.x][(int)startingCell.y]?.tilePrefab.name);

        Debug.Log("Looping through rest of cells");

        while (cellsFilled < numberOfCells)
        {
            // Loop through all cells
            // Need to do this each time a new tile is created
            int lowerstNumberOfOptions = rotatedTilePrefabs.Count;
            Vector2 lowestOptionsCell = new Vector2(0, 0);
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (cells[i][j]?.tilePrefab != null) continue;
                    // Check neighbours to determine options for this tile

                    // Need to remove out of index values
                    // Also remove any nulls
                    // Positive x
                    TerrainType? posXTerrainType = GetNeighbouringTile(cells, i + 1, j, 0);
                    // Negative x
                    TerrainType? negXTerrainType = GetNeighbouringTile(cells, i - 1, j, 1);
                    // Positive z
                    TerrainType? posZTerrainType = GetNeighbouringTile(cells, i, j + 1, 2);
                    // Negative z
                    TerrainType? negZTerrainType = GetNeighbouringTile(cells, i, j - 1, 3);

                    if (negXTerrainType != null)
                    {
                        Debug.Log("HERE");
                        Debug.Log(negXTerrainType);
                    }

                    List<GameObject> potentialTilePrefabs = new List<GameObject>();

                    rotatedTilePrefabs.ForEach(tilePrefab =>
                    {
                        Tile currentTile = tilePrefab.GetComponent<Tile>();
                        if (
                            (posXTerrainType == currentTile.posX || posXTerrainType == null)
                            && (negXTerrainType == currentTile.negX || negXTerrainType == null)
                            && (posZTerrainType == currentTile.posZ || posZTerrainType == null)
                            && (negZTerrainType == currentTile.negZ || negZTerrainType == null)
                        )
                        {
                            potentialTilePrefabs.Add(tilePrefab);
                        }
                    });

                    // This needs to be split out
                    //GameObject chosenTilePrefab = potentialTilePrefabs[Random.Range(0, potentialTilePrefabs.Count)];

                    cells[i][j] =
                        new Cell(cells[i][j]?.tile, cells[i][j]?.tilePrefab, potentialTilePrefabs);

                    Debug.Log(potentialTilePrefabs.Count);
                    Debug.Log(lowerstNumberOfOptions);

                    if (potentialTilePrefabs.Count <= lowerstNumberOfOptions)
                    {
                        lowerstNumberOfOptions = potentialTilePrefabs.Count;
                        lowestOptionsCell = new Vector2(i, j);
                    }
                }
            }

            int newTileX = (int)lowestOptionsCell.x;
            int newTileZ = (int)lowestOptionsCell.y;
            List<GameObject> potentialPrefabs = cells[newTileX][newTileZ]?.possibleTilePrefabs;
            Debug.Log(potentialPrefabs.Count);
            GameObject chosenTilePrefab = potentialPrefabs[Random.Range(0, potentialPrefabs.Count)];
            Vector3 newTilePosition =
                new Vector3(transform.position.x + (newTileX * tileLength), 0, transform.position.z + (newTileZ * tileLength));
            GameObject newTile = Instantiate(chosenTilePrefab, transform);
            newTile.transform.position = newTilePosition;
            newTile.SetActive(true);

            cells[newTileX][newTileZ] = new Cell(chosenTilePrefab.GetComponent<Tile>(), chosenTilePrefab, new List<GameObject>());
            cellsFilled++;
        }

        Debug.Log(cells);

    }

    TerrainType? GetNeighbouringTile(Cell?[][] cells, int x, int y, int direction)
    {
        if (x >= gridSize.x || y >= gridSize.y || x < 0 || y < 0)
        {
            return null;
        }
        Tile? tile = (cells[x][y] != null) ? cells[x][y]?.tile : null;

        TerrainType? neighbouringTerrainType = null;
        switch (direction)
        {
            case 0:
                neighbouringTerrainType = tile?.negX ?? null;
                break;
            case 1:
                neighbouringTerrainType = tile?.posX ?? null;
                break;
            case 2:
                neighbouringTerrainType = tile?.negZ ?? null;
                break;
            case 3:
                neighbouringTerrainType = tile?.posZ ?? null;
                break;
        }

        Debug.Log($"{neighbouringTerrainType}");

        return neighbouringTerrainType;
    }

    List<GameObject> RotateTilePrefabs(List<GameObject> originalTilePrefabs)
    {
        List<GameObject> newRotatedTiles = new List<GameObject>();
        for (int i = 0; i < originalTilePrefabs.Count; i++)
        {
            newRotatedTiles.Add(originalTilePrefabs[i]);

            Tile originalTile = originalTilePrefabs[i].GetComponent<Tile>();
            if (originalTile.canRotate)
            {
                Debug.Log("Original Rotation");
                Debug.Log(originalTilePrefabs[i].transform.rotation.eulerAngles);
                originalTilePrefabs[i].transform.rotation = Quaternion.identity;
                // If it can rotate, then add all rotations to new list
                GameObject rotatedTile90 = Instantiate(originalTilePrefabs[i]);
                rotatedTile90.transform.Rotate(0, 90, 0, Space.Self);
                rotatedTile90.GetComponent<Tile>().RotateTerrainPositions();
                rotatedTile90.SetActive(false);
                rotatedTile90.name = originalTile.name + " 90";
                newRotatedTiles.Add(rotatedTile90);

                GameObject rotatedTile180 = Instantiate(rotatedTile90);
                rotatedTile180.transform.Rotate(0, 90, 0, Space.Self);
                rotatedTile180.GetComponent<Tile>().RotateTerrainPositions();
                rotatedTile180.SetActive(false);
                rotatedTile180.name = originalTile.name + " 180";
                newRotatedTiles.Add(rotatedTile180);

                GameObject rotatedTile270 = Instantiate(rotatedTile180);
                rotatedTile270.transform.Rotate(0, 90, 0, Space.Self);
                rotatedTile270.GetComponent<Tile>().RotateTerrainPositions();
                rotatedTile270.SetActive(false);
                rotatedTile270.name = originalTile.name + " 270";
                newRotatedTiles.Add(rotatedTile270);

                Debug.Log(originalTile.name);
                Debug.Log(originalTile.GetComponent<Tile>().posX);
                Debug.Log(rotatedTile90.GetComponent<Tile>().posX);
                Debug.Log(rotatedTile180.GetComponent<Tile>().posX);
                Debug.Log(rotatedTile270.GetComponent<Tile>().posX);

                Debug.Log("Tile rotations");
                Debug.Log(originalTilePrefabs[i].transform.rotation.eulerAngles);
                Debug.Log(rotatedTile90.transform.rotation.eulerAngles);
                Debug.Log(rotatedTile180.transform.rotation.eulerAngles);
                Debug.Log(rotatedTile270.transform.rotation.eulerAngles);
            }
        }

        return newRotatedTiles;
    }
}

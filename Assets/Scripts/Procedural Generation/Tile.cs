using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Tile;

public class Tile : MonoBehaviour
{
    public enum TerrainType
    {
        Ground,
        Road
    }

    public bool canRotate = false;

    public TerrainType posX, negX, posZ, negZ;

    public void RotateTerrainPositions()
    {
        // Assumes positive 90 degree rotation
        TerrainType prevPosX = posX;
        TerrainType prevNegX = negX;
        TerrainType prevPosZ = posZ;
        TerrainType prevNegZ = negZ;

        posX = prevPosZ;
        negX = prevNegZ;
        posZ = prevNegX;
        negZ = prevPosX;
    }
}


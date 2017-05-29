using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Type { Monk, Disciple, Temple, Influence };
public enum Color { White, Black };

public class BoardManager : MonoBehaviour
{
    private const float TILE_SIZE = 1.0f;
    private const int TILE_AMOUNT = 6;

    private int selectionX = -1;
    private int selectionY = -1;
    private float maxRayDistance = 25.0f;

    public GameObject[] piecePrefabs;
    TileManager[,] tiles = new TileManager[6, 6];
    private List<GameObject> unusedDisciples = new List<GameObject>();

    int monkWhitePosX = 1; int monkWhitePosY = 1;
    int monkBlackPosX = 4; int monkBlackPosY = 4;

    // Use this for initialization
    void Start ()
    {
        //Initiate all tiles (they are still empty)
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
            {
                Vector3 pos = transform.position + new Vector3(i * TILE_SIZE, 0, j * TILE_SIZE);
                TileManager tile = new TileManager(pos);
                tiles[i, j] = tile;
            }

        SetUpBoard();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateSelection();
        DrawBoard();

        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            int destX = monkWhitePosX;
            if (monkWhitePosX < 5) destX += 1;
            MoveMonk(Color.White, destX, monkWhitePosY);
            PlacePiece(Type.Influence, Color.Black, tiles[monkWhitePosX, monkWhitePosY]);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int destX = monkWhitePosX;
            if (monkWhitePosX > 0) destX -= 1;
            MoveMonk(Color.White, destX, monkWhitePosY);
        }
    }

    private void SetUpBoard()
    {
        //Startposition
        PlacePiece(Type.Monk, Color.White, tiles[monkWhitePosX, monkWhitePosY], 225);
        PlacePiece(Type.Monk, Color.Black, tiles[monkBlackPosX, monkBlackPosY], 45);
        PlacePiece(Type.Influence, Color.White, tiles[1, 1]);
        PlacePiece(Type.Influence, Color.Black, tiles[4, 4]);

        //Place unused disciples next to the board
        unusedDisciples.Add(Instantiate(piecePrefabs[2], transform.position + new Vector3(6.7f, 0, 0.7f), Quaternion.identity) as GameObject);
        unusedDisciples.Add(Instantiate(piecePrefabs[2], transform.position + new Vector3(6.7f, 0, 1.2f), Quaternion.identity) as GameObject);
        unusedDisciples.Add(Instantiate(piecePrefabs[2], transform.position + new Vector3(6.7f, 0, 1.7f), Quaternion.identity) as GameObject);
        unusedDisciples.Add(Instantiate(piecePrefabs[3], transform.position + new Vector3(6.7f, 0, 5.3f), Quaternion.Euler(0,180,0)) as GameObject);
        unusedDisciples.Add(Instantiate(piecePrefabs[3], transform.position + new Vector3(6.7f, 0, 4.8f), Quaternion.Euler(0, 180, 0)) as GameObject);
        unusedDisciples.Add(Instantiate(piecePrefabs[3], transform.position + new Vector3(6.7f, 0, 4.3f), Quaternion.Euler(0, 180, 0)) as GameObject);
    }

    private void MoveMonk(Color color, int destX, int destY)
    {
        //If the monk goes out of bounds, or stays put, or tries to move to the opponents tile: stop
        if (destX > 5 || destY > 5 || (monkWhitePosX == destX && monkWhitePosY == destY) || (monkBlackPosX == destX && monkBlackPosY == destY))
            return;

        int startX;
        int startY;

        if (color == Color.White)
        {
            startX = monkWhitePosX;
            startY = monkWhitePosY;
            RemovePiece(Type.Monk, color, tiles[monkWhitePosX, monkWhitePosY]);
            PlacePiece(Type.Monk, color, tiles[destX, destY], GetOrientation(startX, startY, destX, destY));
            monkWhitePosX = destX;
            monkWhitePosY = destY;
        }
        else
        {
            startX = monkBlackPosX;
            startY = monkBlackPosY;
            RemovePiece(Type.Monk, color, tiles[monkBlackPosX, monkBlackPosY]);
            PlacePiece(Type.Monk, color, tiles[destX, destY], GetOrientation(startX, startY, destX, destY));
            monkBlackPosX = destX;
            monkBlackPosY = destY;
        } 
    }

    private void PlacePiece(Type type, Color color, TileManager tile, float rotation = 0)
    {
        //Create the piece
        Piece piece = new Piece(type, color, Instantiate(piecePrefabs[PrefabID(type, color)], tile.position, Quaternion.Euler(0, rotation, 0)) as GameObject);

        //Place it on the tile
        if(tile.CanPlace(piece))
        {
            if (type == Type.Influence) tile.influence.Add(piece);
            else tile.pieces.Add(piece);
        }
        ReArrange(tile);
    }

    private void RemovePiece(Type type, Color color, TileManager tile)
    {
        foreach (Piece piece in tile.pieces)
        {
            if (piece.type == type && piece.color == color)
            {
                Destroy(piece.obj);
                tile.pieces.Remove(piece);
                return;
            }
        }
    }

    private int GetOrientation(int startX, int startY, int destX, int destY)
    {
        if (startX > destX)
        {
            if (startY == destY)
                return 90;
            if (startY < destY)
                return 135;
            if (startY > destY)
                return 45;
        }
        if (startX < destX)
        {
            if (startY == destY)
                return 270;
            if (startY < destY)
                return 225;
            if (startY > destY)
                return 315;
        }
        else
            if (startY > destY)
                return 180;
        return 0;
    }

    private void ReArrange(TileManager tile)
    {
        if (tile.pieces.Count == 0)
        {
            if (tile.influence.Count == 0) return;
            if (tile.influence.Count > 0)
                tile.influence[0].obj.transform.position = tile.position + new Vector3(0.5f * TILE_SIZE, 0, 0.5f * TILE_SIZE);
            if (tile.influence.Count > 1)
                tile.influence[1].obj.transform.position = tile.position + new Vector3(0.65f * TILE_SIZE, 0, 0.6f * TILE_SIZE);
            if (tile.influence.Count > 2)
                tile.influence[2].obj.transform.position = tile.position + new Vector3(0.67f * TILE_SIZE, 0, 0.37f * TILE_SIZE);
            if (tile.influence.Count > 3)
                tile.influence[3].obj.transform.position = tile.position + new Vector3(0.37f * TILE_SIZE, 0, 0.73f * TILE_SIZE);
            if (tile.influence.Count > 4)
                tile.influence[4].obj.transform.position = tile.position + new Vector3(0.37f * TILE_SIZE, 0, 0.35f * TILE_SIZE);
            if (tile.influence.Count > 5)
                tile.influence[5].obj.transform.position = tile.position + new Vector3(0.72f * TILE_SIZE, 0, 0.78f * TILE_SIZE);
        }

        else if (tile.pieces.Count == 1)
        {
            tile.pieces[0].obj.transform.position = tile.position + new Vector3(0.5f * TILE_SIZE, 0, 0.5f * TILE_SIZE);

            if (tile.influence.Count == 0) return;
            if (tile.influence.Count > 0)
                tile.influence[0].obj.transform.position = tile.position + new Vector3(0.8f * TILE_SIZE, 0, 0.2f * TILE_SIZE);
            if (tile.influence.Count > 1)
                tile.influence[1].obj.transform.position = tile.position + new Vector3(0.85f * TILE_SIZE, 0, 0.7f * TILE_SIZE);
            if (tile.influence.Count > 2)
                tile.influence[2].obj.transform.position = tile.position + new Vector3(0.68f * TILE_SIZE, 0, 0.89f * TILE_SIZE);
            if (tile.influence.Count > 3)
                tile.influence[3].obj.transform.position = tile.position + new Vector3(0.24f * TILE_SIZE, 0, 0.22f * TILE_SIZE);
            if (tile.influence.Count > 4)
                tile.influence[4].obj.transform.position = tile.position + new Vector3(0.12f * TILE_SIZE, 0, 0.45f * TILE_SIZE);
            if (tile.influence.Count > 5)
                tile.influence[5].obj.transform.position = tile.position + new Vector3(0.89f * TILE_SIZE, 0, 0.34f * TILE_SIZE);
        }

        else if (tile.pieces.Count == 2)
        {
            tile.pieces[0].obj.transform.position = tile.position + new Vector3(0.3f, 0, 0.3f);
            tile.pieces[1].obj.transform.position = tile.position + new Vector3(0.7f, 0, 0.7f);

            if (tile.influence.Count == 0) return;
            if (tile.influence.Count > 0)
                tile.influence[0].obj.transform.position = tile.position + new Vector3(0.8f, 0, 0.2f);
            if (tile.influence.Count > 1)
                tile.influence[1].obj.transform.position = tile.position + new Vector3(0.9f, 0, 0.3f);
            if (tile.influence.Count > 2)
                tile.influence[2].obj.transform.position = tile.position + new Vector3(0.72f, 0, 0.33f);
            if (tile.influence.Count > 3)
                tile.influence[3].obj.transform.position = tile.position + new Vector3(0.29f, 0, 0.89f);
            if (tile.influence.Count > 4)
                tile.influence[4].obj.transform.position = tile.position + new Vector3(0.36f, 0, 0.68f);
            if (tile.influence.Count > 5)
                tile.influence[5].obj.transform.position = tile.position + new Vector3(0.63f, 0, 0.1f);
        }
    }

    private int PrefabID(Type type, Color color)
    {
        int prefabId = 0;
        if (color == Color.Black) prefabId += 1;
        if (type == Type.Disciple) prefabId += 2;
        if (type == Type.Temple) prefabId += 4;
        if (type == Type.Influence)
            prefabId += 6;

        return prefabId;
    }

    private void UpdateSelection()
    {
        if (!Camera.main)
            return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, maxRayDistance, LayerMask.GetMask("BoardPlane")))
        {
            selectionX = (int)hit.point.x;
            if (selectionX > 5) selectionX = 5;
            selectionY = (int)hit.point.z;
            if (selectionY > 5) selectionY = 5;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void DrawBoard()
    {
        Vector3 widthLine = Vector3.right * TILE_AMOUNT;
        Vector3 heightLine = Vector3.forward * TILE_AMOUNT;

        for (int i = 0; i <= TILE_AMOUNT; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= TILE_AMOUNT; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }

        //Draw the selection
        if(selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }
    }
}

public class Piece
{
    public Type type;
    public Color color;
    public GameObject obj;

    public Piece(Type t, Color c, GameObject o)
    {
        type = t;
        color = c;
        obj = o;
    }
}

public class TileManager
{
    public Vector3 position;

    public List<Piece> pieces = new List<Piece>();
    public List<Piece> influence = new List<Piece>();

    public TileManager(Vector3 pos)
    {
        position = pos;
    }

    public bool CanPlace(Piece piece)
    {
        //If there already is a temple on the tile
        foreach(Piece p in pieces)
        {
            if (p.type == Type.Temple && piece.type != Type.Monk)
                return false;
            if (p.type == Type.Monk && piece.type == Type.Monk)
                return false;
            if (p.type == Type.Disciple && piece.type == Type.Disciple)
                return false;
        }
        return true;
    }
}


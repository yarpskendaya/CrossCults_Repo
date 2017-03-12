using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;
    private const int TILE_AMOUNT = 6;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> cultPrefabs;
    private List<GameObject> activeCultPieces;

    private float maxRayDistance = 25.0f;

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    private void Update()
    {
        UpdateSelection();
        DrawBoard();
    }

    private void UpdateSelection()
    {
        if (!Camera.main)
            return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, maxRayDistance, LayerMask.GetMask("BoardPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
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

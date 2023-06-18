using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace WFC
{
    public class WFC_Grid : MonoBehaviour
    {
        [SerializeField] private Vector2 GridSize = new Vector2(9, 9);

        private Dictionary<Vector2, WFC_Cell> _cells = new Dictionary<Vector2, WFC_Cell>();

        public Dictionary<Vector2, WFC_Cell> Cells => _cells;

        public void UpdateNeighbours(WFC_Cell cell)
        {
            List<int> states = cell.States;
            
            List<WFC_Cell> cells_affected = new List<WFC_Cell>();

            for (int i = 0; i < 9; i++)
            {
                Vector2 id_row = new Vector2(cell.ID.x, i);
                Vector2 id_column = new Vector2(i, cell.ID.y);

                if (_cells[id_row] == cell || _cells[id_column] == cell)
                {
                    continue;
                }

                cells_affected.Add(_cells[id_row]);
                cells_affected.Add(_cells[id_column]);
            }

            Cells.TryGetValue(cell.ID + new Vector2(1, 0), out cell);
            cell?.UpdateState(states, cells_affected);
            Cells.TryGetValue(cell.ID + new Vector2(-1, 0), out cell);
            cell?.UpdateState(states, cells_affected);
            Cells.TryGetValue(cell.ID + new Vector2(0, 1), out cell);
            cell?.UpdateState(states, cells_affected);
            Cells.TryGetValue(cell.ID + new Vector2(0, -1), out cell);
            cell?.UpdateState(states, cells_affected);
        }

        private void Start()
        {
            for (int columns = 0; columns < GridSize.y; columns++)
            {
                for (int rows = 0; rows < GridSize.x; rows++)
                {
                    Vector3 position = new Vector3((float)rows - (GridSize.x / 2), (float)columns - (GridSize.y / 2),
                        0);
                    position += new Vector3(0.5f, 0.5f, 0);

                    Vector2 id = new Vector2(rows, columns);
                    
                    WFC_Cell cell = new GameObject().AddComponent<WFC_Cell>();
                    cell.Initialize(id, this);
                    cell.transform.position = position;
                    cell.transform.parent = transform;
                    
                    _cells.Add(id, cell);
                }
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out RaycastHit hit_info))
                {
                    hit_info.collider.gameObject.GetComponent<WFC_Cell>().Collapse();
                }
            }
        }
    }
}
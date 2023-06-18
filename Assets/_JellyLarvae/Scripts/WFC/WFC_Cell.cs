using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using WFC;

//[RequireComponent(typeof(TMP_Text))]
public class WFC_Cell : MonoBehaviour
{
    private Vector2 _id;
    private List<int> _states = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private bool _collapsed = false;
    private bool _updated = false;

    public Vector2 ID => _id;
    public List<int> States => _states;
    public bool Collapsed => _collapsed;
    
    private TMP_Text Text;
    private BoxCollider Collider;
    private WFC_Grid Grid;
    
    public void Initialize(Vector2 id, WFC_Grid grid)
    {
        Text = (TMP_Text) gameObject.AddComponent(typeof(TextMeshPro));
        gameObject.name = $"{id.x},{id.y}";
        
        UpdateText();
        
        Text.fontSize = 2.5f;
        Text.rectTransform.sizeDelta = new Vector2(0.9f, 5.0f);
        Text.characterSpacing = 50.0f;
        Text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        Text.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        Collider = (BoxCollider)gameObject.AddComponent(typeof(BoxCollider));
        Grid = grid;
        
        _id = id;
    }

    public void Collapse()
    {
        if (_collapsed)
        {
            return;
        }
        
        // choose random available state to collapse 
        int new_state = _states[Random.Range(0, _states.Count)];
        
        _states.Clear();
        _states.Add(new_state);
            
        UpdateText();
        
        _collapsed = true;
        
        Grid.UpdateNeighbours(this);
    }
    
    public void UpdateState(List<int> states_to_remove, List<WFC_Cell> cells_affected)
    {
        foreach (WFC_Cell cell in cells_affected)
        {
            var diff = cell._states.Except(states_to_remove).ToList();
            cell._states = diff;
            cell.UpdateText();
        }
    }

    private void UpdateText()
    {
        string text = "";
        
        for (int i = 0; i < _states.Count; i++)
        {
            text += _states[i];
        }
        
        Text.text = text;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static JellyRenderer;

public class JellySurfaceDetection : MonoBehaviour
{
    public static JellySurfaceDetection _Instance;
    private PointInfos[] _PointInfos = Array.Empty<PointInfos>();
    public bool _IsUpdating;

    [Min(1)] public int _MaximumReader = 256;
    [SerializeField] private JellyRenderer _JellyRenderer;
    
    private Dictionary<int, JellyReader> _JellyReaders = new Dictionary<int, JellyReader>();
    private Dictionary<int, PointInfos> _ValueOfJellyReaders = new Dictionary<int, PointInfos>();

    private int _ReaderCount;
    private void Awake()
    {
        if (_Instance)
        {
            Destroy(this);
        }
        else
        {
            _Instance = this;
        }
    }

    public void AddJellyReader(int _id, Vector2 pos, Vector2 eatPos, bool _eat, float _eatRadius)
    {
        JellyReader jellyReader = new JellyReader()
        {
            id =_id, position = pos, eatPosition = eatPos, isEating = Convert.ToSingle(_eat), eatRadius = _eatRadius
        };
        PointInfos pointInfos = new PointInfos(){id = _id};
        
        _JellyReaders.Add(_id, jellyReader);
        _ValueOfJellyReaders.Add(_id, pointInfos);

        _ReaderCount++;
    }
    public void RemoveJellyReader(int id)
    {
        _JellyReaders.Remove(id);
        _ValueOfJellyReaders.Remove(id);

        _ReaderCount--;
    }
    
    public void UpdateJellyReaderPosition(int id, Vector2 pos, Vector2 eatPos)
    {
        JellyReader instance = _JellyReaders[id];
        instance.position = pos;
        instance.eatPosition = eatPos;
        
        _JellyReaders[id] = instance;
    }

    private void Update()
    {
        _IsUpdating = (_ReaderCount > 0);
        if (!_IsUpdating) return;
        UpdateJellyValue();
    }

    public void UpdateJellyValue()
    {
        _PointInfos = _JellyRenderer.GetJellyValue(ref _JellyReaders);
    }

    public float GetJellyValue(int id)
    {
        foreach (var point in _PointInfos)
        {
            if (point.id == id) return point.jellyValue;
        }

        return 0f;
    }
}

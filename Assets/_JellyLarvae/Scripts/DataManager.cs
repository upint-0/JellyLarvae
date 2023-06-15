using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager _Instance;

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

    public int GetBestScore()
    {
        return PlayerPrefs.GetInt("_PlayerBestScore");
    }

    public void SaveBestScore(int score)
    {
        PlayerPrefs.SetInt("_PlayerBestScore", score);
    }
}

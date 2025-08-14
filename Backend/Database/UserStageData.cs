using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserStageData
{
    public int stage;
    public int floor;
    public float gauge;

    public UserStageData()
    {
        stage = 1;
        floor = 1;
        gauge = 0.0f;
    }

    public UserStageData(JsonData jsonData)
    {
        stage = StringParser.ParseInt(jsonData["stage"]);
        floor = StringParser.ParseInt(jsonData["floor"]);
        gauge = StringParser.ParseFloat(jsonData["gauge"]);
    }
}
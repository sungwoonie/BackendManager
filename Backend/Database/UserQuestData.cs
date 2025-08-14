using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserQuestData
{
    public Dictionary<string, QuestServerData> questDatas;

    public UserQuestData()
    {
        questDatas = new Dictionary<string, QuestServerData>();
    }
}

[Serializable]
public class QuestServerData
{
    public DateTime startTime;
    public int count;
    public DayType dayType;
    public bool claimed;
}
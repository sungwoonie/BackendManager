using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UserServerParsingStat
{
    public Dictionary<ScriptableStatType, List<string>> serverStats;

    public UserServerParsingStat()
    {
        serverStats = new Dictionary<ScriptableStatType, List<string>>();
    }

    public void UpdateServerStatToClient()
    {
        ScriptableStatManager.instance.SetServerStats(serverStats);
    }

    public void SetServerStat(Dictionary<ScriptableStatType, List<string>> statList)
    {
        serverStats = statList;
    }
}
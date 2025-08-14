using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;

namespace StarCloudgamesLibrary
{
    [System.Serializable]
    public class UserInformationData
    {
        public string uid;
        public string nid;
        public string federationId;
        public string inDate;
        public bool adRemove;

        public int level;
        public double experiencePoint;

        public List<int> dungeonSteps;
        public Dictionary<PatrolType, KeyValuePair<string, DateTime>> patrolDatas;
        public string limitBreakRecord;
        public int[] towerOfPandaData;

        public UserInformationData()
        {
            uid = "";
            nid = "";
            federationId = "";
            inDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            adRemove = false;
            experiencePoint = 0.0;
            dungeonSteps = new List<int>(new int[Enum.GetValues(typeof(DungeonType)).Length - 1]);

            level = 1;
            patrolDatas = new Dictionary<PatrolType, KeyValuePair<string, DateTime>>();
            limitBreakRecord = "0";

            towerOfPandaData = new int[]
            {
                1, 1
            };
        }

        private void InitializeUserInformationDatas(JsonData jsonData)
        {
            level = StringParser.ParseInt(jsonData,"level", 1);
            experiencePoint = StringParser.ParseDouble(jsonData, "experiencePoint");

            dungeonSteps = StringParser.ParseIntList(jsonData, "dungeonSteps", Enum.GetValues(typeof(DungeonType)).Length - 1);

            patrolDatas = new Dictionary<PatrolType, KeyValuePair<string, DateTime>>();
            if(jsonData.ContainsKey("patrolDatas"))
            {
                var patrolJson = jsonData["patrolDatas"];
                foreach(string key in patrolJson.Keys)
                {
                    if(!Enum.TryParse(key, out PatrolType type)) continue;

                    var entry = patrolJson[key];
                    string strKey = entry["Key"].ToString();
                    string strValue = entry["Value"].ToString();

                    if(DateTime.TryParse(strValue, out DateTime dateTime))
                    {
                        patrolDatas[type] = new KeyValuePair<string, DateTime>(strKey, dateTime);
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid DateTime format in patrolDatas[{key}]");
                    }
                }
            }

            limitBreakRecord = StringParser.ParseString(jsonData, "limitBreakRecord", "0");
            towerOfPandaData = StringParser.ParseIntList(jsonData, "towerOfPandaData", ';').ToArray();

            adRemove = false;
        }

        public UserInformationData(JsonData jsonData)
        {
            uid = jsonData["gamerId"].ToString();
            federationId = jsonData["subscriptionType"].ToString();
            inDate = jsonData["inDate"].ToString();

            if(jsonData["nickname"] == null)
            {
                DebugManager.DebugInGameMessage("Nickname is not exist");
                nid = "";
            }
            else
            {
                nid = jsonData["nickname"].ToString();
            }

            InitializeUserInformationDatas(jsonData);
        }
    }
}
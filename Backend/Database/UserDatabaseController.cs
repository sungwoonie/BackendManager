using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Newtonsoft.Json;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public partial class UserDatabaseController : SingleTon<UserDatabaseController>
    {
        public UserInformationData UserInformationData;
        public UserCurrencyData UserCurrencyData;
        public UserReceiptData UserReceiptData;
        public UserRankingData UserRankingData;
        public UserEquippedData UserEquippedData;
        public UserStageData UserStageData;
        public UserServerParsingStat UserServerParsingStat;
        public UserQuestData UserQuestData;

        #region "Unity"

        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        #endregion

        #region "Initialize"

        private void Initialize()
        {
            UserInformationData = new UserInformationData();

            UserRankingData = new UserRankingData();

            UserReceiptData = new UserReceiptData()
            {
                ReceiptList = new List<ServerReceipt>()
            };

            UserCurrencyData = new UserCurrencyData();
            UserCurrencyData.InitializeCurrencyList();

            UserEquippedData = new UserEquippedData();
            UserEquippedData.Initialize();

            UserStageData = new UserStageData();

            UserServerParsingStat = new UserServerParsingStat();
            UserQuestData = new UserQuestData();
        }

        #endregion

        #region "Set"

        public void OverwriteUserDatabaseController(JsonData jsonData)
        {
            var typeMap = new Dictionary<string, Action<JsonData>>
            {
                { "UserInformationData", data => UserInformationData = JsonConvert.DeserializeObject<UserInformationData>(data.ToJson()) },
                { "UserCurrencyData", data => UserCurrencyData = JsonConvert.DeserializeObject<UserCurrencyData>(data.ToJson()) },
                { "UserReceiptData", data => UserReceiptData = JsonConvert.DeserializeObject<UserReceiptData>(data.ToJson()) },
                { "UserRankingData", data => UserRankingData = JsonConvert.DeserializeObject<UserRankingData>(data.ToJson()) },
                { "UserEquippedData", data => UserEquippedData = JsonConvert.DeserializeObject<UserEquippedData>(data.ToJson()) },
                { "UserStageData", data => UserStageData = JsonConvert.DeserializeObject<UserStageData>(data.ToJson()) },
                { "UserServerParsingStat", (data) =>
                {
                    UserServerParsingStat = JsonConvert.DeserializeObject<UserServerParsingStat>(data.ToJson());
                    UserServerParsingStat.UpdateServerStatToClient();
                }},
                { "UserQuestData", data => UserQuestData = JsonConvert.DeserializeObject<UserQuestData>(data.ToJson())}
            }; 

            for (int i = 0; i < jsonData.Count; i++)
            {
                if (jsonData[i].IsObject)
                {
                    foreach (var key in jsonData[i].Keys)
                    {
                        if (typeMap.TryGetValue(key, out var setter))
                        {
                            setter(jsonData[i][key]);
                        }
                        else
                        {
                            Debug.LogWarning($"Unknown key in user data: {key}");
                        }
                    }
                }
            }
        }

        #endregion

        #region "Save Data"

        public void SaveAllDataToServer()
        {
            UserServerParsingStat.SetServerStat(ScriptableStatManager.instance.GetServerStats());

            BackendManager.instance.UpdateAllDataToServer();
        }

        public void SaveDataToServer()
        {//여기서 최신화 후 저장 진행
            UserServerParsingStat.SetServerStat(ScriptableStatManager.instance.GetServerStats());
            BackendManager.instance.UpdateSingleData(UserServerParsingStat, out var inging);
            Debug.Log(inging);
        }

        #endregion
    }
}
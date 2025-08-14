using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Newtonsoft.Json;
using System.Reflection;

namespace StarCloudgamesLibrary
{
    public partial class BackendManager
    {
        public UserDatabaseController userDatabaseController;

        private List<string> tableNames;
        private Dictionary<string, FieldInfo> dataFields;

        #region "Initialize"

        public void InitializeDatabase()
        {
            userDatabaseController ??= gameObject.AddComponent<UserDatabaseController>();
            GetUserDatabaseFromServer();
        }

        #endregion

        #region "Nick Name"

        private void CheckNID()
        {
            if (string.IsNullOrEmpty(userDatabaseController.UserInformationData.nid))
            {
                DebugManager.DebugInGameMessage("Nickname is not exist. Set NID");
                UIManager.instance.GetUI(UIType.UINicknamePopup).Open();
            }
            else
            {
                SceneLoadManager.LoadScene(SceneName.MainScene, true);
            }
        }

        public bool CheckNIDDuplication(string nid, out BackendReturnObject callback)
        {
            callback = Backend.BMember.CheckNicknameDuplication(nid);

            if (callback.StatusCode == 204)
            {
                DebugManager.DebugServerMessage($"Can Set NID to {nid}");
                return true;
            }
            else
            {
                DebugManager.DebugServerMessage($"{nid} is duplicated! erorr : {callback}");
                return false;
            }
        }

        public bool SetNewNicknameSuccess(string nid)
        {
            var callback = Backend.BMember.CreateNickname(nid);

            if(callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Set NID to {nid} is success!");
                userDatabaseController.UserInformationData.nid = nid;
                return true;
            }
            else
            {
                DebugManager.DebugServerMessage($"set NID to {nid} is failed! erorr : {callback}");

                if(ItsServerError(callback))
                {
                    DirectErrorHandler(callback, null);
                }
                else
                {
                    if(callback.StatusCode == 409)
                    {
                        UIManager.instance.ToastMessage(LocalizationManager.instance.GetLocalizedString("NicknameDuplicateToastMessage"));
                    }
                }
            }

            return false;
        }

        #endregion

        #region "Set Data"

        private void SetUserDataByServer(JsonData jsonData)
        {
            userDatabaseController.OverwriteUserDatabaseController(jsonData);

            CheckNID();
        }

        #endregion

        #region "Write Data"

        private void WriteNewData()
        {
            var userInformationData = GetUserInformationData();
            if(userInformationData == null)
            {
                UIManager.instance.SystemMessage(true, "RestartGame", "RestartGame_ServerError", "Quit", Application.Quit);
                return;
            }

            userDatabaseController.UserInformationData = new UserInformationData(userInformationData);

            tableNames ??= TableNames();

            var transactionValue = GetWriteTransactionValuesByName(tableNames);
            var callback = Backend.GameData.TransactionWriteV2(transactionValue);

            if(callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Insert User Data Success!");
                CheckNID();
            }
            else
            {
                DebugManager.DebugServerMessage($"Insert User Data Failed! erorr : {callback}");
                DirectErrorHandler(callback, null);
            }
        }

        #endregion

        #region "Get Data From Server"

        private JsonData GetUserInformationData()
        {
            var callback = Backend.BMember.GetUserInfo();
            
            if (callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Get User Information Success!. {callback}");
                return callback.GetReturnValuetoJSON()["row"];
            }
            else
            {
                DebugManager.DebugInGameErrorMessage($"Get User Information Failed!. {callback}");
                return null;
            }
        }

        private void GetUserDatabaseFromServer()
        {
            tableNames ??= TableNames();

            var transactionValues = GetReadTransactionValuesByName(tableNames);
            var callback = Backend.GameData.TransactionReadV2(transactionValues);

            if(callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Get User Data Success!. start initialize data");
                SetUserDataByServer(callback.GetFlattenJSON()["Responses"]);
            }
            else
            {
                if(ItsServerError(callback))
                {
                    DirectErrorHandler(callback, null);
                }
                else if(callback.StatusCode == 404)
                {
                    WriteNewData();
                }
            }
        }

        #endregion

        #region "Table Name"

        private List<string> TableNames()
        {
            dataFields = new Dictionary<string, FieldInfo>();

            var tableNames = new List<string>();
            var callBack = Backend.GameData.GetTableList();

            if (callBack.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Get Table Names Success!");

                var dataBase = typeof(UserDatabaseController);

                JsonData tableListJson = callBack.GetReturnValuetoJSON()["tables"];
                for (int i = 0; i < tableListJson.Count; i++)
                {
                    var tableName = tableListJson[i]["tableName"].ToString();
                    tableNames.Add(tableName);

                    var newField = dataBase.GetField(tableName);
                    if(newField != null)
                    {
                        dataFields[tableName] = newField;
                    }

                    DebugManager.DebugServerMessage($"Table name {i} : {tableListJson[i]["tableName"]}");
                }

                return tableNames;
            }
            else
            {
                DebugManager.DebugServerMessage($"Get Table Names Failed! erorr : {callBack}");
                return null;
            }
        }

        #endregion

        #region "Transaction"

        private List<TransactionValue> GetReadTransactionValuesByName(List<string> tableNames)
        {
            var transactionList = new List<TransactionValue>();

            for (int i = 0; i < tableNames.Count; i++)
            {
                if(dataFields.ContainsKey(tableNames[i]))
                {
                    transactionList.Add(TransactionValue.SetGet(tableNames[i], new Where()));
                }
            }

            return transactionList;
        }

        private List<TransactionValue> GetWriteTransactionValuesByName(List<string> tableNames)
        {
            var transactionList = new List<TransactionValue>();

            for (int i = 0; i < tableNames.Count; i++)
            {
                var newParam = GetParam(tableNames[i]);
                if(newParam == null)
                {
                    Debug.Log(tableNames[i] + "is not exist");
                    continue;
                }

                transactionList.Add(TransactionValue.SetInsert(tableNames[i], newParam));
            }

            return transactionList;
        }

        private List<TransactionValue> GetUpdateTransactionValuesByName(List<string> tableNames)
        {
            var transactionList = new List<TransactionValue>();

            for(int i = 0; i < tableNames.Count; i++)
            {
                var newParam = GetParam(tableNames[i]);
                if(newParam == null)
                {
                    Debug.Log(tableNames[i] + "is not exist");
                    continue;
                }

                transactionList.Add(TransactionValue.SetUpdate(tableNames[i], new Where(), newParam));
            }

            return transactionList;
        }

        #endregion

        #region "Update Data"

        public void UpdateAllDataToServer()
        {
            var transactionValues = GetUpdateTransactionValuesByName(tableNames);
            Backend.GameData.TransactionWriteV2(transactionValues, (callback) =>
            {
                if (callback.IsSuccess())
                {
                    DebugManager.DebugServerMessage($"Update All Data Success!");
                }
                else
                {
                    DebugManager.DebugServerErrorMessage($"Update All Data Failed! error : {callback}");
                }
            });
        }

        public bool UpdateMultipleDataByTableName(List<string> targetTables, out BackendReturnObject callback)
        {
            callback = null;
            foreach(var targetTable in targetTables)
            {
                if(!tableNames.Contains(targetTable))
                {
                    DebugManager.DebugServerErrorMessage($"{targetTable} is not exist!!!!");
                    return false;
                }
            }

            DebugManager.DebugServerMessage("Start Update Multiple Data");
            targetTables.ForEach(x => DebugManager.DebugServerMessage($"Target Data table : {x}"));

            callback = Backend.GameData.TransactionWriteV2(GetUpdateTransactionValuesByName(targetTables));

            if(callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Update success!");

                return true;
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Update failed!. error : {callback}");
                return false;
            }
        }

        public bool UpdateSingleData(string tableName, out BackendReturnObject callback)
        {
            callback = null;

            if(!tableNames.Contains(tableName))
            {
                DebugManager.DebugServerErrorMessage($"{tableName} is not exist.");
                return false;
            }

            DebugManager.DebugServerMessage($"Start Update Single Data. Target Table : {tableName}");

            var newParam = GetParam(tableName);
            if(newParam == null)
            {
                return false;
            }

            callback = Backend.PlayerData.UpdateMyLatestData(tableName, newParam);

            if(callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Update {tableName} is success!");
                return true;
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Update {tableName} is failed!. error : {callback}");
                return false;
            }
        }

        public bool UpdateSingleData<T>(T data, out BackendReturnObject callback)
        {
            var tableName = data.GetType().Name;
            callback = null;

            if (!tableNames.Contains(tableName))
            {
                DebugManager.DebugServerErrorMessage($"{tableName} is not exist.");
                return false;
            }

            DebugManager.DebugServerMessage($"Start Update Single Data. Target data : {data}");

            var newParam = GetParam(tableName);
            if(newParam == null)
            {
                return false;
            }

            callback = Backend.PlayerData.UpdateMyLatestData(tableName, newParam);

            if (callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Update {data} is success!");
                return true;
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Update {data} is failed!. error : {callback}");
                return false;
            }
        }

        #endregion

        #region "Param"

        public Param GetParam(string name)
        {
            if(name.Equals("UserRankingData"))
            {
                return new Param
                {
                    { "highScore", userDatabaseController.UserRankingData.highScore }
                };
            }
            else
            {
                if(dataFields.ContainsKey(name))
                {
                    var newData = dataFields[name].GetValue(userDatabaseController);

                    if(newData == null)
                    {
                        return null;
                    }

                    return new Param
                    {
                        { name, newData }
                    };
                }
            }

            return null;
        }

        #endregion
    }
}
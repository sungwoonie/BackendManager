using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public partial class UserDatabaseController
    {
        public void ProgressQuest(QuestData questData, int amount)
        {
            if(UserQuestData.questDatas.ContainsKey(questData.id))
            {
                UserQuestData.questDatas[questData.id].count += amount;
            }
            else
            {
                UserQuestData.questDatas[questData.id] = new QuestServerData()
                {
                    count = amount,
                    dayType = questData.dayType,
                    startTime = BackendTimeManager.instance.GetCurrentServerTime()
                };
            }
        }

        public int GetCurrentProgress(QuestData questData)
        {
            if(UserQuestData.questDatas.ContainsKey(questData.id))
            {
                return UserQuestData.questDatas[questData.id].count;
            }
            else
            {
                return 0;
            }
        }

        public QuestServerData GetServerData(QuestData questData)
        {
            if(UserQuestData.questDatas.ContainsKey(questData.id)) return UserQuestData.questDatas[questData.id];
            return null;
        }
    }
}
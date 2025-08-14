using System;
using System.Collections.Generic;
using System.Linq;

namespace StarCloudgamesLibrary
{
    public partial class UserDatabaseController
    {
        private LevelData currentLevelData;
        private double currentMaxEXP;

        #region "Set Up"

        public void SetUpLevelData()
        {
            currentLevelData = LevelDataManager.instance.GetCurrentLevelData(GetCurrentLevel());
            currentMaxEXP = currentLevelData.GetCurrentMaxEXP(GetCurrentLevel());
        }

        #endregion

        #region "Get Level"

        public int GetCurrentLevel()
        {
            var currentLevel = UserInformationData.level;

            if(currentLevel < 1)
            {
                currentLevel = 1;
                UserInformationData.level = currentLevel;
            }

            return currentLevel;
        }

        public double GetCurrentEXP() => UserInformationData.experiencePoint;

        public float GetCurrentEXPRatio()
        {
            if(currentLevelData == null) SetUpLevelData();

            return (float)(GetCurrentEXP() / currentMaxEXP);
        }

        #endregion

        #region "Add Level"

        public void AddExperiencePoint(double amount)
        {
            if(currentLevelData == null) SetUpLevelData();

            UserInformationData.experiencePoint += amount;
            var currentEXP = GetCurrentEXP();

            while(true)
            {
                if(currentEXP < currentMaxEXP) break;

                currentEXP -= currentMaxEXP;

                var nextLevelData = LevelDataManager.instance.GetCurrentLevelData(GetCurrentLevel() + 1);
                if(nextLevelData == null)
                {
                    currentEXP = currentMaxEXP;
                    break;
                }

                LevelUp();
            }

            UserInformationData.experiencePoint = currentEXP;
        }

        public void LevelUp()
        {
            UserInformationData.level++;
            SetUpLevelData();
        }

        #endregion

        #region "Dungeon Step"

        public int GetDungeonStep(DungeonType type)
        {
            var dungeonIndex = (int)type - 1;
            if(dungeonIndex < 0) return -1;

            if(UserInformationData.dungeonSteps == null || UserInformationData.dungeonSteps.Count <= dungeonIndex)
            {
                var dungeonCount = Enum.GetValues(typeof(DungeonType)).Length - 1;
                UserInformationData.dungeonSteps = new List<int>();
                for(int i = 0; i < dungeonCount; i++)
                {
                    UserInformationData.dungeonSteps.Add(0);
                }
            }

            return UserInformationData.dungeonSteps[dungeonIndex] + 1;
        }

        #endregion

        #region "Patrol"

        public bool IsPatrolling(string petName, out DateTime endTime)
        {
            foreach(var patrolData in UserInformationData.patrolDatas.Values)
            {
                if(patrolData.Key.Equals(petName))
                {
                    endTime = patrolData.Value;
                    return true;
                }
            }

            endTime = default;
            return false;
        }

        public bool IsPatrolling(PatrolType patrolType, out KeyValuePair<string, DateTime> patrolData)
        {
            if(UserInformationData.patrolDatas.TryGetValue(patrolType, out var currentPatrolData))
            {
                patrolData = currentPatrolData;
                return true;
            }

            patrolData = default;
            return false;
        }

        public void StartNewPatrol(PatrolType patrolType, DateTime endTime, string petName)
        {
            var patrolData = new KeyValuePair<string, DateTime>(petName, endTime);
            UserInformationData.patrolDatas[patrolType] = patrolData;
        }

        public void PatrolEnd(PatrolType patrolType)
        {
            if(IsPatrolling(patrolType, out _))
            {
                UserInformationData.patrolDatas.Remove(patrolType);
            }
        }

        #endregion

        #region "TowerOfPanda"

        public int[] GetTowerOfPandaData()
        {
            var currentData = UserInformationData.towerOfPandaData;
            if(currentData == null || currentData[0] <= 0 || currentData[1] <= 0)
            {
                UserInformationData.towerOfPandaData = new int[] { 1, 1 };
                currentData = UserInformationData.towerOfPandaData;
            }

            return currentData;
        }

        public Difficulty GetTowerOfPandaDifficulty()
        {
            return (Difficulty)GetTowerOfPandaData()[0];
        }

        public int GetTowerOfPandaFloor()
        {
            return GetTowerOfPandaData()[1];
        }

        public void IncreaseTowerOfPanda()
        {
            var currentData = GetTowerOfPandaData();
            if(TowerOfPandaManager.instance.GetData((Difficulty)currentData[0], currentData[1] + 1) != null)
            {
                currentData[1]++;
            }
            else if(TowerOfPandaManager.instance.GetData((Difficulty)currentData[0] + 1, 1) != null)
            {
                currentData[0]++;
                currentData[1] = 1;
            }
            else
            {
                DebugManager.DebugInGameErrorMessage("No more Tower of Panda data available for the next floor or difficulty.");
                return;
            }

            UserInformationData.towerOfPandaData = currentData;
        }

        #endregion
    }
}
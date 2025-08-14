using System;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public partial class UserDatabaseController
    {
        #region "Get"

        public int GetStage() => UserStageData.stage;
        public int GetFloor() => UserStageData.floor;
        public float GetGauge() => UserStageData.gauge;

        #endregion

        #region "Increase"

        public void IncreaseStage()
        {
            UserStageData.stage++;
            UserStageData.gauge = 0.0f;
        }

        public void IncreaseFloor()
        {
            var targetFloor = GetFloor() + 1;
            if (targetFloor > 500)
            {
                IncreaseStage();
                UserStageData.floor = 1;
            }
            else
            {
                UserStageData.floor = targetFloor;
                UserStageData.gauge = 0.0f;
            }
        }

        public void IncreaseGauge()
        {
            var increaseGaugeAmount = 100.0f / 50.0f;
            var targetGauge = Mathf.Clamp(GetGauge() + increaseGaugeAmount, 0, 100.0f);
            UserStageData.gauge = targetGauge;
        }

        #endregion
    }
}
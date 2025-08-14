using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public partial class UserDatabaseController
    {
        private UIMainPanel uiMainPanel;
        private UIBackgroundReward uIBackgroundReward;
        private UIPopupReward uIPopupReward;

        #region "Get Reward"

        public void GetReward(SCReward reward, UIType rewardUIType, string rewardUITitleKey)
        {
            AddReward(reward);

            var newRewardList = new List<SCReward>() { reward };
            ShowRewardPopup(newRewardList, rewardUIType, rewardUITitleKey);
            ShowRewardBanner(reward);
        }

        public void GetRewards(List<SCReward> rewards, UIType rewardUIType, string rewardUITitleKey)
        {
            foreach(var reward in rewards)
            {
                AddReward(reward);
                ShowRewardBanner(reward);
            }

            ShowRewardPopup(rewards, rewardUIType, rewardUITitleKey);
        }

        private void AddReward(SCReward reward)
        {
            switch(reward.rewardType)
            {
                case RewardType.Currency:
                    EarnCurrency(reward.rewardID, reward.amount);
                    break;
                case RewardType.UserData:
                    GetUserData(reward);
                    break;
                case RewardType.Equippable:
                    GetEquippable(reward);
                    break;
                case RewardType.Item:
                    GetItem(reward);
                    break;
                default:
                    DebugManager.DebugInGameWarningMessage($"{reward}'s reward type is not valid");
                    break;
            }
        }

        #endregion

        #region "UI"

        private void ShowRewardBanner(SCReward reward)
        {
            uiMainPanel ??= (UIMainPanel)UIManager.instance.GetUI(UIType.UIMainPanel);
            uiMainPanel.ShowRewardBanner(reward);
        }

        private void ShowRewardPopup(List<SCReward> rewards, UIType rewardUIType, string rewardUITitleKey)
        {
            switch(rewardUIType)
            {
                case UIType.UIBackgroundReward:
                    uIBackgroundReward ??= (UIBackgroundReward)UIManager.instance.GetUI(rewardUIType);
                    uIBackgroundReward.SetUp(rewards, rewardUITitleKey);
                    break;
                case UIType.UIPopupReward:
                    uIPopupReward ??= (UIPopupReward)UIManager.instance.GetUI(rewardUIType);
                    uIPopupReward.SetUp(rewards, rewardUITitleKey);
                    break;
                default:
                    DebugManager.DebugInGameWarningMessage($"{rewardUIType} is not reward ui type.");
                    break;
            }
        }

        #endregion

        #region "Get By Category"

        private void GetUserData(SCReward reward)
        {
            switch (reward.rewardID)
            {
                case RewardID.experiencePoint:
                    AddExperiencePoint(reward.amount);
                    break;
                default:
                    break;
            }
        }

        private void GetEquippable(SCReward reward)
        {
            if(reward.rewardID.IsRandomReward())
            {
                reward.ResolveRandomReward();
            }

            var targetStat = ScriptableStatManager.instance.GetStatByReward(reward);
            if(targetStat == null)
            {
                DebugManager.DebugInGameErrorMessage($"{reward.rewardName} is not exist");
                return;
            }

            for(int i = 0; i < reward.amount; i++)
            {
                targetStat.GetNewStat();
            }
        }

        private void GetItem(SCReward reward)
        {
            switch (reward.rewardID)
            {
                case RewardID.randomDungeonKeyDirect:
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
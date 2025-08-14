using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace StarCloudgamesLibrary
{
    [System.Serializable]
    public class UserCurrencyData
    {
        public List<PaidCash> paidCashList;

        public Dictionary<RewardID, double> currencyList;

        #region "Initialize"

        public void InitializeCurrencyList()
        {
            currencyList = new Dictionary<RewardID, double>();

            var enumValues = Enum.GetValues(typeof(RewardID));
            foreach(RewardID enumKey in enumValues)
            {
                if((int)enumKey > 99) continue;

                var number = (int)enumKey;
                if(number > 0 && number < 100)
                {
                    currencyList[enumKey] = 100000000;
                }
            }

            paidCashList = new List<PaidCash>();
        }

        #endregion

        #region "Get"

        public double GetCurrency(RewardID currencyID)
        {
            if(currencyID == RewardID.paidCash)
            {
                return GetCash();
            }
            else
            {
                if(currencyList.TryGetValue(currencyID, out var currency))
                {
                    return currency;
                }
            }

            return 0d;
        }

        private double GetCash()
        {
            var resultCash = GetCurrency(RewardID.freeCash);

            foreach (var paidCash in paidCashList)
            {
                resultCash += paidCash.amount;
            }

            return resultCash;
        }

        #endregion

        #region "Earn"

        public void EarnCurrency(RewardID currencyID, double amount, PaidCash newPaidCash = null)
        {
            if(currencyID == RewardID.paidCash && newPaidCash != null)
            {
                paidCashList.Add(newPaidCash);
            }
            else
            {
                if(currencyList.TryGetValue(currencyID, out var currency))
                {
                    currencyList[currencyID] = currency + amount;
                }
                else
                {
                    Debug.LogError($"{currencyID} is not exist");
                }
            }
        }

        #endregion

        #region "Use"

        private void UseCash(double amount)
        {
            var remainingAmount = amount;

            for(int i = 0; i < paidCashList.Count; i++)
            {
                if(paidCashList[i].amount > 0)
                {
                    var cashStruct = paidCashList[i];

                    if(remainingAmount > cashStruct.amount)
                    {
                        remainingAmount -= cashStruct.amount;
                        cashStruct.amount = 0;
                    }
                    else
                    {
                        cashStruct.amount -= remainingAmount;
                        remainingAmount = 0;
                    }

                    paidCashList[i] = cashStruct;

                    if(remainingAmount <= 0)
                    {
                        break;
                    }
                }
            }

            UseCurrency(RewardID.freeCash, remainingAmount);

            DebugManager.DebugInGameMessage($"Used {amount} paid cash");
        }

        public void UseCurrency(RewardID currencyID, double amount)
        {
            if(currencyID == RewardID.paidCash)
            {
                UseCash(amount);
            }
            else
            {
                if(currencyList.TryGetValue(currencyID, out var currency))
                {
                    currencyList[currencyID] = currency - amount;
                }
                else
                {
                    Debug.LogError($"{currencyID} is not exist");
                }
            }
        }

        #endregion
    }

    [System.Serializable]
    public class PaidCash
    {
        public string key;
        public double amount;
        public string dateTime;
    }
}
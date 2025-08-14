using System;

namespace StarCloudgamesLibrary
{
    public partial class UserDatabaseController
    {
        private event Action CurrencyChangedEvent;

        #region "Event"

        public void AddCurrencyChangedEvent(Action action)
        {
            CurrencyChangedEvent += action;
        }

        public void RemoveCurrencyChangedEvent(Action action)
        {
            CurrencyChangedEvent -= action;
        }

        private void CurrencyChanged() => CurrencyChangedEvent?.Invoke();

        #endregion

        #region "String"

        private string CurrencyToFormat(object currency)
        {
            return string.Format("{0:N0}", currency);
        }

        public string GetCurrentGoldString()
        {
            return CurrencyToFormat(GetCurrency(RewardID.gold));
        }

        public string GetCurrentCashString()
        {
            return CurrencyToFormat(GetCurrency(RewardID.freeCash));
        }

        #endregion

        #region "Get"

        public double GetCurrency(RewardID currencyType)
        {
            return UserCurrencyData.GetCurrency(currencyType);
        }

        #endregion

        #region "Modify"

        public bool CanUseCurrency(RewardID currencyType, double amount)
        {
            return GetCurrency(currencyType) >= amount;
        }

        public void EarnCurrency(RewardID currencyType, double amount, PaidCash newPaidCash = null)
        {
            UserCurrencyData.EarnCurrency(currencyType, amount, newPaidCash);
            CurrencyChanged();
            //DebugManager.DebugInGameMessage($"Earned {amount} Free {currencyType}");
        }

        public bool UseCurrency(RewardID currencyType, double amount)
        {
            if(CanUseCurrency(currencyType, amount))
            {
                UserCurrencyData.UseCurrency(currencyType, amount);

                CurrencyChanged();
                DebugManager.DebugInGameMessage($"Used {amount} {currencyType}");
                return true;
            }
            else
            {
                DebugManager.DebugInGameMessage($"Can't use {amount} {currencyType}");
                return false;
            }
        }

        #endregion
    }
}
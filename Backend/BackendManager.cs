using System;
using BackEnd;
using Unity.VisualScripting;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public partial class BackendManager : SingleTon<BackendManager>
    {
        private TheBackend.ToolKit.InvalidFilter.FilterManager filterManager;

        #region "Unity"

        private void Start()
        {
            InitializeBackend();
        }

        #endregion

        #region "Initialize"

        private void InitializeBackend()
        {
            var bro = Backend.Initialize();

            if (bro.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Backend Initialize Success!");
                InitializeFilterManager();
                BackendTimeManager.instance.InitializeTime();
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Backend Initialize Failed : {bro}");
                DirectErrorHandler(bro, InitializeBackend);
            }
        }

        private void InitializeFilterManager()
        {
            filterManager = new TheBackend.ToolKit.InvalidFilter.FilterManager();
            if (filterManager.LoadInvalidString())
            {
                DebugManager.DebugServerMessage("FilterManager Initialize Success!");
            }
            else
            {
                DebugManager.DebugServerErrorMessage("FilterManager Initialize Failed!");
            }
        }

        public bool Initialized()
        {
            return Backend.IsInitialized && !filterManager.IsEmpty;
        }

        #endregion

        #region "Filter"

        public bool CanFilter(string text)
        {
            if (filterManager.IsEmpty)
            {
                DebugManager.DebugServerErrorMessage("FilterManager is not initialized, but trying to access");
                return false;
            }
            else
            {
                if (filterManager.IsFilteredString(text))
                {
                    DebugManager.DebugServerWarningMessage($"can't use {text}. Badword is contained");
                    return false;
                }
                else
                {
                    DebugManager.DebugServerMessage($"can use {text}!");
                    return true;
                }
            }
        }

        #endregion

        #region "Error Handler"

        public void DirectErrorHandler(BackendReturnObject callback, Action action)
        {
            DebugManager.DebugServerErrorMessage($"backend erorr : {callback.ToString()}");
            if(callback.IsClientRequestFailError())
            {
                UIManager.instance.ShowNetworkIndicator(true, false, action);
            }
            else if(callback.IsServerError())
            {
                UIManager.instance.SystemMessage(false, "ServerError", "ServerError_Description", "Quit", Application.Quit);
            }
            else if(callback.IsMaintenanceError())
            {
                UIManager.instance.SystemMessage(false, "ServerMaintainance", "ServerMaintainance_Description", "Quit", Application.Quit);
            }
            else if(callback.IsTooManyRequestError())
            {
            }
            else if(callback.IsBadAccessTokenError())
            {
            }
            else
            {
                UIManager.instance.SystemMessage(false, "ServerError", "ServerError_Description", "Quit", Application.Quit);
            }
        }

        public bool ItsServerError(BackendReturnObject callback)
        {
            if(callback.IsClientRequestFailError() || callback.IsServerError() || callback.IsMaintenanceError() || callback.IsTooManyRequestError() || callback.IsBadAccessTokenError())
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
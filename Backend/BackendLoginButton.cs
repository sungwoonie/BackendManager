using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarCloudgamesLibrary
{
    public class BackendLoginButton : MonoBehaviour
    {
        public LoginType loginType;

        private Button loginButton;

        #region "Unity"

        private void Awake()
        {
            InitializeButton();
        }

        #endregion

        #region "Initialize"

        private void InitializeButton()
        {
            loginButton = GetComponent<Button>();
            loginButton.onClick.AddListener(OnClickLogin);
        }

        #endregion

        #region "OnClick"

        private void OnClickLogin()
        {
            if (BackendManager.instance == null)
            {
                DebugManager.DebugServerErrorMessage("Backend Manager is not exist.");
                return;
            }

            var uiLogin = (UILogin)UIManager.instance.GetUI(UIType.UILogin);
            uiLogin.OnClickLogin();

            UIManager.instance.ShowNetworkIndicator(true);

            switch (loginType)
            {
                case LoginType.Guest:
                    BackendManager.instance.GuestLogin();
                    break;
                case LoginType.Federation:
#if UNITY_ANDROID && !UNITY_EDITOR
                    BackendManager.instance.OnClickGPGSLogin();
#elif UNITY_IOS
                    BackendManager.instance.OnClickAppleLogin();
#else
                    UIManager.instance.ToastMessage("System_WrongPlatform");
                    DebugManager.DebugServerWarningMessage("Current platform is not Fedaration but trying to login");
#endif
                    break;
                case LoginType.GuestToFederation:

                    if(!BackendManager.instance.userDatabaseController.UserInformationData.federationId.Equals("customSignUp"))
                    {
                        UIManager.instance.ToastMessage("System_AlreadyFederation");
                    }
                    else
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                        BackendManager.instance.GuestToGPGS();
#elif UNITY_IOS
                        BackendManager.instance.GuestToApple();
#else
                        UIManager.instance.ToastMessage("System_WrongPlatform");
                        DebugManager.DebugServerWarningMessage("Current platform is not Fedaration but trying to login");
#endif
                    }
                    break;
            }
        }

#endregion
    }

    public enum LoginType
    {
        Guest, Federation, GuestToFederation
    }
}
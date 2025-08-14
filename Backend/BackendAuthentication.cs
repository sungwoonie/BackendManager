using BackEnd;
using AppleAuth;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public partial class BackendManager
    {
        private Action<FederationType> federationAction;
        private string accessToken;

        #region "Auto"

        public bool AutoLogin()
        {
            var callback = Backend.BMember.LoginWithTheBackendToken();

            if (callback.IsSuccess())
            {
                DebugManager.DebugServerMessage("Auto Login success!");
                return true;
            }
            else
            {
                switch (callback.StatusCode)
                {
                    case 400:
                        DebugManager.DebugServerErrorMessage($"Access token is not exist");
                        break;
                    case 401:
                        DebugManager.DebugServerErrorMessage($"Refresh token is expired");
                        break;
                    case 403:
                        DebugManager.DebugServerErrorMessage($"Banned User. message : {callback.Message}");
                        UIManager.instance.SystemMessage(false, "System_BannedAuthenticate", "System_BannedAuthenticateDescription", "Quit", () => Application.Quit());
                        break;
                    default:
                        DebugManager.DebugServerErrorMessage($"Unknown Error. message : {callback.Message}");
                        UIManager.instance.SystemMessage(false, "System_UnknownError", "System_System_UnknownErrorDescription", "Quit", () => Application.Quit());
                        break;
                }

                return false;
            }
        }

        #endregion

        #region "Backend Federation"

        private void LoginBackendByFederation(FederationType federationType)
        {
            var federationLogin = Backend.BMember.AuthorizeFederation(accessToken, federationType);

            if(federationLogin.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Backend Federation Login success. callback : {federationLogin}");
                LoginSuccess();
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Backend Federation Login failed. message : {federationLogin.Message}. code : {federationLogin.ErrorCode}");
                DirectErrorHandler(federationLogin, null);
            }
        }

        private void ChangeGuestToFederation(FederationType federationType)
        {
            var callback = Backend.BMember.ChangeCustomToFederation(accessToken, federationType);

            if(callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Guest To Federation Success!");
                SceneLoadManager.LoadScene(SceneName.LoginScene, true);
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Backend Federation Login failed. message : {callback.Message}. code : {callback.ErrorCode}");
                DirectErrorHandler(callback, null);
            }
        }

        #endregion

        #region "Apple"

#if UNITY_IOS

        public void OnClickAppleLogin()
        {
            federationAction = LoginBackendByFederation;
            AuthenticateAppleAuth();
        }

        private void AuthenticateAppleAuth()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                DebugManager.DebugServerMessage("Trying to Apple Login");

                var deserializer = new PayloadDeserializer();
                var appleAuthManager = new AppleAuthManager(deserializer);

                var quickLoginArgs = new AppleAuthQuickLoginArgs();

                appleAuthManager.QuickLogin(quickLoginArgs, credential =>
                {
                    DebugManager.DebugServerMessage($"Apple Quick Login Success. credential : {credential}");

                    var appleIdCredential = credential as IAppleIDCredential;
                    GetAppleAccessToken(appleIdCredential);
                },
                error =>
                {
                    DebugManager.DebugServerErrorMessage($"Error with Apple Quick Login. Trying to Manual Login");

                    var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

                    appleAuthManager.LoginWithAppleId(loginArgs, credential =>
                    {
                        DebugManager.DebugServerMessage($"Apple Manual Login Success. credential : {credential}");

                        var appleIdCredential = credential as IAppleIDCredential;
                        GetAppleAccessToken(appleIdCredential);
                    },
                    error =>
                    {
                        var authorizationErrorCode = error.GetAuthorizationErrorCode();
                        DebugManager.DebugServerErrorMessage($"Error with Apple Login. Code : {authorizationErrorCode}");
                    });
                });
            }
        }

        private void GetAppleAccessToken(IAppleIDCredential credential)
        {
            if (credential.IdentityToken != null)
            {
                accessToken = Encoding.UTF8.GetString(credential.IdentityToken, 0, credential.IdentityToken.Length);
                DebugManager.DebugServerMessage($"Apple Token : {accessToken}");
                ExecuteFederationAction(FederationType.Apple);
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Error with Apple Quick Login. IdentityToken of appleIdCredential");
            }
        }

        public void GuestToApple()
        {
            federationAction = ChangeGuestToFederation;
            AuthenticateAppleAuth();
        }

#endif

        #endregion

        #region "GPGS"

#if UNITY_ANDROID

        public void OnClickGPGSLogin()
        {
            federationAction = LoginBackendByFederation;
            AuthenticateGPGS();
        }

        private void AuthenticateGPGS()
        {
            PlayGamesPlatform.Instance.Authenticate((status) =>
            {
                if (status == SignInStatus.Success)
                {
                    DebugManager.DebugServerMessage($"GPGS authentication success");
                    GetGPGSAccessToken();
                }
                else
                {
                    DebugManager.DebugServerWarningMessage($"GPGS authentication failed. Will Try manual login");
                    PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
                    {
                        if (status == SignInStatus.Success)
                        {
                            DebugManager.DebugServerMessage($"GPGS authentication success");
                            GetGPGSAccessToken();
                        }
                    });
                }
            });
        }

        private void GetGPGSAccessToken()
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
            {
                DebugManager.DebugServerMessage($"GPGS Code : {code}");

                Backend.BMember.GetGPGS2AccessToken(code, googleCallback =>
                {
                    DebugManager.DebugServerMessage($"GPGS Token : {googleCallback}");

                    if (googleCallback.IsSuccess())
                    {
                        accessToken = googleCallback.GetReturnValuetoJSON()["access_token"].ToString();
                        DebugManager.DebugServerMessage($"GPGS AccessToken : {accessToken}");
                        ExecuteFederationAction(FederationType.GPGS2);
                    }
                });
            });
        }

        public void GuestToGPGS()
        {
            federationAction += ChangeGuestToFederation;
            AuthenticateGPGS();
        }

#endif

        #endregion

        #region "Guest"

        public void GuestLogin()
        {
            var callback = Backend.BMember.GuestLogin();

            if(callback.IsSuccess())
            {
                DebugManager.DebugServerMessage($"Guest Login Success!");
                LoginSuccess();
            }
            else
            {
                DebugManager.DebugServerErrorMessage($"Backend Initialize Failed. message : {callback.Message}. code : {callback.ErrorCode}");
                DirectErrorHandler(callback, null);
            }
        }

        #endregion

        #region "Event"

        public void ExecuteFederationAction(FederationType federationType)
        {
            federationAction?.Invoke(federationType);
            federationAction = null;
        }

        #endregion

        public void LoginSuccess()
        {
            UIManager.instance.ShowNetworkIndicator(false);

            InitializeDatabase();
        }

        public void LoginFailed()
        {
            UIManager.instance.ShowNetworkIndicator(false);
            UIManager.instance.GetUI(UIType.UILogin).Open();
            UIManager.instance.SystemMessage(true, "LoginFailed", "LoginFailed_Description");
        }
    }
}
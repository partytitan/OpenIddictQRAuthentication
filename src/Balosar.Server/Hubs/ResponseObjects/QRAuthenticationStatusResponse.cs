using IdentityModel.Client;

namespace Balosar.Server.Hubs.ResponseObjects;

public class QRAuthenticationStatusResponse
{
    public bool AuthorizationPending { get; set; }
    public bool AuthorizationFailed { get; set; }
    public bool AuthorizationSuccessful { get; set; }
    public bool Error { get; set; }
    public string ErrorMessage { get; set; }

    public QRAuthenticationStatusResponse(TokenResponse tokenResponse)
    {
        switch (tokenResponse)
        {
            case { IsError: true, Error: IdentityModel.OidcConstants.TokenErrors.AuthorizationPending }:
                AuthorizationPending = true;
                break;
            case { IsError: true, Error: IdentityModel.OidcConstants.TokenErrors.AccessDenied }:
                AuthorizationFailed = true;
                break;
            case { IsError: true }:
                Error = true;
                break;
            case { IsError: false, IdentityToken: null }:
                break;
            default:
                AuthorizationSuccessful = true;
                break;
        }
        ErrorMessage = tokenResponse.ErrorDescription;
    }
}
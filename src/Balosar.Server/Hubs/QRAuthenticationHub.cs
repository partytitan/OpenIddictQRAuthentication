using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Balosar.Server.Cache;
using Balosar.Server.Helpers;
using Balosar.Server.Hubs.ResponseObjects;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Balosar.Server.Hubs;

public class QRAuthenticationHub : Hub
{
    private const string QRAuthenticationClientId = "balosar-internal-device-client";
    private const string QRAuthenticationClientSecret = "8a177cb2-0942-458a-92bf-360bfd6d5f2a";
    private const string QRAuthenticationClientScopes = "openid profile email roles";
    
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly QRAuthenticationCache _qrAuthenticationCache;

    public QRAuthenticationHub(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory,
        QRAuthenticationCache qrAuthenticationCache)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _qrAuthenticationCache = qrAuthenticationCache;
    }

    public async Task RequestDeviceCode()
    {        
        var client = _httpClientFactory.CreateClient();

        var deviceAuthorizationAddress = _httpContextAccessor.HttpContext?.Request.BaseUrl() + "connect/device";
        var deviceAuthorizationRequest = new DeviceAuthorizationRequest
        {
            Address = deviceAuthorizationAddress,
            ClientId = QRAuthenticationClientId,
            ClientSecret = QRAuthenticationClientSecret,
            Scope = QRAuthenticationClientScopes,
            
        };
        var response = await client.RequestDeviceAuthorizationAsync(deviceAuthorizationRequest);

        if (response.IsError)
        {
            throw new Exception(response.Error);
        }

        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveDeviceCode", response);
    }

    public async Task RequestQRAuthenticationStatus(string deviceCode)    
    {
        var client = _httpClientFactory.CreateClient();

        var deviceAuthorizationAddress = _httpContextAccessor.HttpContext?.Request.BaseUrl() + "connect/token";
        
        var deviceTokenRequest = new DeviceTokenRequest()
        {
            Address = deviceAuthorizationAddress,
            ClientId = QRAuthenticationClientId,
            ClientSecret = QRAuthenticationClientSecret,
            DeviceCode = deviceCode
        };
        
        var response = await client.RequestDeviceTokenAsync(deviceTokenRequest);

        if (response.IsError && response.ErrorType == ResponseErrorType.Exception)
        {
            throw new Exception(response.Error);
        }

        if (!string.IsNullOrEmpty(response.IdentityToken))
        {
            _qrAuthenticationCache.SetJwtForDeviceCode(deviceCode, new JwtSecurityToken(response.IdentityToken));
        }

        var result = new QRAuthenticationStatusResponse(response);
        
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveQRAuthenticationStatus", result);
    }
}
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Memory;

namespace Balosar.Server.Cache;

public class QRAuthenticationCache
{
    private readonly IMemoryCache _memoryCache;

    public QRAuthenticationCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    public void SetJwtForDeviceCode(string deviceCode, JwtSecurityToken identityToken)
    {
        _memoryCache.Set(deviceCode, identityToken, TimeSpan.FromMinutes(1));
    }

    public JwtSecurityToken GetJwtForDeviceCode(string deviceCode)
    {
        return _memoryCache.Get<JwtSecurityToken>(deviceCode);
    }
}
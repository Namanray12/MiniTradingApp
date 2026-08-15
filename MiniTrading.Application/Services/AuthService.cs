using Microsoft.Extensions.Configuration;
using MiniTrading.Application.Dtos;
using MiniTrading.Application.DTOs;
using MiniTrading.Application.Interfaces;
using MiniTrading.Domain.Constants;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MiniTrading.Application.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AuthDto> GetAuthTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var authUrl = _configuration[AppConstant.ConfigurationKeys.ActTraderAuthUrl] ?? AppConstant.RestAuth.EndpointUrl;
            var username = _configuration[AppConstant.ConfigurationKeys.ActTraderUsername] ?? string.Empty;
            var password = _configuration[AppConstant.ConfigurationKeys.ActTraderPassword] ?? string.Empty;

            var requestUri = new Uri(authUrl);

            using var initialClient = new HttpClient();
            initialClient.DefaultRequestHeaders.TryAddWithoutValidation(AppConstant.RestAuth.UserAgentHeader, AppConstant.RestAuth.UserAgentValue);
            initialClient.DefaultRequestHeaders.TryAddWithoutValidation(AppConstant.RestAuth.AcceptHeader, AppConstant.RestAuth.AcceptValue);

            var initialResponse = await initialClient.GetAsync(authUrl, cancellationToken);

            if (initialResponse.IsSuccessStatusCode)
            {
                var successContent = await initialResponse.Content.ReadAsStringAsync(cancellationToken);
                return ParseTokenResponse(successContent);
            }

            var wwwAuthHeader = string.Empty;
            if (initialResponse.Headers.TryGetValues(AppConstant.RestAuth.WwwAuthenticateHeader, out var values))
            {
                wwwAuthHeader = string.Join(AppConstant.RestAuth.SpaceChar.ToString(), values);
            }
            else
            {
                wwwAuthHeader = initialResponse.Headers.WwwAuthenticate.ToString();
            }

            if (!string.IsNullOrEmpty(wwwAuthHeader) && wwwAuthHeader.Contains(AppConstant.RestAuth.DigestScheme, StringComparison.OrdinalIgnoreCase))
            {
                var realm = ExtractDigestParam(wwwAuthHeader, AppConstant.RestAuth.RealmKey);
                var nonce = ExtractDigestParam(wwwAuthHeader, AppConstant.RestAuth.NonceKey);
                var qop = ExtractDigestParam(wwwAuthHeader, AppConstant.RestAuth.QopKey);
                var opaque = ExtractDigestParam(wwwAuthHeader, AppConstant.RestAuth.OpaqueKey);

                var uriPath = requestUri.PathAndQuery;
                var ha1 = ComputeMd5($"{username}:{realm}:{password}");
                var ha2 = ComputeMd5($"{AppConstant.RestAuth.HttpGetMethod}:{uriPath}");

                string digestResponse;
                string authorizationHeader;

                if (!string.IsNullOrEmpty(qop) && qop.Contains(AppConstant.RestAuth.AuthQop, StringComparison.OrdinalIgnoreCase))
                {
                    var nc = AppConstant.RestAuth.NonceCount;
                    var cnonce = Guid.NewGuid().ToString(AppConstant.RestAuth.GuidFormatN)[..16];
                    digestResponse = ComputeMd5($"{ha1}:{nonce}:{nc}:{cnonce}:{AppConstant.RestAuth.AuthQop}:{ha2}");

                    authorizationHeader = $"{AppConstant.RestAuth.DigestScheme} username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{uriPath}\", response=\"{digestResponse}\", qop={AppConstant.RestAuth.AuthQop}, nc={nc}, cnonce=\"{cnonce}\"";
                }
                else
                {
                    digestResponse = ComputeMd5($"{ha1}:{nonce}:{ha2}");
                    authorizationHeader = $"{AppConstant.RestAuth.DigestScheme} username=\"{username}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{uriPath}\", response=\"{digestResponse}\"";
                }

                if (!string.IsNullOrEmpty(opaque))
                {
                    authorizationHeader += $", opaque=\"{opaque}\"";
                }

                using var authenticatedClient = new HttpClient();
                using var authenticatedRequest = new HttpRequestMessage(HttpMethod.Get, authUrl);
                authenticatedRequest.Headers.TryAddWithoutValidation(AppConstant.RestAuth.AuthorizationHeader, authorizationHeader);
                authenticatedRequest.Headers.TryAddWithoutValidation(AppConstant.RestAuth.UserAgentHeader, AppConstant.RestAuth.UserAgentValue);
                authenticatedRequest.Headers.TryAddWithoutValidation(AppConstant.RestAuth.AcceptHeader, AppConstant.RestAuth.AcceptValue);

                var authResponse = await authenticatedClient.SendAsync(authenticatedRequest, cancellationToken);
                var content = await authResponse.Content.ReadAsStringAsync(cancellationToken);

                if (authResponse.IsSuccessStatusCode)
                {
                    return ParseTokenResponse(content);
                }

                return new AuthDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"{authResponse.StatusCode}: {content}"
                };
            }

            return new AuthDto
            {
                IsSuccess = false,
                ErrorMessage = initialResponse.StatusCode.ToString()
            };
        }
        catch (Exception ex)
        {
            return new AuthDto
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static AuthDto ParseTokenResponse(string jsonContent)
    {
        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        string token = string.Empty;
        if (root.TryGetProperty(AppConstant.RestAuth.ResultProperty, out var resultProp))
        {
            token = resultProp.GetString() ?? string.Empty;
        }
        else if (root.TryGetProperty(AppConstant.RestAuth.TokenProperty, out var tokenProp))
        {
            token = tokenProp.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(token))
        {
            return new AuthDto
            {
                IsSuccess = false,
                ErrorMessage = AppConstant.Messages.TokenNotFound
            };
        }

        return new AuthDto
        {
            Token = token,
            IsSuccess = true,
            ExpiresAt = DateTime.UtcNow.AddHours(AppConstant.Calculations.TokenLifetimeHours)
        };
    }

    private static string ExtractDigestParam(string header, string paramName)
    {
        if (string.IsNullOrEmpty(header)) return string.Empty;

        var key = $"{paramName}{AppConstant.RestAuth.EqualsSign}{AppConstant.RestAuth.QuoteCharString}";
        var index = header.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            var start = index + key.Length;
            var end = header.IndexOf(AppConstant.RestAuth.QuoteChar, start);
            if (end != -1)
            {
                return header.Substring(start, end - start);
            }
        }

        var unquotedKey = $"{paramName}{AppConstant.RestAuth.EqualsSign}";
        var uIndex = header.IndexOf(unquotedKey, StringComparison.OrdinalIgnoreCase);
        if (uIndex != -1)
        {
            var start = uIndex + unquotedKey.Length;
            var end = header.IndexOfAny(AppConstant.RestAuth.DigestParamSeparators, start);
            if (end == -1) end = header.Length;
            return header.Substring(start, end - start).Trim(AppConstant.RestAuth.QuoteChar, AppConstant.RestAuth.SpaceChar);
        }

        return string.Empty;
    }

    private static string ComputeMd5(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
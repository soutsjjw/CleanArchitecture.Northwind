﻿namespace CleanArchitecture.Northwind.Application.Common.Models;

public sealed class AccessTokenResponse
{
    /// <summary>
    /// The value is always "Bearer" which indicates this response provides a "Bearer" token
    /// in the form of an opaque <see cref="AccessToken"/>.
    /// </summary>
    /// <remarks>
    /// This is serialized as "tokenType": "Bearer" using <see cref="JsonSerializerDefaults.Web"/>.
    /// </remarks>
    public string TokenType { get; } = "Bearer";

    /// <summary>
    /// The opaque bearer token to send as part of the Authorization request header.
    /// </summary>
    /// <remarks>
    /// This is serialized as "accessToken": "{AccessToken}" using <see cref="JsonSerializerDefaults.Web"/>.
    /// </remarks>
    public required string AccessToken { get; init; }

    /// <summary>
    /// The number of seconds before the <see cref="AccessToken"/> expires.
    /// </summary>
    /// <remarks>
    /// This is serialized as "expiresIn": "{ExpiresInSeconds}" using <see cref="JsonSerializerDefaults.Web"/>.
    /// </remarks>
    public required long ExpiresIn { get; init; }

    /// <summary>
    /// If set, this provides the ability to get a new access_token after it expires using a refresh endpoint.
    /// </summary>
    /// <remarks>
    /// This is serialized as "refreshToken": "{RefreshToken}" using using <see cref="JsonSerializerDefaults.Web"/>.
    /// </remarks>
    public required string RefreshToken { get; init; }
}

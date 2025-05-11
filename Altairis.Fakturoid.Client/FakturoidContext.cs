global using Altairis.Fakturoid.Client.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Altairis.Fakturoid.Client.Proxies;

namespace Altairis.Fakturoid.Client;

/// <summary>
/// Class representing connection to Fakturoid API, holds authentication information etc.
/// </summary>
public class FakturoidContext {
    private const string DEFAULT_USER_AGENT = "C#/.NET API Client v3 by Altairis (fakturoid@rider.cz)";
    private const string API_BASE_URL_FORMAT = "https://app.fakturoid.cz/api/v3/accounts/{0}/";

    /// <summary>
    /// URL for obtaining access token using OAuth 2
    /// </summary>
    public const string ACCESS_TOKEN_URL = "https://app.fakturoid.cz/api/v3/oauth/token";
    private const float ACCESS_TOKEN_REFRESH_MARGIN = 0.66f; // Refresh access token in 2/3 of its lifetime

    private bool useClientCredentials = true; // Use client credentials flow by default
    private const string GRANT_CLIENT_CREDENTIALS = "client_credentials";
    private const string GRANT_REFRESH_TOKEN = "refresh_token";
    private string refreshTokenValue; // For Authorization Code Flow

    private string accessTokenType;
    private string accessTokenValue;
    private DateTime accessTokenRefresh;
    private bool IsAccessTokenValid => this.accessTokenType != null && this.accessTokenValue != null && this.accessTokenRefresh > DateTime.Now;

    private HttpClient httpClient; // Reuse a static HttpClient instance to avoid socket exhaustion issues.


    // Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="FakturoidContext" /> class.
    /// </summary>
    /// <param name="accountName">Account name (accountName).</param>
    /// <param name="clientId">The client ID for OAuth 2 Client Credentials Flow.</param>
    /// <param name="clientSecret">The client secret for OAuth 2 Client Credentials Flow.</param>
    /// <param name="userAgent">The User-Agent HTTP header value.</param>
    /// <param name="httpClient">Optional HTTP client to use for requests. 
    /// If not provided, a new instance will be created and shared between request for the lifetime of this context. 
    /// For proper HttpClient lifetime management (.NET Core 2.1.+) use IHttpClientFactory
    /// and pass created instance <see cref="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-9.0"/>
    /// </param>
    /// <param name="refreshToken">Optional refresh token for OAuth 2 Authorization Code Flow. If provided, the client will use this token to authenticate (authorization code flow).</param>
    /// <exception cref="ArgumentNullException">accountName
    /// or
    /// authenticationToken
    /// or
    /// userAgent</exception>
    /// <exception cref="ArgumentException">Value cannot be empty or whitespace only string.;accountName
    /// or
    /// Value cannot be empty or whitespace only string.;authenticationToken
    /// or
    /// Value cannot be empty or whitespace only string.;userAgent</exception>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public FakturoidContext(string accountName, string clientId, string clientSecret, string userAgent = DEFAULT_USER_AGENT, HttpClient? httpClient = null, string? refreshToken = null) {
        if (accountName == null) throw new ArgumentNullException(nameof(accountName));
        if (string.IsNullOrWhiteSpace(accountName)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(accountName));
        if (clientId == null) throw new ArgumentNullException(nameof(clientId));
        if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(clientId));
        if (clientSecret == null) throw new ArgumentNullException(nameof(clientSecret));
        if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(clientSecret));
        if (userAgent == null) throw new ArgumentNullException(nameof(userAgent));
        if (string.IsNullOrWhiteSpace(userAgent)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(userAgent));

        // Configuration properties
        this.AccountName = accountName;
        this.ClientId = clientId;
        this.ClientSecret = clientSecret;
        this.UserAgent = userAgent;

        // Proxies
        this.BankAccounts = new(this);
        this.Events = new(this);
        this.Invoices = new(this);
        this.InvoiceMessage = new(this);
        this.InvoicePayments = new(this);
        this.NumberFormats = new(this);
        this.Subjects = new(this);
        this.Todos = new(this);

        // Configure flow
        if (refreshToken is not null && refreshToken.Length > 0) {
            this.useClientCredentials = false; // use Authorization Code Flow
            this.refreshTokenValue = refreshToken;
        }

        // Configure HTTP client
        this.httpClient ??= httpClient ?? new HttpClient();
        ConfigureHttpClient(this.httpClient);
    }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

    #region Properties
    /// <summary>
    /// Gets the Fakturoid account name.
    /// </summary>
    /// <value>
    /// The name of the Fakturoid account.
    /// </value>
    public string AccountName { get; private set; }

    /// <summary>
    /// Gets the Fakturoid account email address.
    /// </summary>
    /// <value>
    /// The email address associated with Fakturoid account being used.
    /// </value>
    public string ClientId { get; private set; }

    /// <summary>
    /// Gets the Fakturoid authentication token.
    /// </summary>
    /// <value>
    /// The Fakturoid authentication token.
    /// </value>
    public string ClientSecret { get; private set; }

    /// <summary>
    /// Gets the User-Agent header used for HTTP requests.
    /// </summary>
    /// <value>
    /// The User-Agent header value.
    /// </value>
    public string UserAgent { get; private set; }

    // Proxies

    /// <summary>
    /// Gets the bank accounts.
    /// </summary>
    public FakturoidBankAccountsProxy BankAccounts { get; }

    /// <summary>
    /// Gets the events.
    /// </summary>
    public FakturoidEventsProxy Events { get; }

    /// <summary>
    /// Gets the invoices.
    /// </summary>
    public FakturoidInvoicesProxy Invoices { get; }

    /// <summary>
    /// Gets the invoice message.
    /// </summary>
    public FakturoidInvoiceMessageProxy InvoiceMessage { get; }
    
    /// <summary>
    /// Gets the invoice payments.
    /// </summary>
    public FakturoidInvoicePaymentsProxy InvoicePayments { get; }

    /// <summary>
    /// Gets the number formats.
    /// </summary>
    public FakturoidNumberFormatsProxy NumberFormats { get; }

    /// <summary>
    /// Gets the subjects.
    /// </summary>
    public FakturoidSubjectsProxy Subjects { get; }

    /// <summary>
    /// Gets the todos.
    /// </summary>
    public FakturoidTodosProxy Todos { get; }
    #endregion

    // Public methods

    /// <summary>
    /// Gets the account information.
    /// </summary>
    /// <returns>Instance of <see cref="FakturoidAccount"/> class containing the account information.</returns>
    public async Task<FakturoidAccount> GetAccountInfoAsync() {
        var c = await this.GetHttpClientAsync();
        var r = await c.GetAsync("account.json");
        r.EnsureFakturoidSuccess();
        return await r.Content.FakturoidReadAsAsync<FakturoidAccount>();
    }

    /// <summary>
    /// Get the access token for the Fakturoid API. If not valid, it will obtaint a new one.
    /// </summary>
    /// <returns>Tuple of access token and its validity</returns>
    public async ValueTask<(string accessToken, DateTime accessTokenValidity, string accessTokenType)> GetAccessTokenAsync() {
        if (!IsAccessTokenValid) await this.RefreshAccessTokenAsync(httpClient);
        return (this.accessTokenValue, this.accessTokenRefresh, this.accessTokenType);
    }

    // Non-public methods

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpClient"/> class, initialized for use with Fakturoid API.
    /// Checks validity of the access token and refreshes it if necessary.
    /// </summary>
    /// <param name="forceTokenRefresh">If set to <c>true</c>, forces the refresh of the access token.</param>
    /// <returns>Instance of <see cref="System.Net.Http.HttpClient"/> class, initialized for use with Fakturoid API.</returns>
    internal async ValueTask<HttpClient> GetHttpClientAsync(bool forceTokenRefresh = false) {
        // Get authentication token if needed
        if (!IsAccessTokenValid) await this.RefreshAccessTokenAsync(httpClient);

        // Set authentication header
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(this.accessTokenType, this.accessTokenValue); // Ensure up-to-date authentication header

        return httpClient; // reusable http client
    }

    /// <summary>
    /// Retrieves and sets the access token for the Fakturoid API.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> instance used to make the request.</param>
    /// <exception cref="HttpRequestException">Thrown when the request to get the access token fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response content cannot be read as <see cref="FakturoidAccessToken"/>.</exception>
    private async ValueTask RefreshAccessTokenAsync(HttpClient client) {
        // Prepare request body
        object body = this.useClientCredentials ? new { grant_type = GRANT_CLIENT_CREDENTIALS } : new { grant_type = GRANT_REFRESH_TOKEN, refresh_token = this.refreshTokenValue };

        // Add authentication header
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(":", this.ClientId, this.ClientSecret)));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        // Send request
        var response = await client.FakturoidPostAsJsonAsync(ACCESS_TOKEN_URL, body);
        response.EnsureSuccessStatusCode();

        // Parse response
        var token = await response.Content.FakturoidReadAsAsync<FakturoidAccessToken>();
        this.accessTokenType = token.TokenType;
        this.accessTokenValue = token.AccessToken;
        this.accessTokenRefresh = DateTime.Now.AddSeconds(token.ExpiresIn * ACCESS_TOKEN_REFRESH_MARGIN);
    }

    /// <summary>
    /// Configures the HTTP client for use with the Fakturoid API.
    /// </summary>
    /// <param name="client">HttpClient to be configured</param>
    private void ConfigureHttpClient(HttpClient client) {
        // Setup HTTP client
        var baseAddress = new Uri(string.Format(API_BASE_URL_FORMAT, this.AccountName));
        client.BaseAddress = baseAddress; // should be singleton! this is not correct way to do it, since on each call it will create new instance of HttpClient

        // Set default headers
        client.DefaultRequestHeaders.Add("User-Agent", this.UserAgent);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}

namespace MiniTrading.Domain.Constants;

public static class AppConstant
{
    public static class TradeRules
    {
        public const int SymbolMaxLength = 20;
        public const int TradeIdMaxLength = 50;
        public const string TradeIdPrefix = "TRD";
        public const string TradeIdFormat = "D6";
    }

    public static class Messages
    {
        public const string InvalidSymbol = "INVALID_SYMBOL";
        public const string InvalidQuantity = "QUANTITY_MUST_BE_GREATER_THAN_ZERO";
        public const string PriceUnavailable = "PRICE_UNAVAILABLE_FOR_SYMBOL";
        public const string OrderFilledSuccessfully = "ORDER_FILLED_SUCCESSFULLY";
        public const string OrderRejected = "ORDER_REJECTED";
        public const string SystemHealthy = "SYSTEM_HEALTHY";
        public const string SystemDegraded = "SYSTEM_DEGRADED";
        public const string Connected = "CONNECTED";
        public const string Connecting = "CONNECTING";
        public const string Disconnected = "DISCONNECTED";
        public const string Error = "ERROR";
        public const string TokenNotFound = "TOKEN_NOT_FOUND";
    }

    public static class Calculations
    {
        public const decimal Zero = 0.0m;
        public const decimal Two = 2.0m;
        public const decimal OneHundred = 100.0m;
        public const int TokenLifetimeHours = 24;
    }

    public static class ConfigurationKeys
    {
        public const string ActTraderAuthUrl = "ActTrader:AuthUrl";
        public const string ActTraderWebSocketUrl = "ActTrader:WebSocketUrl";
        public const string ActTraderUsername = "ActTrader:Username";
        public const string ActTraderPassword = "ActTrader:Password";
        public const string DefaultConnection = "MiniTradingDB";
    }

    public static class RestAuth
    {
        public const string EndpointUrl = "http://s138.acttrader.com:10138/api/v2/auth/token";
        public const string ResultProperty = "result";
        public const string TokenProperty = "token";
        public const string DigestScheme = "Digest";
        public const string BasicScheme = "Basic";
        public const string WwwAuthenticateHeader = "WWW-Authenticate";
        public const string AuthorizationHeader = "Authorization";
        public const string UserAgentHeader = "User-Agent";
        public const string UserAgentValue = "Mozilla/5.0";
        public const string AcceptHeader = "Accept";
        public const string AcceptValue = "application/json, text/plain, */*";
        public const string RealmKey = "realm";
        public const string NonceKey = "nonce";
        public const string QopKey = "qop";
        public const string OpaqueKey = "opaque";
        public const string AuthQop = "auth";
        public const string NonceCount = "00000001";
        public const string HttpGetMethod = "GET";
        public const string GuidFormatN = "N";
        public const string EqualsSign = "=";
        public const string QuoteCharString = "\"";
        public const char QuoteChar = '"';
        public const char CommaChar = ',';
        public const char SpaceChar = ' ';
        public const char CarriageReturnChar = '\r';
        public const char NewlineChar = '\n';
        public static readonly char[] DigestParamSeparators = [CommaChar, SpaceChar, CarriageReturnChar, NewlineChar];
    }

    public static class RestMarket
    {
        public const string BaseSymbolsUrl = "http://s138.acttrader.com:10138/api/v2/market/symbols?token=";
    }

    public static class WebSocketFeed
    {
        public const string BaseEndpointUrl = "ws://s138.acttrader.com:22138/ws?token=";
        public const int BufferSize = 8192;
        public const int ReconnectDelayMs = 5000;
        public const string ResultProperty = "result";
        public const string DataProperty = "data";
        public const string SlashSeparator = "/";
        public const string DashSeparator = "-";

        public static readonly string[] SymbolAliases = ["symbol", "Symbol", "instrument", "Instrument", "name", "Name", "sym"];
        public static readonly string[] BuyAliases = ["Buy", "buy", "Ask", "ask"];
        public static readonly string[] SellAliases = ["Sell", "sell", "Bid", "bid"];
        public static readonly string[] LastPriceAliases = ["lastPrice", "LastPrice", "price", "Price", "last", "Last", "rate", "Rate", "close_day_buy_rate"];
        public static readonly string[] ChangePercentAliases = ["changePercent", "ChangePercent", "change", "Change"];
        public static readonly string[] HighAliases = ["high", "High", "h"];
        public static readonly string[] LowAliases = ["low", "Low", "l"];
    }

    public static class Database
    {
        public const string TableNameTrades = "Trades";
    }

    public static class Cors
    {
        public const string AllowFrontendPolicy = "AllowFrontend";
    }

    public static class SwaggerDoc
    {
        public const string RouteTemplate = "openapi/{documentName}.json";
        public const string ApiTitle = "Mini Trading Platform API";
    }

    public static class SignalR
    {
        public const string HubEndpoint = "/hubs/trading";
        public const string ReceivePriceUpdateMethod = "ReceivePriceUpdate";
        public const string ReceiveTradeUpdateMethod = "ReceiveTradeUpdate";
    }
}
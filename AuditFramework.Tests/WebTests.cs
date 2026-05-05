using System.Net.Http.Json;
using Aspire.Hosting;
using Microsoft.Extensions.Logging;

namespace AuditFramework.Tests;

public class WebTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        await using var app = await StartApplicationAsync();

        var httpClient = app.CreateHttpClient("webfrontend");
        var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetApiWeatherForecastReturnsFiveItems()
    {
        await using var app = await StartApplicationAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync(
            "/weatherforecast",
            TestContext.Current.CancellationToken
        );

        response.EnsureSuccessStatusCode();

        var forecast = await response.Content.ReadFromJsonAsync<WeatherForecastDto[]>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(forecast);
        Assert.Equal(5, forecast.Length);
        Assert.All(
            forecast,
            item =>
            {
                Assert.NotEqual(default, item.Date);
                Assert.False(string.IsNullOrWhiteSpace(item.Summary));
            }
        );
    }

    [Theory]
    [InlineData("webfrontend")]
    [InlineData("apiservice")]
    public async Task HealthEndpointReturnsOkForEachService(string resourceName)
    {
        await using var app = await StartApplicationAsync();

        var httpClient = app.CreateHttpClient(resourceName);
        var response = await httpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<DistributedApplication> StartApplicationAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.AuditFramework_AppHost>(
                cancellationToken
            );
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var app = await appHost
            .BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        await app
            .ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await app
            .ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        return app;
    }

    private sealed record WeatherForecastDto(
        DateOnly Date,
        int TemperatureC,
        int TemperatureF,
        string? Summary
    );
}

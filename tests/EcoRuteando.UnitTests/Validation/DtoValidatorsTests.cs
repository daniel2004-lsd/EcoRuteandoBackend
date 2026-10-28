using EcoRuteando.Modules.Admin.Application.Exports.Commands.RequestExport;
using EcoRuteando.Modules.Admin.Application.Factors.Commands.CreateTransportFactor;
using EcoRuteando.Modules.Admin.Application.Support.Commands.CreateTicket;
using EcoRuteando.Modules.Admin.Application.System.Commands.UpdateSystemConfiguration;
using EcoRuteando.Modules.Admin.Domain.Enums;
using EcoRuteando.Modules.Integrations.Application.Cache.Commands.SetCachedResponse;
using EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestAirQuality;
using EcoRuteando.Modules.Integrations.Application.Environmental.Commands.IngestWeather;
using EcoRuteando.Modules.Notifications.Application.Notifications.Commands.RegisterDeviceToken;
using EcoRuteando.Modules.Notifications.Application.Notifications.Commands.SendNotification;
using EcoRuteando.Modules.Notifications.Application.Templates.Commands.CreateNotificationTemplate;
using EcoRuteando.Modules.Notifications.Domain.Enums;
using Xunit;

namespace EcoRuteando.UnitTests.Validation;

public class DtoValidatorsTests
{
    // --- Admin Module Validators ---
    [Fact]
    public void CreateTransportFactorCommandValidator_ValidatesCorrectly()
    {
        var validator = new CreateTransportFactorCommandValidator();

        var valid = new CreateTransportFactorCommand(TransportType.Bike, 0.0m, 50.0m, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "Manual");
        Assert.True(validator.Validate(valid).IsValid);

        var invalidCo2 = valid with { Co2FactorKgKm = -1m };
        Assert.False(validator.Validate(invalidCo2).IsValid);

        var invalidDateRange = valid with { ValidFrom = DateTime.UtcNow.AddDays(10), ValidUntil = DateTime.UtcNow };
        Assert.False(validator.Validate(invalidDateRange).IsValid);
    }

    [Fact]
    public void UpdateSystemConfigurationCommandValidator_ValidatesCorrectly()
    {
        var validator = new UpdateSystemConfigurationCommandValidator();

        var valid = new UpdateSystemConfigurationCommand(
            "EcoRuteando", "admin@ecoruteando.com", "3001234567",
            "1.0", "https://ecoruteando.com/terms", "1.0", "https://ecoruteando.com/privacy",
            "maps,weather", 3600, 1800, "#10b981", "#059669", null, null);
        Assert.True(validator.Validate(valid).IsValid);

        var emptyName = valid with { AppName = "" };
        Assert.False(validator.Validate(emptyName).IsValid);

        var invalidEmail = valid with { AdminEmail = "not-an-email" };
        Assert.False(validator.Validate(invalidEmail).IsValid);

        var negativeTtl = valid with { MapCacheTtlSec = -10 };
        Assert.False(validator.Validate(negativeTtl).IsValid);
    }

    [Fact]
    public void CreateTicketCommandValidator_ValidatesCorrectly()
    {
        var validator = new CreateTicketCommandValidator();

        var valid = new CreateTicketCommand(Guid.NewGuid(), "Asunto de prueba", "Mensaje detallado con la consulta", 2);
        Assert.True(validator.Validate(valid).IsValid);

        var emptySubject = valid with { Subject = "" };
        Assert.False(validator.Validate(emptySubject).IsValid);

        var emptyMessage = valid with { Message = "" };
        Assert.False(validator.Validate(emptyMessage).IsValid);

        var invalidPriority = valid with { Priority = 0 };
        Assert.False(validator.Validate(invalidPriority).IsValid);

        var invalidHighPriority = valid with { Priority = 5 };
        Assert.False(validator.Validate(invalidHighPriority).IsValid);
    }

    // --- Notifications Module Validators ---
    [Fact]
    public void RegisterDeviceTokenCommandValidator_ValidatesCorrectly()
    {
        var validator = new RegisterDeviceTokenCommandValidator();

        var valid = new RegisterDeviceTokenCommand(Guid.NewGuid(), "android", "ExponentPushToken[xxxxxxxxxxxx]");
        Assert.True(validator.Validate(valid).IsValid);

        var emptyToken = valid with { ExpoPushToken = "" };
        Assert.False(validator.Validate(emptyToken).IsValid);

        var emptyPlatform = valid with { Platform = "" };
        Assert.False(validator.Validate(emptyPlatform).IsValid);

        var emptyUser = valid with { UserId = Guid.Empty };
        Assert.False(validator.Validate(emptyUser).IsValid);
    }

    [Fact]
    public void SendNotificationCommandValidator_ValidatesCorrectly()
    {
        var validator = new SendNotificationCommandValidator();

        var valid = new SendNotificationCommand("Titulo Notificacion", "Mensaje de alerta para el usuario", NotificationType.Alert, NotificationChannel.Push);
        Assert.True(validator.Validate(valid).IsValid);

        var emptyTitle = valid with { Title = "" };
        Assert.False(validator.Validate(emptyTitle).IsValid);

        var emptyMsg = valid with { Message = "" };
        Assert.False(validator.Validate(emptyMsg).IsValid);
    }

    [Fact]
    public void CreateNotificationTemplateCommandValidator_ValidatesCorrectly()
    {
        var validator = new CreateNotificationTemplateCommandValidator();

        var valid = new CreateNotificationTemplateCommand("WelcomeTemplate", NotificationType.System, "Bienvenido", "<p>Hola</p>", "Hola");
        Assert.True(validator.Validate(valid).IsValid);

        var emptyName = valid with { Name = "" };
        Assert.False(validator.Validate(emptyName).IsValid);
    }

    // --- Integrations Module Validators ---
    [Fact]
    public void SetCachedResponseCommandValidator_ValidatesCorrectly()
    {
        var validator = new SetCachedResponseCommandValidator();

        var valid = new SetCachedResponseCommand("GoogleMaps", "/directions", "origin=A&dest=B", "{\"status\":\"OK\"}", 3600);
        Assert.True(validator.Validate(valid).IsValid);

        var emptyProvider = valid with { Provider = "" };
        Assert.False(validator.Validate(emptyProvider).IsValid);

        var emptyResponse = valid with { Response = "" };
        Assert.False(validator.Validate(emptyResponse).IsValid);

        var zeroTtl = valid with { TtlSeconds = 0 };
        Assert.False(validator.Validate(zeroTtl).IsValid);

        var excessiveTtl = valid with { TtlSeconds = 100000 };
        Assert.False(validator.Validate(excessiveTtl).IsValid);
    }

    [Fact]
    public void IngestWeatherCommandValidator_ValidatesCorrectly()
    {
        var validator = new IngestWeatherCommandValidator();

        var valid = new IngestWeatherCommand(
            Latitude: 2.9273,
            Longitude: -75.2819,
            City: "Neiva",
            TemperatureC: 32.5m,
            HumidityPct: 65,
            WindSpeedKmh: 12.0m,
            Condition: "Soleado",
            RainProbability: 10,
            RawData: "{}",
            Source: "OpenWeather",
            ValidFrom: DateTime.UtcNow,
            ValidUntil: DateTime.UtcNow.AddHours(3));
        Assert.True(validator.Validate(valid).IsValid);

        var invalidLat = valid with { Latitude = 95.0 };
        Assert.False(validator.Validate(invalidLat).IsValid);

        var invalidLng = valid with { Longitude = -190.0 };
        Assert.False(validator.Validate(invalidLng).IsValid);

        var invalidHumidity = valid with { HumidityPct = 120 };
        Assert.False(validator.Validate(invalidHumidity).IsValid);

        var invalidDateRange = valid with { ValidFrom = DateTime.UtcNow.AddHours(4), ValidUntil = DateTime.UtcNow };
        Assert.False(validator.Validate(invalidDateRange).IsValid);
    }

    [Fact]
    public void IngestAirQualityCommandValidator_ValidatesCorrectly()
    {
        var validator = new IngestAirQualityCommandValidator();

        var valid = new IngestAirQualityCommand(
            Latitude: 2.9273,
            Longitude: -75.2819,
            City: "Neiva",
            Aqi: 45,
            Pm25: 12.5m,
            Pm10: 25.0m,
            Co2Ppm: 410.0m,
            Source: "OpenAQ",
            RawData: "{}");
        Assert.True(validator.Validate(valid).IsValid);

        var negativeAqi = valid with { Aqi = -5 };
        Assert.False(validator.Validate(negativeAqi).IsValid);

        var excessiveAqi = valid with { Aqi = 550 };
        Assert.False(validator.Validate(excessiveAqi).IsValid);

        var negativePm = valid with { Pm25 = -1m };
        Assert.False(validator.Validate(negativePm).IsValid);
    }
}

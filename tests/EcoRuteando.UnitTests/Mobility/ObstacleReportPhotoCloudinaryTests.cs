using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CloudinaryDotNet.Actions;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Storage;
using EcoRuteando.Modules.Mobility.Application.ObstacleReports.Commands.CreateObstacleReport;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Modules.Mobility.Infrastructure.Storage;
using EcoRuteando.Modules.Mobility.Presentation.Controllers;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Authorization;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility;

public sealed class ObstacleReportPhotoCloudinaryTests
{
    // 1x1 PNG válido (67 bytes con firma 89 50 4E 47 0D 0A 1A 0A)
    private static readonly byte[] ValidPngBytes =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
        0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
        0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
        0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
        0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
        0x42, 0x60, 0x82
    ];

    // JPEG mínimo con cabecera SOI + APP0 (FF D8 FF E0)
    private static readonly byte[] ValidJpegBytes =
    [
        0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
        0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01,
        0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9
    ];

    // WEBP mínimo con firma RIFF....WEBP
    private static readonly byte[] ValidWebpBytes =
    [
        0x52, 0x49, 0x46, 0x46, 0x10, 0x00, 0x00, 0x00,
        0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x20,
        0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    private static IOptions<CloudinaryOptions> CreateConfiguredOptions() =>
        Options.Create(new CloudinaryOptions
        {
            CloudName = "demo-cloud-test",
            ApiKey = "1234567890",
            ApiSecret = "simulated-secret-for-unit-test-only",
            Folder = "ecoruteando/reports",
            TimeoutSeconds = 10
        });

    private static ObstacleReportsController CreateControllerWithUser(
        IMediator mediator,
        IPhotoStorageService photoStorageService,
        Guid userId)
    {
        var controller = new ObstacleReportsController(mediator, photoStorageService);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
        return controller;
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    // 1. Imagen válida -> HTTP 201 y 9. URL HTTPS devuelta (nunca /uploads/reports/...)
    [Theory]
    [InlineData("evidencia.png", "image/png")]
    [InlineData("evidencia.jpg", "image/jpeg")]
    [InlineData("evidencia.jpeg", "image/jpeg")]
    [InlineData("evidencia.webp", "image/webp")]
    public async Task UploadPhoto_WithValidImage_UploadsToCloudinaryFolderAndReturns201WithHttpsUrl(
        string fileName,
        string contentType)
    {
        var bytes = contentType switch
        {
            "image/png" => ValidPngBytes,
            "image/jpeg" => ValidJpegBytes,
            "image/webp" => ValidWebpBytes,
            _ => ValidPngBytes
        };

        ImageUploadParams? capturedParams = null;
        var sdkMock = new Mock<ICloudinarySdkClient>();
        sdkMock.SetupGet(x => x.IsConfigured).Returns(true);
        sdkMock
            .Setup(x => x.UploadImageAsync(It.IsAny<ImageUploadParams>(), It.IsAny<CancellationToken>()))
            .Callback<ImageUploadParams, CancellationToken>((p, _) => capturedParams = p)
            .ReturnsAsync((ImageUploadParams p, CancellationToken _) => new ImageUploadResult
            {
                StatusCode = HttpStatusCode.OK,
                PublicId = $"{p.Folder}/{p.PublicId}",
                SecureUrl = new Uri($"https://res.cloudinary.com/demo-cloud-test/image/upload/v1/{p.Folder}/{p.PublicId}{Path.GetExtension(fileName)}")
            });

        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var mediatorMock = new Mock<IMediator>();
        var controller = CreateControllerWithUser(mediatorMock.Object, storageService, Guid.NewGuid());
        var formFile = CreateFormFile(bytes, fileName, contentType);

        var result = await controller.UploadPhoto(formFile, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);

        var json = JsonSerializer.Serialize(objectResult.Value);
        using var doc = JsonDocument.Parse(json);
        var url = doc.RootElement.GetProperty("url").GetString();
        var returnedFileName = doc.RootElement.GetProperty("fileName").GetString();

        Assert.NotNull(url);
        Assert.StartsWith("https://res.cloudinary.com/demo-cloud-test/image/upload/", url);
        Assert.Contains("/ecoruteando/reports/", url);
        Assert.DoesNotContain("/uploads/reports/", url);
        Assert.NotNull(returnedFileName);
        Assert.EndsWith(Path.GetExtension(fileName).ToLowerInvariant(), returnedFileName);

        Assert.NotNull(capturedParams);
        Assert.Equal("ecoruteando/reports", capturedParams!.Folder);
        Assert.False(capturedParams.UseFilename);
        Assert.False(capturedParams.UniqueFilename);
        Assert.False(capturedParams.Overwrite);
    }

    // 2. Archivo vacío -> HTTP 400
    [Fact]
    public async Task UploadPhoto_WithEmptyFileOrNull_Returns400BadRequest()
    {
        var sdkMock = new Mock<ICloudinarySdkClient>(MockBehavior.Strict);
        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var controller = CreateControllerWithUser(new Mock<IMediator>().Object, storageService, Guid.NewGuid());

        var nullResult = await controller.UploadPhoto(null, CancellationToken.None);
        var nullBadRequest = Assert.IsType<BadRequestObjectResult>(nullResult);
        Assert.Equal(StatusCodes.Status400BadRequest, nullBadRequest.StatusCode);

        var emptyFile = CreateFormFile([], "empty.png", "image/png");
        var emptyResult = await controller.UploadPhoto(emptyFile, CancellationToken.None);
        var emptyBadRequest = Assert.IsType<BadRequestObjectResult>(emptyResult);
        Assert.Equal(StatusCodes.Status400BadRequest, emptyBadRequest.StatusCode);
    }

    // 3. Archivo superior a 5 MB -> HTTP 400
    [Fact]
    public async Task UploadPhoto_WithFileExceeding5MB_Returns400BadRequest()
    {
        var sdkMock = new Mock<ICloudinarySdkClient>(MockBehavior.Strict);
        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var controller = CreateControllerWithUser(new Mock<IMediator>().Object, storageService, Guid.NewGuid());

        var oversizedBytes = new byte[(5 * 1024 * 1024) + 1];
        Array.Copy(ValidPngBytes, oversizedBytes, ValidPngBytes.Length);
        var oversizedFile = CreateFormFile(oversizedBytes, "oversized.png", "image/png");

        var result = await controller.UploadPhoto(oversizedFile, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        Assert.Contains("5 MB", JsonSerializer.Serialize(badRequest.Value));
    }

    // 4. MIME inválido -> HTTP 400
    [Theory]
    [InlineData("photo.png", "text/plain")]
    [InlineData("photo.jpg", "application/pdf")]
    [InlineData("photo.png", "image/jpeg")] // MIME no coincide con extensión
    public async Task UploadPhoto_WithInvalidOrMismatchedMime_Returns400BadRequest(
        string fileName,
        string contentType)
    {
        var sdkMock = new Mock<ICloudinarySdkClient>(MockBehavior.Strict);
        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var controller = CreateControllerWithUser(new Mock<IMediator>().Object, storageService, Guid.NewGuid());
        var file = CreateFormFile(ValidPngBytes, fileName, contentType);

        var result = await controller.UploadPhoto(file, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    // 5. Extensión inválida o firma binaria falsificada (magic bytes spoofing) -> HTTP 400
    [Theory]
    [InlineData("malware.exe", "image/png")]
    [InlineData("vector.svg", "image/png")]
    [InlineData("anim.gif", "image/png")]
    public async Task UploadPhoto_WithInvalidExtension_Returns400BadRequest(
        string fileName,
        string contentType)
    {
        var sdkMock = new Mock<ICloudinarySdkClient>(MockBehavior.Strict);
        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var controller = CreateControllerWithUser(new Mock<IMediator>().Object, storageService, Guid.NewGuid());
        var file = CreateFormFile(ValidPngBytes, fileName, contentType);

        var result = await controller.UploadPhoto(file, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [Fact]
    public async Task UploadPhoto_WithSpoofedFileSignature_Returns400BadRequestWithoutCallingCloudinary()
    {
        var sdkMock = new Mock<ICloudinarySdkClient>(MockBehavior.Strict);
        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var controller = CreateControllerWithUser(new Mock<IMediator>().Object, storageService, Guid.NewGuid());

        // Extensión .png y MIME image/png, pero contenido binario ejecutable MZ / texto arbitrario
        var fakeBytes = Encoding.UTF8.GetBytes("MZ-fake-executable-payload-not-a-png");
        var spoofedFile = CreateFormFile(fakeBytes, "spoofed.png", "image/png");

        var result = await controller.UploadPhoto(spoofedFile, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        Assert.Contains("binario", JsonSerializer.Serialize(badRequest.Value));
    }

    // 6. Sin autenticación -> HTTP 401 y 7. Sin permiso -> HTTP 403 en pipeline HTTP real
    [Fact]
    public async Task HttpPipeline_WhenUnauthenticatedOrMissingPermission_Returns401And403And201WhenAuthorized()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        var photoStorageMock = new Mock<IPhotoStorageService>();
        photoStorageMock
            .Setup(x => x.UploadReportPhotoAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadResult(
                "https://res.cloudinary.com/demo-cloud-test/image/upload/v1/ecoruteando/reports/abc12345.png",
                "abc12345.png",
                "ecoruteando/reports/abc12345"));

        builder.Services.AddSingleton(photoStorageMock.Object);
        builder.Services.AddSingleton(new Mock<IMediator>().Object);

        builder.Services
            .AddAuthentication("TestScheme")
            .AddScheme<AuthenticationSchemeOptions, TestHeaderAuthHandler>("TestScheme", null);

        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, TestHeaderPermissionHandler>();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(ObstacleReportsController).Assembly);

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.StartAsync();
        var baseAddress = app.Urls.First();
        using var client = new HttpClient { BaseAddress = new Uri(baseAddress) };

        MultipartFormDataContent BuildMultipart()
        {
            var mp = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(ValidPngBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            mp.Add(byteContent, "file", "test.png");
            return mp;
        }

        // Caso 6: Sin autenticación -> HTTP 401
        using (var req401 = new HttpRequestMessage(HttpMethod.Post, "/api/obstacle-reports/photo")
        {
            Content = BuildMultipart()
        })
        {
            var resp401 = await client.SendAsync(req401);
            Assert.Equal(HttpStatusCode.Unauthorized, resp401.StatusCode);
        }

        // Caso 7: Autenticado pero sin permiso reports.create -> HTTP 403
        using (var req403 = new HttpRequestMessage(HttpMethod.Post, "/api/obstacle-reports/photo")
        {
            Content = BuildMultipart()
        })
        {
            req403.Headers.Add("X-Test-UserId", Guid.NewGuid().ToString());
            req403.Headers.Add("X-Test-Permissions", "reports.read");
            var resp403 = await client.SendAsync(req403);
            Assert.Equal(HttpStatusCode.Forbidden, resp403.StatusCode);
        }

        // Caso autorizado con permiso reports.create -> HTTP 201
        using (var req201 = new HttpRequestMessage(HttpMethod.Post, "/api/obstacle-reports/photo")
        {
            Content = BuildMultipart()
        })
        {
            req201.Headers.Add("X-Test-UserId", Guid.NewGuid().ToString());
            req201.Headers.Add("X-Test-Permissions", "reports.create");
            var resp201 = await client.SendAsync(req201);
            Assert.Equal(HttpStatusCode.Created, resp201.StatusCode);
            var body201 = await resp201.Content.ReadAsStringAsync();
            Assert.Contains("https://res.cloudinary.com/demo-cloud-test/image/upload/v1/ecoruteando/reports/abc12345.png", body201);
        }

        await app.StopAsync();
    }

    // 8. Error del proveedor -> error controlado sin filtrar secretos ni detalles internos
    [Fact]
    public async Task UploadPhoto_WhenProviderFailsOrReturnsError_ReturnsControlled502WithoutLeakingSecrets()
    {
        var sdkMock = new Mock<ICloudinarySdkClient>();
        sdkMock.SetupGet(x => x.IsConfigured).Returns(true);
        sdkMock
            .Setup(x => x.UploadImageAsync(It.IsAny<ImageUploadParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageUploadResult
            {
                StatusCode = HttpStatusCode.BadRequest,
                Error = new Error { Message = "Internal Cloudinary SDK signature error with api_secret=SUPER_SECRET_123" }
            });

        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var controller = CreateControllerWithUser(new Mock<IMediator>().Object, storageService, Guid.NewGuid());
        var file = CreateFormFile(ValidPngBytes, "photo.png", "image/png");

        var result = await controller.UploadPhoto(file, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);

        var payload = JsonSerializer.Serialize(objectResult.Value);
        using var doc = JsonDocument.Parse(payload);
        var message = doc.RootElement.GetProperty("message").GetString();

        Assert.DoesNotContain("SUPER_SECRET_123", payload);
        Assert.DoesNotContain("simulated-secret-for-unit-test-only", payload);
        Assert.NotNull(message);
        Assert.Contains("No fue posible almacenar la fotografía", message);
    }

    [Fact]
    public async Task UploadPhoto_WhenCredentialsNotConfigured_ReturnsControlled503()
    {
        var unconfiguredOptions = Options.Create(new CloudinaryOptions());
        var sdkAdapter = new CloudinarySdkClientAdapter(unconfiguredOptions);
        var storageService = new CloudinaryPhotoStorageService(
            sdkAdapter,
            unconfiguredOptions,
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        var controller = CreateControllerWithUser(new Mock<IMediator>().Object, storageService, Guid.NewGuid());
        var file = CreateFormFile(ValidPngBytes, "photo.png", "image/png");

        var result = await controller.UploadPhoto(file, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
    }

    // 10. Creación del reporte con fotografía (y limpieza de huérfanos solo de la operación actual)
    [Fact]
    public async Task CreateObstacleReport_WithCloudinaryPhotoUrl_PersistsReportAndReturns201()
    {
        var userId = Guid.NewGuid();
        var cloudinaryUrl = "https://res.cloudinary.com/demo-cloud-test/image/upload/v1/ecoruteando/reports/9f8e7d6c5b4a.png";

        // Validar con CreateObstacleReportCommandValidator
        var validator = new CreateObstacleReportCommandValidator();
        var command = new CreateObstacleReportCommand(
            ReportType: "bache",
            Description: "Bache profundo en ciclorruta",
            Latitude: 4.60971,
            Longitude: -74.08175,
            AddressText: "Av. El Dorado # 68",
            UserId: userId,
            PhotoUrl: cloudinaryUrl);

        var validationResult = await validator.ValidateAsync(command);
        Assert.True(validationResult.IsValid);

        // Ejecutar CreateObstacleReportCommandHandler
        ObstacleReport? persistedReport = null;
        var repoMock = new Mock<IObstacleReportRepository>();
        repoMock
            .Setup(r => r.AddAsync(It.IsAny<ObstacleReport>(), It.IsAny<CancellationToken>()))
            .Callback<ObstacleReport, CancellationToken>((rep, _) => persistedReport = rep)
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IMobilityUnitOfWork>();
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateObstacleReportCommandHandler(repoMock.Object, uowMock.Object);
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateObstacleReportCommand>(), It.IsAny<CancellationToken>()))
            .Returns((CreateObstacleReportCommand cmd, CancellationToken ct) => handler.Handle(cmd, ct));

        var storageMock = new Mock<IPhotoStorageService>();
        var controller = CreateControllerWithUser(mediatorMock.Object, storageMock.Object, userId);

        var actionResult = await controller.Create(command, CancellationToken.None);

        var createdResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.NotNull(persistedReport);
        Assert.Equal(cloudinaryUrl, persistedReport!.PhotoUrl);
        Assert.Equal((short)50, persistedReport.ConfidenceScore);

        storageMock.Verify(s => s.MarkPhotoCommitted(cloudinaryUrl), Times.Once);
    }

    [Fact]
    public async Task CreateObstacleReport_WhenCreationFails_DeletesOnlyUncommittedPhotoFromCurrentOperationAndPreservesHistoricalPhotos()
    {
        var userId = Guid.NewGuid();
        var destroyedPublicIds = new List<string>();

        var sdkMock = new Mock<ICloudinarySdkClient>();
        sdkMock.SetupGet(x => x.IsConfigured).Returns(true);
        sdkMock
            .Setup(x => x.UploadImageAsync(It.IsAny<ImageUploadParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageUploadParams p, CancellationToken _) => new ImageUploadResult
            {
                StatusCode = HttpStatusCode.OK,
                PublicId = $"{p.Folder}/{p.PublicId}",
                SecureUrl = new Uri($"https://res.cloudinary.com/demo-cloud-test/image/upload/v1/{p.Folder}/{p.PublicId}.png")
            });
        sdkMock
            .Setup(x => x.DestroyImageAsync(It.IsAny<DeletionParams>(), It.IsAny<CancellationToken>()))
            .Callback<DeletionParams, CancellationToken>((dp, _) => destroyedPublicIds.Add(dp.PublicId))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        var storageService = new CloudinaryPhotoStorageService(
            sdkMock.Object,
            CreateConfiguredOptions(),
            NullLogger<CloudinaryPhotoStorageService>.Instance);

        // 1. Subir imagen en la operación actual
        await using var stream = new MemoryStream(ValidPngBytes);
        var uploaded = await storageService.UploadReportPhotoAsync(
            stream,
            "current_op.png",
            "image/png",
            userId,
            CancellationToken.None);

        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateObstacleReportCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Error simulado al crear el reporte."));

        var controller = CreateControllerWithUser(mediatorMock.Object, storageService, userId);

        // 2. Falla la creación con una imagen histórica que NO pertenece a la operación actual -> NO debe borrarse
        var historicalUrl = "https://res.cloudinary.com/demo-cloud-test/image/upload/v1/ecoruteando/reports/historical_photo_001.png";
        await Assert.ThrowsAsync<DomainException>(() =>
            controller.Create(
                new CreateObstacleReportCommand("bache", "desc", 4.6, -74.0, null, userId, historicalUrl),
                CancellationToken.None));

        Assert.Empty(destroyedPublicIds);

        // 3. Falla la creación con la imagen recién subida en la operación actual por el mismo usuario -> SÍ se limpia el huérfano
        await Assert.ThrowsAsync<DomainException>(() =>
            controller.Create(
                new CreateObstacleReportCommand("bache", "desc", 4.6, -74.0, null, userId, uploaded.Url),
                CancellationToken.None));

        Assert.Single(destroyedPublicIds);
        Assert.Equal(uploaded.PublicId, destroyedPublicIds[0]);
    }

    private sealed class TestHeaderAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestHeaderAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-UserId", out var userIdValues) ||
                string.IsNullOrWhiteSpace(userIdValues.FirstOrDefault()))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userIdValues.First()!)
            };

            if (Request.Headers.TryGetValue("X-Test-Permissions", out var permValues))
            {
                foreach (var perm in permValues.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    claims.Add(new Claim("permission", perm));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private sealed class TestHeaderPermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

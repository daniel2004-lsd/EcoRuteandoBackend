using EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

/// <summary>
/// Servicios de Google Maps: Directions, Geocoding y Reverse Geocoding.
/// </summary>
[ApiController]
[Route("api/maps")]
public sealed class GoogleMapsController : ControllerBase
{
    private static readonly HashSet<string> AllowedPlaceTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "restaurant", "hotel", "church", "park", "cafe", "bar", "museum"
        };

    private const double MaxPlacesRadius = 50_000;

    private readonly IGoogleMapsService _googleMapsService;

    public GoogleMapsController(IGoogleMapsService googleMapsService)
    {
        _googleMapsService = googleMapsService;
    }

    /// <summary>
    /// Calcula una ruta entre origen y destino usando Google Directions API.
    /// Modos de transporte: driving, walking, bicycling, transit.
    /// </summary>
    [HttpGet("directions")]
    public async Task<IActionResult> GetDirections(
        [FromQuery] double originLat,
        [FromQuery] double originLng,
        [FromQuery] double destinationLat,
        [FromQuery] double destinationLng,
        [FromQuery] string travelMode,
        CancellationToken cancellationToken)
    {
        var validModes = new[] { "driving", "walking", "bicycling", "transit" };
        if (!validModes.Contains(travelMode.ToLowerInvariant()))
        {
            return BadRequest(new
            {
                message = $"El modo de transporte '{travelMode}' no es válido. " +
                          "Valores permitidos: driving, walking, bicycling, transit."
            });
        }

        var result = await _googleMapsService.GetDirectionsAsync(
            originLat,
            originLng,
            destinationLat,
            destinationLng,
            travelMode.ToLowerInvariant(),
            cancellationToken);

        if (result is null)
        {
            return StatusCode(502, new
            {
                message = "No se pudo obtener la ruta desde Google Maps."
            });
        }

        if (result.Status == "ZERO_RESULTS")
        {
            var (friendlyMode, suggestion) = travelMode switch
            {
                "bicycling" => ("en bicicleta", "Intenta a pie o en automóvil."),
                "transit" => ("en transporte público", "En Neiva puede que no haya rutas de bus disponibles para este trayecto. Intenta a pie o en automóvil."),
                "driving" => ("en automóvil", "Intenta caminando."),
                _ => ("a pie", "Intenta en automóvil.")
            };

            return NotFound(new
            {
                message = $"No existe una ruta {friendlyMode} entre el origen y el destino. {suggestion}"
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Convierte una dirección a coordenadas usando Google Geocoding API.
    /// Ejemplo: "Neiva, Huila, Colombia"
    /// </summary>
    [HttpGet("geocode")]
    public async Task<IActionResult> Geocode(
        [FromQuery] string address,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(new
            {
                message = "La dirección es obligatoria."
            });
        }

        var result = await _googleMapsService.GeocodeAddressAsync(
            address,
            cancellationToken);

        if (result is null)
        {
            return StatusCode(502, new
            {
                message = "No se pudo geolocalizar la dirección desde Google Maps."
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Convierte coordenadas a dirección usando Google Reverse Geocoding API.
    /// </summary>
    [HttpGet("reverse-geocode")]
    public async Task<IActionResult> ReverseGeocode(
        [FromQuery] double lat,
        [FromQuery] double lng,
        CancellationToken cancellationToken)
    {
        var result = await _googleMapsService.ReverseGeocodeAsync(
            lat,
            lng,
            cancellationToken);

        if (result is null)
        {
            return StatusCode(502, new
            {
                message = "No se pudo obtener la dirección desde Google Maps."
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Busca lugares cercanos de un tipo permitido (accesible a invitados, ver CU17).
    /// </summary>
    [HttpGet("places")]
    public async Task<IActionResult> GetPlacesNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] string type,
        [FromQuery] double radius = 1500,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(type))
            return BadRequest(new { message = "El tipo de lugar es obligatorio (ej: restaurant, hotel, church)." });

        var normalizedType = type.Trim().ToLowerInvariant();
        if (!AllowedPlaceTypes.Contains(normalizedType))
            return BadRequest(new
            {
                message = $"El tipo '{type}' no está permitido.",
                allowedTypes = AllowedPlaceTypes
            });

        if (lat is < -90 or > 90 || lng is < -180 or > 180)
            return BadRequest(new { message = "Las coordenadas están fuera del rango válido." });

        if (radius is <= 0 or > MaxPlacesRadius)
            return BadRequest(new { message = $"El radio debe estar entre 1 y {MaxPlacesRadius} metros." });

        var result = await _googleMapsService.GetPlacesNearbyAsync(lat, lng, normalizedType, radius, cancellationToken);
        if (result is null) return StatusCode(502, new { message = "No se pudieron obtener los lugares." });
        return Ok(result);
    }
}

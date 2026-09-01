using EcoRuteando.Modules.Mobility.Application.Abstractions.GoogleMaps;
using EcoRuteando.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRuteando.Modules.Mobility.Presentation.Controllers;

/// <summary>
/// Servicios de Google Maps: Directions, Geocoding y Reverse Geocoding.
/// </summary>
[ApiController]
[Route("api/maps")]
[Authorize]
public sealed class GoogleMapsController : ControllerBase
{
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
    [HasPermission("routes.read")]
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
            var friendlyMode = travelMode switch
            {
                "walking" => "a pie",
                "bicycling" => "en bicicleta",
                "transit" => "en transporte público",
                "driving" => "en automóvil",
                _ => $"en modo {travelMode}"
            };

            return NotFound(new
            {
                message = $"No existe una ruta {friendlyMode} entre el origen y el destino. Prueba con otro modo de transporte."
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Convierte una dirección a coordenadas usando Google Geocoding API.
    /// Ejemplo: "Neiva, Huila, Colombia"
    /// </summary>
    [HttpGet("geocode")]
    [HasPermission("routes.read")]
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
    [HasPermission("routes.read")]
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
}
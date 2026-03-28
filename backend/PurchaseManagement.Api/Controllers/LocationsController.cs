using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Api.Services;

namespace PurchaseManagement.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController(IMasterDataService masterData) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await masterData.GetLocationsAsync(cancellationToken));
}

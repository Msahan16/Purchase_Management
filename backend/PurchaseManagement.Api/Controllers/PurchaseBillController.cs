using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Services;

namespace PurchaseManagement.Api.Controllers;

[ApiController]
[Route("api/purchase-bill")]
public class PurchaseBillController(IPurchaseBillService bills, IPurchaseBillPdfService pdf) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await bills.ListAsync(cancellationToken));

    [HttpGet("{id:int}", Name = "GetPurchaseBill")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var bill = await bills.GetByIdAsync(id, cancellationToken);
        return bill == null ? NotFound() : Ok(bill);
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> Pdf(int id, CancellationToken cancellationToken)
    {
        var bill = await bills.GetByIdAsync(id, cancellationToken);
        if (bill == null)
            return NotFound();
        var bytes = pdf.GeneratePdf(bill);
        return File(bytes, "application/pdf", $"purchase-bill-{id}.pdf");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseBillSaveDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await bills.CreateAsync(dto, cancellationToken);
            return result.WasCreated
                ? CreatedAtRoute("GetPurchaseBill", new { id = result.Bill.Id }, result.Bill)
                : Ok(result.Bill);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return Problem(
                detail: ex.InnerException?.Message ?? ex.Message,
                title: "Database error while saving purchase bill");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseBillSaveDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await bills.UpdateAsync(id, dto, cancellationToken);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return Problem(
                detail: ex.InnerException?.Message ?? ex.Message,
                title: "Database error while updating purchase bill");
        }
    }
}

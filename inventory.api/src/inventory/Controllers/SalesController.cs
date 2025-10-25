using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inventory.Data;
using inventory.Models;
using inventory.DTOs;

namespace inventory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public SalesController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Stock)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound();
            }

            return sale;
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<ActionResult<Sale>> CreateSale(CreateSaleDTO saleDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if customer exists
                var customer = await _context.Customers.FindAsync(saleDTO.CustomerId);
                if (customer == null)
                {
                    return BadRequest("Customer not found");
                }

                // Generate invoice number (you might want to implement a more sophisticated logic)
                var lastInvoiceNumber = await _context.Sales
                    .OrderByDescending(s => s.InvoiceNumber)
                    .Select(s => s.InvoiceNumber)
                    .FirstOrDefaultAsync() ?? "INV-0000";

                var nextNumber = int.Parse(lastInvoiceNumber.Split('-')[1]) + 1;
                var newInvoiceNumber = $"INV-{nextNumber:D4}";

                // Create sale
                var sale = new Sale
                {
                    CustomerId = saleDTO.CustomerId,
                    InvoiceNumber = newInvoiceNumber,
                    SaleDate = DateTime.UtcNow,
                    Notes = saleDTO.Notes,
                    PaymentMethod = saleDTO.PaymentMethod,
                    PaymentDueDate = saleDTO.PaymentDueDate,
                    Status = "Pending",
                    PaymentStatus = "Pending"
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                decimal subTotal = 0;
                
                // Process each sale item
                foreach (var itemDTO in saleDTO.Items)
                {
                    var stock = await _context.Stock.FindAsync(itemDTO.StockId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException($"Stock item {itemDTO.StockId} not found");
                    }

                    if (stock.CurrentQuantity < itemDTO.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient quantity for {stock.ItemName}");
                    }

                    // Create sale item
                    var saleItem = new SaleItem
                    {
                        SaleId = sale.SaleId,
                        StockId = itemDTO.StockId,
                        Quantity = itemDTO.Quantity,
                        UnitPrice = itemDTO.UnitPrice,
                        TotalPrice = itemDTO.UnitPrice * itemDTO.Quantity,
                        DiscountAmount = itemDTO.DiscountAmount
                    };

                    _context.SaleItems.Add(saleItem);

                    // Update stock quantity
                    stock.CurrentQuantity -= itemDTO.Quantity;
                    stock.LastUpdatedAt = DateTime.UtcNow;

                    subTotal += saleItem.TotalPrice;
                }

                // Update sale totals
                sale.SubTotal = subTotal;
                sale.TaxAmount = subTotal * 0.1m; // 10% tax rate - you might want to make this configurable
                sale.TotalAmount = subTotal + sale.TaxAmount;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetSale), new { id = sale.SaleId }, sale);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PATCH: api/Sales/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateSaleStatusDTO updateDTO)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            sale.Status = updateDTO.Status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Sales/5/payment
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> AddPayment(int id, UpdateSalePaymentDTO paymentDTO)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            sale.PaidAmount += paymentDTO.Amount;
            sale.PaymentMethod = paymentDTO.PaymentMethod;

            // Update payment status
            if (sale.PaidAmount >= sale.TotalAmount)
            {
                sale.PaymentStatus = "Paid";
            }
            else if (sale.PaidAmount > 0)
            {
                sale.PaymentStatus = "Partial";
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
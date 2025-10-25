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
    public class PurchasesController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public PurchasesController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: api/Purchases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Purchase>>> GetPurchases()
        {
            return await _context.Purchases
                .Include(p => p.Client)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        // GET: api/Purchases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Purchase>> GetPurchase(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Client)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Stock)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return purchase;
        }

        // GET: api/Purchases/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Purchase>>> GetPendingPurchases()
        {
            return await _context.Purchases
                .Include(p => p.Client)
                .Where(p => p.Status == "Pending")
                .OrderBy(p => p.PurchaseDate)
                .ToListAsync();
        }

        // POST: api/Purchases
        [HttpPost]
        public async Task<ActionResult<Purchase>> CreatePurchase(CreatePurchaseDTO purchaseDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate client
                var client = await _context.Clients.FindAsync(purchaseDTO.ClientId);
                if (client == null)
                {
                    return BadRequest("Client not found");
                }

                // Generate purchase number
                var lastPurchaseNumber = await _context.Purchases
                    .OrderByDescending(p => p.PurchaseNumber)
                    .Select(p => p.PurchaseNumber)
                    .FirstOrDefaultAsync() ?? "PO-0000";

                var nextNumber = int.Parse(lastPurchaseNumber.Split('-')[1]) + 1;
                var newPurchaseNumber = $"PO-{nextNumber:D4}";

                // Create purchase
                var purchase = new Purchase
                {
                    ClientId = purchaseDTO.ClientId,
                    UserId = 1, // TODO: Get from authentication
                    PurchaseNumber = newPurchaseNumber,
                    PurchaseDate = DateTime.UtcNow,
                    Status = "Pending",
                    PaymentStatus = "Pending",
                    PaymentDueDate = purchaseDTO.PaymentDueDate,
                    Notes = purchaseDTO.Notes
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                // Add purchase items
                foreach (var itemDTO in purchaseDTO.Items)
                {
                    var stock = await _context.Stock.FindAsync(itemDTO.StockId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException($"Stock item {itemDTO.StockId} not found");
                    }

                    var purchaseItem = new PurchaseItem
                    {
                        PurchaseId = purchase.PurchaseId,
                        StockId = itemDTO.StockId,
                        Quantity = itemDTO.Quantity,
                        UnitPrice = itemDTO.UnitPrice,
                        TotalPrice = itemDTO.Quantity * itemDTO.UnitPrice
                    };

                    _context.PurchaseItems.Add(purchaseItem);
                    totalAmount += purchaseItem.TotalPrice;
                }

                purchase.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetPurchase), new { id = purchase.PurchaseId }, purchase);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PATCH: api/Purchases/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdatePurchaseStatusDTO updateDTO)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Stock)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (updateDTO.Status == "Received" && purchase.Status != "Received")
                {
                    // Update stock quantities when purchase is received
                    foreach (var item in purchase.PurchaseItems)
                    {
                        item.Stock.CurrentQuantity += item.Quantity;
                        item.Stock.LastUpdatedAt = DateTime.UtcNow;
                    }
                }

                purchase.Status = updateDTO.Status;
                if (!string.IsNullOrEmpty(updateDTO.Notes))
                {
                    purchase.Notes = purchase.Notes + "\n" + updateDTO.Notes;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // POST: api/Purchases/5/payment
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> AddPayment(int id, UpdatePurchasePaymentDTO paymentDTO)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                purchase.PaidAmount += paymentDTO.Amount;
                purchase.PaymentStatus = paymentDTO.PaymentStatus;

                if (purchase.PaidAmount >= purchase.TotalAmount)
                {
                    purchase.PaymentStatus = "Paid";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
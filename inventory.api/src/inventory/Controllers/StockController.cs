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
    public class StockController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public StockController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: api/Stock
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStock()
        {
            return await _context.Stock
                .Where(s => s.IsActive)
                .OrderBy(s => s.ItemName)
                .ToListAsync();
        }

        // GET: api/Stock/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStock(int id)
        {
            var stock = await _context.Stock.FindAsync(id);

            if (stock == null || !stock.IsActive)
            {
                return NotFound();
            }

            return stock;
        }

        // GET: api/Stock/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Stock>>> GetLowStock()
        {
            return await _context.Stock
                .Where(s => s.IsActive && s.CurrentQuantity <= s.ReorderPoint)
                .OrderBy(s => s.CurrentQuantity)
                .ToListAsync();
        }

        // POST: api/Stock
        [HttpPost]
        public async Task<ActionResult<Stock>> CreateStock(CreateStockDTO stockDTO)
        {
            // Check if SKU already exists
            if (await _context.Stock.AnyAsync(s => s.SKU == stockDTO.SKU))
            {
                return BadRequest("SKU already exists");
            }

            var stock = new Stock
            {
                ItemName = stockDTO.ItemName,
                SKU = stockDTO.SKU,
                Category = stockDTO.Category,
                Description = stockDTO.Description,
                UnitPrice = stockDTO.UnitPrice,
                WholesalePrice = stockDTO.WholesalePrice,
                CurrentQuantity = stockDTO.CurrentQuantity,
                MinimumQuantity = stockDTO.MinimumQuantity,
                ReorderPoint = stockDTO.ReorderPoint,
                Location = stockDTO.Location,
                Brand = stockDTO.Brand,
                Model = stockDTO.Model,
                Size = stockDTO.Size,
                Color = stockDTO.Color,
                UnitOfMeasure = stockDTO.UnitOfMeasure,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Stock.Add(stock);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStock), new { id = stock.StockId }, stock);
        }

        // PUT: api/Stock/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, UpdateStockDTO stockDTO)
        {
            if (id != stockDTO.StockId)
            {
                return BadRequest();
            }

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null || !stock.IsActive)
            {
                return NotFound();
            }

            // Check if new SKU conflicts with existing ones
            if (await _context.Stock.AnyAsync(s => s.SKU == stockDTO.SKU && s.StockId != id))
            {
                return BadRequest("SKU already exists");
            }

            stock.ItemName = stockDTO.ItemName;
            stock.SKU = stockDTO.SKU;
            stock.Category = stockDTO.Category;
            stock.Description = stockDTO.Description;
            stock.UnitPrice = stockDTO.UnitPrice;
            stock.WholesalePrice = stockDTO.WholesalePrice;
            stock.CurrentQuantity = stockDTO.CurrentQuantity;
            stock.MinimumQuantity = stockDTO.MinimumQuantity;
            stock.ReorderPoint = stockDTO.ReorderPoint;
            stock.Location = stockDTO.Location;
            stock.Brand = stockDTO.Brand;
            stock.Model = stockDTO.Model;
            stock.Size = stockDTO.Size;
            stock.Color = stockDTO.Color;
            stock.UnitOfMeasure = stockDTO.UnitOfMeasure;
            stock.LastUpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // PATCH: api/Stock/5/quantity
        [HttpPatch("{id}/quantity")]
        public async Task<IActionResult> UpdateQuantity(int id, StockQuantityUpdateDTO updateDTO)
        {
            if (id != updateDTO.StockId)
            {
                return BadRequest();
            }

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null || !stock.IsActive)
            {
                return NotFound();
            }

            var newQuantity = stock.CurrentQuantity + updateDTO.QuantityChange;
            if (newQuantity < 0)
            {
                return BadRequest("Insufficient stock quantity");
            }

            stock.CurrentQuantity = newQuantity;
            stock.LastUpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Stock/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            var stock = await _context.Stock.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            // Soft delete
            stock.IsActive = false;
            stock.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockExists(int id)
        {
            return _context.Stock.Any(e => e.StockId == id);
        }
    }
}
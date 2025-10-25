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
    public class CustomersController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public CustomersController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Sales)
                .FirstOrDefaultAsync(c => c.CustomerId == id && c.IsActive);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // GET: api/Customers/type/wholesale
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersByType(string type)
        {
            return await _context.Customers
                .Where(c => c.IsActive && c.CustomerType.ToLower() == type.ToLower())
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // POST: api/Customers
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer(CreateCustomerDTO customerDTO)
        {
            if (await _context.Customers.AnyAsync(c => c.Email == customerDTO.Email && !string.IsNullOrEmpty(customerDTO.Email)))
            {
                return BadRequest("Email already registered");
            }

            var customer = new Customer
            {
                Name = customerDTO.Name,
                CustomerType = customerDTO.CustomerType,
                Address = customerDTO.Address,
                Phone = customerDTO.Phone,
                Email = customerDTO.Email,
                CreditLimit = customerDTO.CreditLimit,
                CurrentCredit = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }

        // PUT: api/Customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDTO customerDTO)
        {
            if (id != customerDTO.CustomerId)
            {
                return BadRequest();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null || !customer.IsActive)
            {
                return NotFound();
            }

            // Check email uniqueness if email is being changed
            if (!string.IsNullOrEmpty(customerDTO.Email) && 
                customerDTO.Email != customer.Email && 
                await _context.Customers.AnyAsync(c => c.Email == customerDTO.Email))
            {
                return BadRequest("Email already registered");
            }

            customer.Name = customerDTO.Name;
            customer.CustomerType = customerDTO.CustomerType;
            customer.Address = customerDTO.Address;
            customer.Phone = customerDTO.Phone;
            customer.Email = customerDTO.Email;
            customer.CreditLimit = customerDTO.CreditLimit;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // PATCH: api/Customers/5/credit
        [HttpPatch("{id}/credit")]
        public async Task<IActionResult> UpdateCreditLimit(int id, UpdateCustomerCreditDTO updateDTO)
        {
            if (id != updateDTO.CustomerId)
            {
                return BadRequest();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null || !customer.IsActive)
            {
                return NotFound();
            }

            // Don't allow credit limit below current credit usage
            if (updateDTO.NewCreditLimit < customer.CurrentCredit)
            {
                return BadRequest("New credit limit cannot be less than current credit usage");
            }

            customer.CreditLimit = updateDTO.NewCreditLimit;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Check if customer has any active sales
            var hasActiveSales = await _context.Sales
                .AnyAsync(s => s.CustomerId == id && 
                             (s.Status == "Pending" || s.PaymentStatus != "Paid"));

            if (hasActiveSales)
            {
                return BadRequest("Cannot delete customer with pending or unpaid sales");
            }

            // Soft delete
            customer.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
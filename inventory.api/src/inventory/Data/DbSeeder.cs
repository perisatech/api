using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using inventory.Models;

namespace inventory.Data
{
    public static class DbSeeder
    {
        public static async Task SeedData(InventoryDbContext context)
        {
            // Create admin user if none exists
            if (!await context.Users.AnyAsync())
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@inventory.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FirstName = "Admin",
                    LastName = "User",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
            
            // Seed sample stock items
            if (!await context.Stock.AnyAsync())
            {
                var items = new[]
                {
                    new Models.Stock
                    {
                        ItemName = "Standard Widget",
                        SKU = "WIDGET-STD-001",
                        Category = "Widgets",
                        Description = "A standard widget for general purpose",
                        UnitPrice = 12.50m,
                        WholesalePrice = 10.00m,
                        CurrentQuantity = 150,
                        MinimumQuantity = 10,
                        ReorderPoint = 20,
                        Location = "A1"
                    },
                    new Models.Stock
                    {
                        ItemName = "Premium Gizmo",
                        SKU = "GIZMO-PRM-001",
                        Category = "Gizmos",
                        Description = "A premium gizmo for retail customers",
                        UnitPrice = 49.99m,
                        WholesalePrice = 40.00m,
                        CurrentQuantity = 50,
                        MinimumQuantity = 5,
                        ReorderPoint = 10,
                        Location = "B2"
                    }
                };

                context.Stock.AddRange(items);
                await context.SaveChangesAsync();
            }

            // Seed sample customers (retail and wholesale)
            if (!await context.Customers.AnyAsync())
            {
                var customers = new[]
                {
                    new Customer
                    {
                        Name = "Retail Customer",
                        CustomerType = "Retail",
                        Phone = "+10000000001",
                        Email = "retail@example.local",
                        CreditLimit = 0m,
                        CurrentCredit = 0m,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Customer
                    {
                        Name = "Wholesale Buyer",
                        CustomerType = "Wholesale",
                        Phone = "+10000000002",
                        Email = "wholesale@example.local",
                        CreditLimit = 10000m,
                        CurrentCredit = 0m,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Customers.AddRange(customers);
                await context.SaveChangesAsync();
            }

            // Seed a sample client/supplier
            if (!await context.Clients.AnyAsync())
            {
                var client = new Client
                {
                    Name = "Default Supplier",
                    ClientType = "Supplier",
                    Phone = "+10000000003",
                    Email = "supplier@example.local",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Clients.Add(client);
                await context.SaveChangesAsync();
            }
        }
    }
}
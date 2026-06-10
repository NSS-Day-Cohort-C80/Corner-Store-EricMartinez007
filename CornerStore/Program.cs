using CornerStore.Models;
using CornerStore.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// cashiers endpoints
app.MapGet("/api/cashiers/{id}", (CornerStoreDbContext db, int id) =>
{
    CashierDTO cashier = db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
        .Where(c => c.Id == id)
        .Select(c => new CashierDTO
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            FullName = c.FirstName + " " + c.LastName,
            Orders = c.Orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                CashierId = o.CashierId,
                PaidOnDate = o.PaidOnDate,
                Total = o.OrderProducts.Sum(op => op.Product.Price * op.Quantity),
                OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    Quantity = op.Quantity,
                    Product = new ProductDTO
                    {
                        Id = op.Product.Id,
                        ProductName = op.Product.ProductName,
                        Price = op.Product.Price,
                        Brand = op.Product.Brand,
                        CategoryId = op.Product.CategoryId
                    }
                }).ToList()
            }).ToList()
        })
        .SingleOrDefault();

        if (cashier == null)
        {
           return Results.NotFound(); 
        }
        return Results.Ok(cashier);
});

app.MapPost("/api/cashiers", (CornerStoreDbContext db, CreateCashierDTO newCashierDTO) =>
{
    Cashier cashier = new Cashier
    {
        FirstName = newCashierDTO.FirstName,
        LastName = newCashierDTO.LastName, 
    };

    db.Cashiers.Add(cashier);
    db.SaveChanges();

    return Results.Created($"/api/cashiers/{cashier.Id}", new CashierDTO
    {
        Id = cashier.Id,
        FirstName = cashier.FirstName,
        LastName = cashier.LastName,
        FullName = cashier.FirstName + " " + cashier.LastName
    });
});

// products endpoints
// this one was a little tricky at first to figure out. I was doing exact matches at first and only searching cor categoryId not name
app.MapGet("/api/products", (CornerStoreDbContext db, string? search) =>
{
    return db.Products
        .Include(p => p.Category)
        .Where(p => search == null ||
            p.ProductName.ToLower().Contains(search.ToLower()) ||
            p.Category.CategoryName.ToLower().Contains(search.ToLower()))
        .Select(p => new ProductDTO
        {
            Id = p.Id,
            ProductName = p.ProductName,
            Price = p.Price,
            Brand = p.Brand,
            CategoryId = p.CategoryId,
            Category = new CategoryDTO
            {
                Id = p.Category.Id,
                CategoryName = p.Category.CategoryName
            }
        });
});

app.MapPost("/api/products", (CornerStoreDbContext db, CreateProductDTO newProductDTO) =>
{
    Product product = new Product
    {
        ProductName = newProductDTO.ProductName,
        Price = newProductDTO.Price,
        Brand = newProductDTO.Brand,
        CategoryId = newProductDTO.CategoryId
    };

    db.Products.Add(product);
    db.SaveChanges();

    return Results.Created($"/api/products/{product.Id}", new ProductDTO
    {
        Id = product.Id,
        ProductName = newProductDTO.ProductName,
        Price = newProductDTO.Price,
        Brand = newProductDTO.Brand,
        CategoryId = newProductDTO.CategoryId
    });
});

app.MapPut("/api/products/{id}/edit", (CornerStoreDbContext db, int id, ProductDTO updateDTO) =>
{
    Product product = db.Products.SingleOrDefault(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound();
    }

    product.ProductName = updateDTO.ProductName;
    product.Price = updateDTO.Price;
    product.Brand = updateDTO.Brand;
    product.CategoryId = updateDTO.CategoryId;

    db.SaveChanges();

    return Results.NoContent();
});

// orders endpoints
app.MapGet("/api/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    OrderDTO order = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
        .Where(o => o.Id == id)
        .Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            Cashier = new CashierDTO
            {
                Id = o.Cashier.Id,
                FirstName = o.Cashier.FirstName,
                LastName = o.Cashier.LastName,
                FullName = o.Cashier.FirstName + " " + o.Cashier.LastName,
            },
            PaidOnDate = o.PaidOnDate,
            Total = o.OrderProducts.Sum(op => op.Product.Price * op.Quantity),
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
            {
                OrderId = op.OrderId,
                ProductId = op.ProductId,
                Product = new ProductDTO
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = op.Product.Category.Id,
                        CategoryName = op.Product.Category.CategoryName
                    }
                }
            }).ToList()
        })
        .SingleOrDefault();

        if (order == null)
        {
           return Results.NotFound(); 
        }
        return Results.Ok(order);
});

// tricky with date
app.MapGet("/api/orders", (CornerStoreDbContext db, string? orderDate) =>
{
    DateTime? parsed = orderDate != null ? DateTime.Parse(orderDate) : null;

    return db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
        .Where(o => parsed == null || o.PaidOnDate.Value.Date == parsed.Value.Date)
        .Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            Cashier = new CashierDTO
            {
                Id = o.Cashier.Id,
                FirstName = o.Cashier.FirstName,
                LastName = o.Cashier.LastName,
                FullName = o.Cashier.FirstName + " " + o.Cashier.LastName,
            },
            PaidOnDate = o.PaidOnDate,
            Total = o.OrderProducts.Sum(op => op.Product.Price * op.Quantity),
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
            {
                OrderId = op.OrderId,
                ProductId = op.ProductId,
                Product = new ProductDTO
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = op.Product.Category.Id,
                        CategoryName = op.Product.Category.CategoryName
                    }
                }
            }).ToList()
        });
});

app.MapDelete("/api/orders/{id}/delete", (CornerStoreDbContext db, int id) => 
{
    Order order = db.Orders.SingleOrDefault(p => p.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    db.Orders.Remove(order);
    db.SaveChanges();
    return Results.NoContent();
});

// tricky with posting order and order products 
app.MapPost("/api/orders", (CornerStoreDbContext db, CreateOrderDTO newOrderDTO) =>
{
    Order order = new Order
    {
        CashierId = newOrderDTO.CashierId,
        PaidOnDate = DateTime.Today,
        OrderProducts = newOrderDTO.ProductIds.Select(id => new OrderProduct
        {
            ProductId = id,
            Quantity = 1
        }).ToList()
    };

    db.Orders.Add(order);
    db.SaveChanges();

    order = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
        .First(o => o.Id == order.Id);

    return Results.Created($"/api/orders/{order.Id}", new OrderDTO
    {
        Id = order.Id,
        CashierId = order.CashierId,
        Cashier = new CashierDTO
        {
            Id = order.Cashier.Id,
            FirstName = order.Cashier.FirstName,
            LastName = order.Cashier.LastName,
            FullName = order.Cashier.FirstName + " " + order.Cashier.LastName
        },
        PaidOnDate = order.PaidOnDate,
        Total = order.OrderProducts.Sum(op => op.Product.Price * op.Quantity),
        OrderProducts = order.OrderProducts.Select(op => new OrderProductDTO
        {
            OrderId = op.OrderId,
            ProductId = op.ProductId,
            Quantity = op.Quantity,
            Product = new ProductDTO
            {
                Id = op.Product.Id,
                ProductName = op.Product.ProductName,
                Price = op.Product.Price,
                Brand = op.Product.Brand,
                CategoryId = op.Product.CategoryId,
                Category = new CategoryDTO
                {
                    Id = op.Product.Category.Id,
                    CategoryName = op.Product.Category.CategoryName
                }
            }
        }).ToList()
    });
});

app.Run();

//don't move or change this!
public partial class Program { }
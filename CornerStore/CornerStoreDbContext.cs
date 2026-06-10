using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers {get; set;}
    public DbSet<Category> Categories {get; set;}
    public DbSet<Order> Orders {get; set;}
    public DbSet<OrderProduct> OrderProducts {get; set;}
    public DbSet<Product> Products {get; set;}

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //OrderProduct has no Id property, so EF needs to be told its primary key
        //is the combination of OrderId and ProductId (a composite key)
        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<Cashier>().HasData(
            new Cashier { Id = 1, FirstName = "Monkey D.", LastName = "Luffy" },
            new Cashier { Id = 2, FirstName = "Roronoa", LastName = "Zoro" },
            new Cashier { Id = 3, FirstName = "Vinsmoke", LastName = "Sanji" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "Devil Fruits" },
            new Category { Id = 2, CategoryName = "Beverages" },
            new Category { Id = 3, CategoryName = "Provisions" },
            new Category { Id = 4, CategoryName = "Sweets" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Gum-Gum Fruit", Price = 99.99M, Brand = "Grand Line Imports", CategoryId = 1 },
            new Product { Id = 2, ProductName = "Flame-Flame Fruit", Price = 149.99M, Brand = "Grand Line Imports", CategoryId = 1 },
            new Product { Id = 3, ProductName = "Cola", Price = 1.99M, Brand = "Franky Cola Co.", CategoryId = 2 },
            new Product { Id = 4, ProductName = "Binks' Sake", Price = 12.49M, Brand = "Laboon Brewery", CategoryId = 2 },
            new Product { Id = 5, ProductName = "Sea King Jerky", Price = 5.99M, Brand = "Baratie Provisions", CategoryId = 3 },
            new Product { Id = 6, ProductName = "Wedding Cake Slice", Price = 4.79M, Brand = "Whole Cake Bakery", CategoryId = 4 }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, CashierId = 1, PaidOnDate = new DateTime(2026, 6, 1, 9, 15, 0) },
            new Order { Id = 2, CashierId = 2, PaidOnDate = new DateTime(2026, 6, 3, 14, 30, 0) },
            new Order { Id = 3, CashierId = 3, PaidOnDate = new DateTime(2026, 6, 5, 11, 45, 0) },
            new Order { Id = 4, CashierId = 1, PaidOnDate = new DateTime(2026, 6, 8, 16, 20, 0) },
            new Order { Id = 5, CashierId = 2, PaidOnDate = null } //an open (unpaid) order
        );

        modelBuilder.Entity<OrderProduct>().HasData(
            new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 2 },
            new OrderProduct { OrderId = 1, ProductId = 3, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 5, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 6, Quantity = 3 },
            new OrderProduct { OrderId = 3, ProductId = 4, Quantity = 6 },
            new OrderProduct { OrderId = 4, ProductId = 2, Quantity = 1 },
            new OrderProduct { OrderId = 4, ProductId = 3, Quantity = 2 },
            new OrderProduct { OrderId = 4, ProductId = 6, Quantity = 1 },
            new OrderProduct { OrderId = 5, ProductId = 1, Quantity = 1 }
        );
    }
}
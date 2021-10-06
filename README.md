# GenericRepoSpecPattern
This package enables you to use a generic repository specification pattern for coomunicating to your database.

# Setup

### Create a GenericRepository class
```csharp
using Microsoft.EntityFrameworkCore;
using PTJK.GenericRepoSpecPattern.Data;
using PTJK.GenericRepoSpecPattern.Entities;
using PTJK.GenericRepoSpecPattern.Interfaces;
using PTJK.GenericRepoSpecPattern.Specifications;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly YourDBContext _context;

    public GenericRepository(YourDBContext context)
    {
        _context = context;
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> GetEntityWithSpec(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }
```

### Create a specification

```csharp
    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
    }
}
```

### Add Scoped Service to startup.cs
```csharp
using PTJK.GenericRepoSpecPattern.Interfaces;

public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
}
```


# Usage

### Example Entity
```csharp
public class Product : GenericRepoSpecPattern.Entities.BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string PictureUrl { get; set; }
    public ProductType ProductType { get; set; }
    public int ProductTypeId { get; set; }
    public ProductBrand ProductBrand { get; set; }
    public int ProductBrandId { get; set; }
}

public class ProductBrand : GenericRepoSpecPattern.Entities.BaseEntity
{
    public string Name { get; set; }
}

public class ProductType : GenericRepoSpecPattern.Entities.BaseEntity
{
    public string Name { get; set; }
}
```

### Create a specification
```csharp
using Skinet.GenericRepoSpecPattern.Specifications;

public class ProductsWithTypesAndBrandsSpecification : BaseSpecification<Product>
{
    public ProductsWithTypesAndBrandsSpecification()
    {
        AddInclude(x => x.ProductType);
        AddInclude(x => x.ProductBrand);
    }

    public ProductsWithTypesAndBrandsSpecification(int id) : base(x => x.Id == id)
    {
        AddInclude(x => x.ProductType);
        AddInclude(x => x.ProductBrand);
    }
}
```

### Example usage in API Controller
```csharp

using PTJK.GenericRepoSpecPattern.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly IGenericRepository<Product> _productsRepo;
    private readonly IGenericRepository<ProductBrand> _productBrandRepo;
    private readonly IGenericRepository<ProductType> _productTypeRepo;

    public ProductsController(IGenericRepository<Product> productsRepo,
        IGenericRepository<ProductBrand> productBrandRepo,
        IGenericRepository<ProductType> productTypeRepo)
    {
        _productsRepo = productsRepo;
        _productBrandRepo = productBrandRepo;
        _productTypeRepo = productTypeRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        var spec = new ProductsWithTypesAndBrandsSpecification();
        var products = await _productsRepo.ListAsync(spec);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var spec = new ProductsWithTypesAndBrandsSpecification(id);
        return await _productsRepo.GetEntityWithSpec(spec);
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
    {
        return Ok(await _productBrandRepo.ListAllAsync());
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
    {
        return Ok(await _productTypeRepo.ListAllAsync());
    }
}
```

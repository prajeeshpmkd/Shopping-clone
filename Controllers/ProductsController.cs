using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.WebAPI.Data;
using ShoppingApp.WebAPI.Data.Repositories;
using ShoppingApp.WebAPI.Entities.Models;
using ShoppingApp.WebAPI.Entities.Resources;

namespace ShoppingApp.WebAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IApplicationRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ProductsController(IApplicationRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await repository.GetProducts();

            var result = mapper.Map<IEnumerable<ProductResource>>(products);

            return Ok(result);
        }

        [HttpGet("{id}", Name = nameof(GetProduct))]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await repository.GetProduct(id);

            if (product == null)
            {
                return NotFound();
            }

            var result = mapper.Map<ProductResource>(product);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SaveProductResource payload)
        {
            var category = await repository.GetCategory(payload.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid CategoryId");
                return BadRequest(ModelState);
            }

            var product = mapper.Map<Product>(payload);

            repository.AddProduct(product);
            await unitOfWork.CompleteAsync();

            var result = mapper.Map<ProductResource>(product);

            return CreatedAtRoute(nameof(GetProduct), new { id = product.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] SaveProductResource payload)
        {
            var category = await repository.GetCategory(payload.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid CategoryId");
                return BadRequest(ModelState);
            }

            var product = await repository.GetProduct(id);

            if (product == null)
            {
                return NotFound();
            }

            mapper.Map<SaveProductResource, Product>(payload, product);

            await unitOfWork.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await repository.GetProduct(id);

            if (product == null)
            {
                return NotFound();
            }

            repository.RemoveProduct(product);
            await unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}
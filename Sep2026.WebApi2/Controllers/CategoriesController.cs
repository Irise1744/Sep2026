using Dapper;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Sep2026.WebApi2.Models;

namespace Sep2026.WebApi2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IDbConnection _db;

        public CategoriesController(IDbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            const string sql = @"
                SELECT CategoryId, CategoryName, Description, CreatedAtUtc
                FROM dbo.Categories
                WHERE DeleteFlag = 0;";

            var categories =
                await _db.QueryAsync<CategoryResponseModel>(sql);

            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            const string sql = @"
                SELECT CategoryId, CategoryName, Description, CreatedAtUtc
                FROM dbo.Categories
                WHERE CategoryId = @Id AND DeleteFlag = 0;";

            var category =
                await _db.QuerySingleOrDefaultAsync<CategoryResponseModel>(
                    sql, new { Id = id });

            if (category is null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            CategoriesCreateRequestModel request)
        {
            var error = Validate(request.CategoryName, request.Description);
            if (error is not null)
                return BadRequest(error);

            const string duplicateSql = @"
                SELECT COUNT(1)
                FROM dbo.Categories
                WHERE CategoryName = @CategoryName AND DeleteFlag = 0;";

            var exists = await _db.ExecuteScalarAsync<int>(
                duplicateSql, new { request.CategoryName });

            if (exists > 0)
                return Conflict("Category name already exists.");

            const string insertSql = @"
                INSERT INTO dbo.Categories (CategoryName, Description)
                OUTPUT INSERTED.CategoryId
                VALUES (@CategoryName, @Description);";

            var id = await _db.ExecuteScalarAsync<int>(insertSql, request);

            var createdCategory = await GetCategoryData(id);

            return CreatedAtAction(
                nameof(GetCategory),
                new { id },
                createdCategory);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
            int id, CategoriesUpsertRequestModel request)
        {
            var error = Validate(request.CategoryName, request.Description);
            if (error is not null)
                return BadRequest(error);

            const string duplicateSql = @"
                SELECT COUNT(1)
                FROM dbo.Categories
                WHERE CategoryName = @CategoryName
                  AND CategoryId <> @Id
                  AND DeleteFlag = 0;";

            var exists = await _db.ExecuteScalarAsync<int>(
                duplicateSql, new { request.CategoryName, Id = id });

            if (exists > 0)
                return Conflict("Category name already exists.");

            const string updateSql = @"
                UPDATE dbo.Categories
                SET CategoryName = @CategoryName,
                    Description = @Description
                WHERE CategoryId = @Id AND DeleteFlag = 0;";

            var rowsUpdated = await _db.ExecuteAsync(
                updateSql,
                new { request.CategoryName, request.Description, Id = id });

            if (rowsUpdated == 0)
                return NotFound();

            var updatedCategory = await GetCategoryData(id);
            return Ok(updatedCategory);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            const string sql = @"
                UPDATE dbo.Categories
                SET DeleteFlag = 1
                WHERE CategoryId = @Id AND DeleteFlag = 0;";

            var rowsUpdated =
                await _db.ExecuteAsync(sql, new { Id = id });

            if (rowsUpdated == 0)
                return NotFound();

            return NoContent();
        }

        private async Task<CategoryResponseModel?> GetCategoryData(int id)
        {
            const string sql = @"
                SELECT CategoryId, CategoryName, Description, CreatedAtUtc
                FROM dbo.Categories
                WHERE CategoryId = @Id AND DeleteFlag = 0;";

            return await _db.QuerySingleOrDefaultAsync<CategoryResponseModel>(
                sql, new { Id = id });
        }

        private static string? Validate(
            string categoryName, string? description)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return "Category name is required.";

            if (categoryName.Length > 100)
                return "Category name must be 100 characters or fewer.";

            if (description?.Length > 500)
                return "Description must be 500 characters or fewer.";

            return null;
        }
    }

    public class CategoryResponseModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
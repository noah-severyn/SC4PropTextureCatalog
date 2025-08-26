using Microsoft.AspNetCore.Mvc;
using SC4PropTextureCatalogAPI.Models;

namespace SC4PropTextureCatalogAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController(IItemRepository repo) : ControllerBase {
        private readonly IItemRepository _repo = repo;

        [HttpGet("{assetId:int}")]
        public async Task<IActionResult> Get(int assetId) {
            var item = await _repo.GetByIdAsync(assetId);
            return item is not null ? Ok(item) : NotFound();
        }

        [HttpGet("{searchText}")]
        public async Task<ActionResult<IEnumerable<TGI>>> Get(string searchText) {
            var item = await _repo.GetSearchResultsAsync(searchText);
            return item is not null ? Ok(item) : NotFound();
        }
    }
}

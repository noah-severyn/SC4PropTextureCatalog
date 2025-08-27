using Microsoft.AspNetCore.Mvc;
using SC4PropTextureCatalogAPI.Models;

namespace SC4PropTextureCatalogAPI.Controllers {
    [ApiController]
    [Route("api")]
    public class QueryController(IItemRepository repo) : ControllerBase {
        private readonly IItemRepository _repo = repo;

        /// <summary>
        /// Return a TGI item by it's unique identifier
        /// </summary>
        /// <param name="compositeId">The composite id is a composite key composed of the exchange id and the unique upload id on the exchange, separated by a dash</param>
        /// <returns></returns>
        [HttpGet("item/{compositeId}")]
        public async Task<IActionResult> GetItem(int compositeId) {
            var item = await _repo.GetByIdAsync(compositeId);
            return item is not null ? Ok(item) : NotFound();
        }

        [HttpGet("search/{searchText}")]
        public async Task<ActionResult<IEnumerable<CatalogItems>>> GetSearchResults(string searchText) {
            var item = await _repo.GetSearchResultsAsync(searchText);
            return item is not null ? Ok(item) : NotFound();
        }

        [HttpGet("id/{instanceId}")]
        public async Task<ActionResult<IEnumerable<CatalogItems>>> GetInstanceId(string instanceId) {
            var item = await _repo.GetByInstanceAsync(instanceId);
            return item is not null ? Ok(item) : NotFound();
        }
    }
}

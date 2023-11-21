using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item,Item>();
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full,searchParams.SearchTerm).SortByTextScore();
        }
        query = searchParams.OrderBy switch 
        {
            "make" => query.Sort(x => x.Ascending(o => o.Make)),
            "new" => query.Sort(x => x.Descending(o => o.CreateAt)),
            _ => query.Sort(x => x.Ascending(o => o.AuctionEnd))
        };
        query = searchParams.FilterBy switch 
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) 
                && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };
        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }
        query.PageNumber(searchParams.PageNumber == 0 ? 1 : searchParams.PageNumber);
        query.PageSize(searchParams.PageSize == 0 ? 4 : searchParams.PageSize);
        var result = await query.ExecuteAsync();

        return Ok(new {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}

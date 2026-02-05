using System;
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
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var queryable = DB.Default.PagedSearch<Item, Item>();
        queryable.Sort(x => x.Ascending(i => i.Make));

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            queryable.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        queryable = searchParams.OrderBy switch
        {
            "make" => queryable.Sort(x => x.Ascending(i => i.Make)).Sort(x => x.Ascending(i => i.Model)),
            "new" => queryable.Sort(x => x.Descending(i => i.CreatedAt)),
            _ => queryable.Sort(x => x.Ascending(i => i.AuctionEnd))
        };

        queryable = searchParams.FilterBy switch
        {
            "finished" => queryable.Match(i => i.AuctionEnd <= DateTime.UtcNow),
            "endingSoon" => queryable.Match(i => i.AuctionEnd <= DateTime.UtcNow.AddHours(6) && i.AuctionEnd > DateTime.UtcNow),
            _ => queryable.Match(i => i.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            queryable = queryable.Match(i => i.Seller.ToLower() == searchParams.Seller.ToLower());
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            queryable = queryable.Match(i => i.Winner.ToLower() == searchParams.Winner.ToLower());
        }

        queryable.PageNumber(searchParams.PageNumber);
        queryable.PageSize(searchParams.PageSize);

        var result = await queryable.ExecuteAsync();

        return Ok(new
        {
            Items = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}

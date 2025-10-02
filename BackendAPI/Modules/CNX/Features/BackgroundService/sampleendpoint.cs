using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CNX.Features.BackgroundService;

public class sampleendpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("sample", async () =>
        {
            return Results.Ok(new { sample =  "new" });
        });
    }
}

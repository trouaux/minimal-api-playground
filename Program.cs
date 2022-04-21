using All;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    // reporting api versions will return the headers
    // "api-supported-versions" and "api-deprecated-versions"
    options.ReportApiVersions = true;
    options.Policies.Sunset(0.9)
                    .Effective(DateTimeOffset.Now.AddDays(60))
                    .Link("policy.html")
                    .Title("Versioning Policy")
                    .Type("text/html");
})
.AddApiExplorer(options =>
{
            // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            options.GroupNameFormat = "'v'VVV";
            // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
            // can also be used to control the format of the API version in route templates
            options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());

//ConfigureServices

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var descriptions = app.DescribeApiVersions();
    // build a swagger endpoint for each discovered API version
    foreach (var description in descriptions)
    {
        var url = $"/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        options.SwaggerEndpoint(url, name);
    }
});

//Configure

app.DefineApi("Test")
            .HasApiVersion(2.0)
            .HasApiVersion(1.0)
            .HasMapping(api =>
            {
                api.MapPost("/api/v{version:apiVersion}/post", Actions.Post)
                    .MapToApiVersion(1.0)
                    .MapToApiVersion(2.0);
                api.MapDelete("/api/v{version:apiVersion}/delete", Actions.Delete)
                    .MapToApiVersion(1.0)
                    .MapToApiVersion(2.0);
            });

app.Run();

public class Actions
{
    internal static async Task<IResult> Post()
    {
        return Results.Ok();
    }


    internal static async Task<IResult> Delete()
    {
        return Results.Ok();
    }
}

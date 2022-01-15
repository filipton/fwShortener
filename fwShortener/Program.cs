using System.Text.RegularExpressions;
using fwShortener;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

public class Program
{
    //filip:AdminPswd123
    //ZmlsaXA6QWRtaW5Qc3dkMTIz

    public static string LoginToken = "ZmlsaXA6QWRtaW5Qc3dkMTIz";
    
    public static void Main(string[] args)
    {
        SQLConnection.Init();
        if (!Directory.Exists("images"))
        {
            Directory.CreateDirectory("images");
        }

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            //app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
        app.Map("/Error", HandleErrorPage);

        app.Map("/create", HandleCreatePage);
        app.Map("/upload", HandleUploadImage);
        app.Map("/admin", HandleAdminPage);
        app.Map("/api", HandleApi);

        app.Map("/s", HandleShortLinks);
        app.Map("/i", HandleImages);
        
        app.MapGet("/list-r", SQLConnection.GetAllReports);

        app.MapGet("/report", async ctx =>
        {
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync(
                File.ReadAllText("wwwroot/report.html")
            );
        });

        app.MapPost("/report", async ctx =>
        {
            //.Replace("{ResponseMessage}", "")
            string output = File.ReadAllText("wwwroot/report-complete.html");

            string aurl = ctx.Request.Form["aurl"];
            string amsg = ctx.Request.Form["amsg"];

            if (Regex.IsMatch(aurl, "((http(s?))\\://)((i.)?)filipton.tech/"))
            {
                SQLConnection.AddReport(aurl, amsg);
                output = output.Replace("{ResponseMessage}", "Thank you for reporting! We'll check it shortly!");
            }
            else
            {
                output = output.Replace("{ResponseMessage}", "You need to pass correct url!");
            }

            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync(output);
        });

        var options = new DefaultFilesOptions();
        options.DefaultFileNames.Clear();
        options.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(options);

        app.UseStaticFiles();

        app.Run();
    }
    
    static void HandleErrorPage(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            string scString = context.Request.Query["statusCode"];
            int statusCode = int.Parse(string.IsNullOrEmpty(scString) ? "500" : scString);

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync((await File.ReadAllTextAsync("wwwroot/error.html")).
                Replace("{statusCode}", statusCode.ToString()).
                Replace("{statusDescription}", fwShortener.StatusCodes.GetStatusDescription(statusCode))
            );
        });
    }

    static void HandleAdminPage(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            if (context.Request.Headers.ContainsKey("Authorization") || context.Request.Cookies.ContainsKey("auth"))
            {
                if (context.Request.Cookies["auth"] == LoginToken ||
                    context.Request.Headers["Authorization"].ToString().Contains(LoginToken))
                {
                    context.Response.Headers.Add("Set-Cookie", $"auth={LoginToken}");
                    await context.Response.WriteAsync(await File.ReadAllTextAsync("wwwroot/admin.html"));
                }
                else
                {
                    context.Response.StatusCode = 401;
                    context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"admin\"");
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                context.Response.Headers.Add("WWW-Authenticate", "Basic relam=\"admin\"");
            }
        });
    }

    static void HandleApi(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            //endpoints.MapGet("/", () => "Api");
            endpoints.MapGet("/links/{from:int}/{count:int}", async (HttpRequest request, HttpResponse response, int from, int count) =>
            {
                if (request.Cookies["auth"] == LoginToken)
                {
                    response.ContentType = "application/json";

                    await response.WriteAsJsonAsync(await SQLConnection.GetUrls(from, count));
                }
                else
                {
                    response.StatusCode = 401;
                }
            });
            endpoints.MapGet("/reports/{from:int}/{count:int}", async (HttpRequest request, HttpResponse response, int from, int count) =>
            {
                if (request.Cookies["auth"] == LoginToken)
                {
                    response.ContentType = "application/json";

                    await response.WriteAsJsonAsync(await SQLConnection.GetReports(from, count));
                }
                else
                {
                    response.StatusCode = 401;
                }
            });
            
            endpoints.MapGet("/links", async () => await SQLConnection.GetUrlsCount());
            endpoints.MapGet("/reports", async () => await SQLConnection.GetReportsCount());

            endpoints.MapGet("/remove/link/{id}", async (HttpRequest request, HttpResponse response, string id) =>
            {
                if (request.Cookies["auth"] == LoginToken)
                {
                    response.ContentType = "application/json";

                    await response.WriteAsync((await SQLConnection.RemoveUrl(id)).ToString());
                }
                else
                {
                    response.StatusCode = 401;
                }
            });
            endpoints.MapGet("/remove/report/{rid}", async (HttpRequest request, HttpResponse response, string rid) =>
            {
                if (request.Cookies["auth"] == LoginToken)
                {
                    response.ContentType = "application/json";

                    await response.WriteAsync((await SQLConnection.RemoveReport(rid)).ToString());
                }
                else
                {
                    response.StatusCode = 401;
                }
            });
        });
        app.UseStatusCodePages();
    }
    
    /*
     * Url Shortener
    */
    #region Url Shortener
    static void HandleCreatePage(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            string randomId = SQLConnection.RandomString(8);
            string providedUrl = context.Request.Form["surl"];

            if (Uri.TryCreate(providedUrl, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                SQLConnection.SetUrl(randomId, providedUrl);
                await context.Response.WriteAsync($"https://filipton.tech/s/{randomId} will redirect you to: {context.Request.Form["surl"]}");   
            }
            else
            {
                await context.Response.WriteAsync($"Url that you provided was not valid url!");   
            }
        });
    }

    static void HandleShortLinks(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            string url = SQLConnection.GetUrl(context.Request.Path.Value.TrimStart('/'));
            if (!string.IsNullOrEmpty(url))
            {
                context.Response.Redirect(url);   
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        });
    }
    #endregion

    /*
     * Images Server
    */
    #region Images Server
    static void HandleUploadImage(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            string randomId = SQLConnection.RandomString(8);
            
            if (context.Request.Form.Files.Count == 1)
            {
                MemoryStream fileInMem = new MemoryStream();
                await context.Request.Form.Files[0].CopyToAsync(fileInMem);
                fileInMem.Position = 0;

                var image = Image.Load(fileInMem, out IImageFormat imageFormat);

                if (imageFormat.Name.Equals("PNG") || imageFormat.Name.Equals("JPEG"))
                {
                    await image.SaveAsJpegAsync(Path.Combine("images", randomId + ".jpg"));
                }
                else if (imageFormat.Name.Equals("GIF"))
                {
                    await image.SaveAsGifAsync(Path.Combine("images", randomId + ".gif"));
                }

                await context.Response.WriteAsync($"IMAGE SAVED AS: https://filipton.tech/i/{randomId} OR https://i.filipton.tech/{randomId}");
            }
            else
            {
                await context.Response.WriteAsync("This method takes one file!");
            }
        });
    }

    static void HandleImages(IApplicationBuilder app)
    {
        app.Run(async context =>
        {
            string path = context.Request.Path.ToString().TrimStart('/');

            // && Directory.Exists(new FileInfo(Path.Combine("images", path)).DirectoryName)
            if (!string.IsNullOrEmpty(path))
            {
                string[] files = Directory.GetFiles("images", $"{path}*");
                if (files.Length > 0)
                {
                    await context.Response.Body.WriteAsync(File.ReadAllBytes(files[0]));
                    return;
                }
            }
            context.Response.StatusCode = 404;
        });
    }
    #endregion
}
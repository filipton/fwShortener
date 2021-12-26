using fwShortener;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

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

app.Map("/s", HandleShortLinks);
app.Map("/i", HandleImages);

app.MapGet("/list-s", () =>
{
    return SQLConnection.GerALlUrls();
});

var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(options);

app.UseStaticFiles();

app.Run();


static void HandleErrorPage(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        string scString = context.Request.Query["statusCode"];
        int statusCode = int.Parse(string.IsNullOrEmpty(scString) ? "500" : scString);

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(File.ReadAllText("wwwroot/error.html").
            Replace("{statusCode}", statusCode.ToString()).
            Replace("{statusDescription}", fwShortener.StatusCodes.GetStatusDescription(statusCode))
        );
    });
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
            context.Request.Form.Files[0].CopyTo(fileInMem);
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
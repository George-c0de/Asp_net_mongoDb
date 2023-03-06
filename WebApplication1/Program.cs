using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using WebApplication1.Models;
using WebApplication1.Services;




//MongoDb

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
var pack = new ConventionPack();
pack.Add(new IgnoreExtraElementsConvention(true));
ConventionRegistry.Register("My Solution Conventions", pack, t => true);

// Add services to the container.
builder.Services.Configure<DatabaseSettings>(
	builder.Configuration.GetSection("Project20Database"));

builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<TestServices>();
builder.Services.AddSingleton<CategoryServices>();
builder.Services.AddSingleton<QuestionsServices>();
builder.Services.AddSingleton<ResultServices>();
builder.Services.AddSingleton<EmailMessageSender>();
//IMongoCollection<Users> Users; // коллекция в базе данных
//IGridFSBucket gridFS;
//////MongoDB
//var settings = MongoClientSettings.FromConnectionString("mongodb+srv://admin:admin@cluster0.c37dj.mongodb.net/Project20?retryWrites=true&w=majority");
//settings.ServerApi = new ServerApi(ServerApiVersion.V1);
//var client = new MongoClient(settings);
//var database = client.GetDatabase("test");
//gridFS = new GridFSBucket(database);
//Users = database.GetCollection<Users>("Products");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/api/Login");
		options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/api/Login");
	});
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
app.UseDeveloperExceptionPage();

app.UseStaticFiles();

app.UseDefaultFiles();


app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

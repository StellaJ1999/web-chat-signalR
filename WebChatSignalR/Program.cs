using WebChatSignalR.Endpoints;
using WebChatSignalR.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages()
   .WithStaticAssets();

app.MapAuthEndpoints();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

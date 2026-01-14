using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementPlatform.Data;
using TaskManagementPlatform.Models;
using TaskManagementPlatform.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IOpenAIService, OpenAIService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Visitor", "Member", "Organizer", "Administrator" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
    var adminEmail = "sergiusefu@gmail.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(user, "sergiusefu");
        await userManager.AddToRoleAsync(user, "Administrator");
    }

    var organizerEmail = "organizer@gmail.com";
    if (await userManager.FindByEmailAsync(organizerEmail) == null)
    {
        var user = new ApplicationUser { UserName = organizerEmail, Email = organizerEmail, EmailConfirmed = true };
        await userManager.CreateAsync(user, "organizer");
        await userManager.AddToRoleAsync(user, "Organizer");
    }

    var memberEmail = "member@gmail.com";
    if (await userManager.FindByEmailAsync(memberEmail) == null)
    {
        var user = new ApplicationUser { UserName = memberEmail, Email = memberEmail, EmailConfirmed = true };
        await userManager.CreateAsync(user, "member");
        await userManager.AddToRoleAsync(user, "Member");
    }
}

app.Run();

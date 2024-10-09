using AppControleDeGastos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configurar servi�os para o cont�iner de inje��o de depend�ncia
builder.Services.AddControllers();

// Configurar o ApplicationDbContext para usar SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar autentica��o com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "AuthCookie"; // Nome do cookie
        options.LoginPath = "/Auth/Login";  // Caminho de redirecionamento para login
        options.LogoutPath = "/Auth/Logout"; // Caminho de redirecionamento para logout
    });

// Configurar cache distribu�do e sess�o
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expira��o da sess�o
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Adicione os dom�nios permitidos aqui
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Permitir cookies e credenciais
    });
});

var app = builder.Build();

// Configurar o pipeline de requisi��o HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Adiciona o cabe�alho HSTS em ambientes de produ��o
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de CORS
app.UseCors("AllowSpecificOrigin"); // Aplicar a pol�tica de CORS configurada

// Middleware de sess�o e autentica��o
app.UseSession(); // Middleware de sess�o deve ser chamado antes de UseAuthentication
app.UseAuthentication(); // Adiciona a autentica��o
app.UseAuthorization();  // Adiciona a autoriza��o

// Mapeamento de controladores
app.MapControllers();

// Iniciar a aplica��o
app.Run();
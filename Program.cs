using AppControleDeGastos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços para o contêiner de injeção de dependência
builder.Services.AddControllers();

// Configurar o ApplicationDbContext para usar SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "AuthCookie"; // Nome do cookie
        options.LoginPath = "/Auth/Login";  // Caminho de redirecionamento para login
        options.LogoutPath = "/Auth/Logout"; // Caminho de redirecionamento para logout
    });

// Configurar cache distribuído e sessão
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Adicione os domínios permitidos aqui
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Permitir cookies e credenciais
    });
});

var app = builder.Build();

// Configurar o pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Adiciona o cabeçalho HSTS em ambientes de produção
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de CORS
app.UseCors("AllowSpecificOrigin"); // Aplicar a política de CORS configurada

// Middleware de sessão e autenticação
app.UseSession(); // Middleware de sessão deve ser chamado antes de UseAuthentication
app.UseAuthentication(); // Adiciona a autenticação
app.UseAuthorization();  // Adiciona a autorização

// Mapeamento de controladores
app.MapControllers();

// Iniciar a aplicação
app.Run();
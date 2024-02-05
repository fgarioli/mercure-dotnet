using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mercure.Publisher;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/", async (HttpContext context) =>
{
    var mensagem = await context.Request.ReadFromJsonAsync<Mensagem>();

    Console.WriteLine($"Mensagem recebida: {mensagem.Texto}");
    // Chave secreta para assinar o token
    var chaveSecreta = Environment.GetEnvironmentVariable("JWT_SECRET");
    Console.WriteLine(chaveSecreta);

    // Cria a chave de segurança usando a chave secreta
    var chave = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(chaveSecreta));

    // Cria as credenciais de assinatura usando a chave de segurança e o algoritmo de criptografia
    var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

    // Define a validade do token
    DateTime dataExpiracao = DateTime.UtcNow.AddHours(1);

    var mercure = new
    {
        publish = new[] { "*" },
    };

    // Serializa o objeto JSON para uma string
    string mercureJson = System.Text.Json.JsonSerializer.Serialize(mercure);

    // Cria uma lista de claims (informações sobre o usuário)
    var claims = new List<Claim>
        {
            new Claim("mercure", mercureJson, JsonClaimValueTypes.Json)
        };

    // Cria o token JWT
    var token = new JwtSecurityToken(
        issuer: "sua_issuer_aqui",
        audience: "sua_audience_aqui",
        expires: dataExpiracao,
        claims: claims,
        signingCredentials: credenciais
    );

    // Serializa o token para uma string
    string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    var dict = new Dictionary<string, string>
    {
        { "topic", "https://example.com/books/1" },
        { "data", mensagem.Texto },
        { "private", "true" }
    };
    using var client = new HttpClient();
    using var req = new HttpRequestMessage(HttpMethod.Post, "http://mercure/.well-known/mercure")
    {
        Content = new FormUrlEncodedContent(dict),
        Headers = { { "Authorization", $"Bearer {tokenString}" } }
    };
    using var res = await client.SendAsync(req);
    return res.Content.ReadAsStringAsync();
});

app.Run();

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using SkiServiceMongoDB.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Swashbuckle.AspNetCore.Annotations;


[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoCollection<Auftrag> _auftraegeCollection;

    public ValuesController(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
        var database = _mongoClient.GetDatabase("SkiServiceDB");
        _auftraegeCollection = database.GetCollection<Auftrag>("Auftraege");
    }

    // Aufträge auslesen
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<IEnumerable<Auftrag>> Get()
    {
        var auftraege = _auftraegeCollection.Find(new BsonDocument()).ToList();
        return Ok(auftraege);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Add a new Auftrag", Description = "Requires username and password. Returns JWT Bearer token.")]
    public ActionResult<string> Post([FromBody] UserCredentials userCredentials)
    {
        // Überprüfen Sie die Benutzeranmeldeinformationen und generieren Sie einen JWT-Token
        var isValid = ValidateUserCredentials(userCredentials.Username, userCredentials.Password);

        if (isValid)
        {
            var token = GenerateJwtToken(userCredentials.Username);
            return Ok(new { Token = token });
        }
        else
        {
            return Unauthorized("Invalid username or password");
        }
    }

    public class UserCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }


    // Aufträge bearbeiten
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    public ActionResult<string> Put(string id, [FromBody] Auftrag updatedAuftrag)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<Auftrag>.Filter.Eq(a => a.Id, objectId);
        var update = Builders<Auftrag>.Update
            .Set(a => a.Kundenname, updatedAuftrag.Kundenname)
            .Set(a => a.EMail, updatedAuftrag.EMail)
            .Set(a => a.Telefon, updatedAuftrag.Telefon)
            .Set(a => a.Prioritaet, updatedAuftrag.Prioritaet)
            .Set(a => a.Dienstleistung, updatedAuftrag.Dienstleistung);

        var result = _auftraegeCollection.UpdateOne(filter, update);

        if (result.ModifiedCount > 0)
        {
            return Ok($"Auftrag with ID {id} updated successfully");
        }
        else
        {
            return NotFound($"Auftrag with ID {id} not found");
        }
    }

    // Aufträge löschen
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    public ActionResult<string> Delete(string id)
    {
        var objectId = new ObjectId(id);
        var filter = Builders<Auftrag>.Filter.Eq(a => a.Id, objectId);

        var result = _auftraegeCollection.DeleteOne(filter);

        if (result.DeletedCount > 0)
        {
            return Ok($"Auftrag with ID {id} deleted successfully");
        }
        else
        {
            return NotFound($"Auftrag with ID {id} not found");
        }
    }

    private bool ValidateUserCredentials(string username, string password)
    {
      
        return Startup.ValidUsers.TryGetValue(username, out var expectedPassword) && password == expectedPassword;
    }

    private string GenerateJwtToken(string username)
    {
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("your_secret_key_with_at_least_128_bits");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
            Expires = DateTime.UtcNow.AddDays(7), // Ablaufzeit des Tokens
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    public class Auftrag
    {
        public ObjectId Id { get; set; }
        public string Kundenname { get; set; }
        public string EMail { get; set; }
        public string Telefon { get; set; }
        public string Prioritaet { get; set; }
        public string Dienstleistung { get; set; }
    }
}

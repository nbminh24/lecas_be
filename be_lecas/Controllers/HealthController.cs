using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using be_lecas.Models;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IMongoClient mongoClient, ILogger<HealthController> logger)
        {
            _mongoClient = mongoClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Test MongoDB connection
                var database = _mongoClient.GetDatabase("lecas");
                await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
                
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    mongodb = "connected",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    mongodb = "disconnected",
                    error = ex.Message,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
                });
            }
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            var healthInfo = new
            {
                status = "checking",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
                mongodb = new
                {
                    status = "checking",
                    connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") != null ? "configured" : "using appsettings",
                    database = "lecas"
                },
                redis = new
                {
                    status = "checking",
                    connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") != null ? "configured" : "using appsettings"
                }
            };

            try
            {
                // Test MongoDB connection
                var database = _mongoClient.GetDatabase("lecas");
                await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
                
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
                    mongodb = new
                    {
                        status = "connected",
                        connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") != null ? "configured" : "using appsettings",
                        database = "lecas"
                    },
                    redis = new
                    {
                        status = "unknown",
                        connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") != null ? "configured" : "using appsettings"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detailed health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
                    mongodb = new
                    {
                        status = "disconnected",
                        connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") != null ? "configured" : "using appsettings",
                        database = "lecas",
                        error = ex.Message
                    },
                    redis = new
                    {
                        status = "unknown",
                        connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") != null ? "configured" : "using appsettings"
                    }
                });
            }
        }
    }
} 
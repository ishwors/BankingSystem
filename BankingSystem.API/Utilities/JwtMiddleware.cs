namespace BankingSystem.API.Utilities
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger; // Add ILogger

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger) // Modify constructor to inject ILogger
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Retrieve the JWT token from wherever it is stored (e.g., cookies, headers, etc.)
            string jwtToken = RetrieveJwtToken(context); // Implement this method

            if (!string.IsNullOrEmpty(jwtToken))
            {
                // Check if the "Authorization" header already exists before adding it
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + jwtToken);
                    _logger.LogInformation("Added Authorization header with JWT token: Bearer {jwtToken}");
                }
                else
                {
                    _logger.LogWarning("Authorization header already exists. Skipping addition.");
                }
            }

            await _next(context);
        }

        private string RetrieveJwtToken(HttpContext context)
        {
            // Retrieve the JWT token from the Authorization header
            string authorizationHeader = context.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }

            string[] headerParts = authorizationHeader.Split(' ');
            if (headerParts.Length != 2 || !headerParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return headerParts[1];
        }
    }
}

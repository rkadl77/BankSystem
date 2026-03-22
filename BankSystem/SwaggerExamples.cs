using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BankSystem
{
    public class SwaggerExamples : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.Name == "CreateUserRequest")
            {
                schema.Example = new OpenApiObject
                {
                    ["firstName"] = new OpenApiString("John"),
                    ["lastName"] = new OpenApiString("Doe"),
                    ["email"] = new OpenApiString("john@bank.com"),
                    ["password"] = new OpenApiString("Secret123!"),
                    ["role"] = new OpenApiString("client")
                };
            }
            else if (context.Type.Name == "CreateAccountRequest")
            {
                schema.Example = new OpenApiObject
                {
                    ["currency"] = new OpenApiString("USD"),
                    ["clientId"] = new OpenApiString("00000000-0000-0000-0000-000000000000")
                };
            }
            else if (context.Type.Name == "TransferRequest")
            {
                schema.Example = new OpenApiObject
                {
                    ["fromAccountId"] = new OpenApiString("00000000-0000-0000-0000-000000000000"),
                    ["toAccountId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                    ["amount"] = new OpenApiDouble(100.00)
                };
            }
            else if (context.Type.Name == "LoginRequest")
            {
                schema.Example = new OpenApiObject
                {
                    ["username"] = new OpenApiString("john@bank.com"),
                    ["password"] = new OpenApiString("Secret123!")
                };
            }
        }
    }
}

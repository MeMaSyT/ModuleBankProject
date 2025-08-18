using Microsoft.OpenApi.Models;
using ModulebankProject.Features.Inbox.Events.AccountOpened;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ModulebankProject.Features.Inbox.Events
{
    public class EventDocumentFilter : IDocumentFilter
    {
        private static readonly Type[] EventTypes = new[]
        {
            typeof(AccountOpenedEvent),
            typeof(MoneyCreditedEvent),
            typeof(MoneyDebitedEvent),
            typeof(TransferCompletedEvent),
            typeof(InterestAccruedEvent),
            typeof(ClientChangeBlockEvent),
        };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // добавляем тег Events
            swaggerDoc.Tags ??= new List<OpenApiTag>();
            if (!swaggerDoc.Tags.Any(t => t.Name == "Events"))
                swaggerDoc.Tags.Add(new OpenApiTag { Name = "Events", Description = "Event contracts" });

            // добавляем схемы если ещё нет
            foreach (var t in EventTypes)
            {
                if (!swaggerDoc.Components.Schemas.ContainsKey(t.Name))
                    swaggerDoc.Components.Schemas[t.Name] = context.SchemaGenerator.GenerateSchema(t, context.SchemaRepository);
            }

            // добавляем пути/операции для каждого события
            swaggerDoc.Paths ??= new OpenApiPaths();

            foreach (var t in EventTypes)
            {
                var path = $"/events/{t.Name}"; // можно поменять путь на удобный для вас
                var operation = new OpenApiOperation
                {
                    Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Events" } },
                    Summary = $"Schema for {t.Name}",
                    Description = $"Возвращает контракт события {t.Name}",
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Event schema",
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.Schema,
                                            Id = t.Name
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                swaggerDoc.Paths[path] = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [OperationType.Get] = operation
                    }
                };
            }
        }
    }
}
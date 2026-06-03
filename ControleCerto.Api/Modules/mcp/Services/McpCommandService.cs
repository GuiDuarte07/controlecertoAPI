using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ControleCerto.DTOs.CreditPurchase;
using ControleCerto.DTOs.Transaction;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Models;
using ControleCerto.Modules.Mcp.Services.Interfaces;
using ControleCerto.Services.Interfaces;

namespace ControleCerto.Modules.Mcp.Services
{
    public class McpCommandService : IMcpCommandService
    {
        private readonly IToolRegistryService _toolRegistryService;
        private readonly IMcpAuthorizationService _mcpAuthorizationService;
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly ICreditCardService _creditCardService;
        private readonly ILogger<McpCommandService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public McpCommandService(
            IToolRegistryService toolRegistryService,
            IMcpAuthorizationService mcpAuthorizationService,
            ITransactionService transactionService,
            IAccountService accountService,
            ICategoryService categoryService,
            ICreditCardService creditCardService,
            ILogger<McpCommandService> logger)
        {
            _toolRegistryService = toolRegistryService;
            _mcpAuthorizationService = mcpAuthorizationService;
            _transactionService = transactionService;
            _accountService = accountService;
            _categoryService = categoryService;
            _creditCardService = creditCardService;
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<McpCommandResponse> ExecuteAsync(
            McpCommandRequest request,
            McpTokenClaims tokenClaims,
            CancellationToken cancellationToken = default)
        {
            var resource = NormalizeValue(request.Resource);
            var action = NormalizeValue(request.Action);

            if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(action))
            {
                return McpCommandResponse.FromError("command", "Campos 'resource' e 'action' são obrigatórios.");
            }

            var tool = _toolRegistryService.GetByResourceAction(resource, action);
            if (tool is null)
            {
                return McpCommandResponse.FromError("command", $"Tool não encontrada para '{resource}/{action}'.");
            }

            if (!_mcpAuthorizationService.HasRequiredPermissions(tokenClaims, tool.RequiredPermissions))
            {
                return McpCommandResponse.FromError("authorization", "Usuário sem permissão para executar esta tool.");
            }

            _logger.LogInformation(
                "MCP command started. UserId={UserId}, Tool={ToolName}, Resource={Resource}, Action={Action}",
                tokenClaims.UserId,
                tool.Name,
                resource,
                action);

            McpCommandResponse response;

            try
            {
                var commandKey = $"{resource}:{action}";
                response = commandKey switch
                {
                    "transaction:list" => await ExecuteListTransactionsAsync(request.Payload, tokenClaims.UserId, cancellationToken),
                    "transaction:create" => await ExecuteCreateTransactionAsync(request.Payload, tokenClaims.UserId, cancellationToken),
                    "credit_purchase:create" => await ExecuteCreateCreditPurchaseAsync(request.Payload, tokenClaims.UserId, cancellationToken),
                    "transaction:update" => await ExecuteUpdateTransactionAsync(request.Payload, tokenClaims.UserId, cancellationToken),
                    "transaction:delete" => await ExecuteDeleteTransactionAsync(request.Payload, tokenClaims.UserId, cancellationToken),
                    "account:list" => await ExecuteListAccountsAsync(tokenClaims.UserId),
                    "category:list" => await ExecuteListCategoriesAsync(tokenClaims.UserId),
                    "credit_card:list" => await ExecuteListCreditCardsAsync(tokenClaims.UserId),
                    _ => McpCommandResponse.FromError("command", "Tool registrada sem implementação.")
                };
            }
            catch (JsonException)
            {
                response = McpCommandResponse.FromError("payload", "Payload inválido para o comando solicitado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "MCP command failed unexpectedly. UserId={UserId}, Resource={Resource}, Action={Action}",
                    tokenClaims.UserId,
                    resource,
                    action);

                response = McpCommandResponse.FromError("command", "Falha interna ao executar a tool MCP.");
            }

            if (response.Success)
            {
                _logger.LogInformation(
                    "MCP command succeeded. UserId={UserId}, Resource={Resource}, Action={Action}",
                    tokenClaims.UserId,
                    resource,
                    action);
            }
            else
            {
                _logger.LogWarning(
                    "MCP command failed. UserId={UserId}, Resource={Resource}, Action={Action}, Errors={Errors}",
                    tokenClaims.UserId,
                    resource,
                    action,
                    string.Join(" | ", response.Errors.Select(error => $"{error.Field}: {error.Message}")));
            }

            return response;
        }

        private async Task<McpCommandResponse> ExecuteListTransactionsAsync(
            JsonElement? payload,
            int userId,
            CancellationToken cancellationToken)
        {
            var deserializeError = TryDeserializePayload(payload, requiredPayload: false, out ListTransactionsPayload? model);
            if (deserializeError is not null)
            {
                return deserializeError;
            }

            model ??= new ListTransactionsPayload();

            var validationErrors = ValidateModel(model);
            if (validationErrors.Count > 0)
            {
                return McpCommandResponse.FromErrors(validationErrors);
            }

            var mode = string.IsNullOrWhiteSpace(model.Mode) ? "statement" : model.Mode.Trim().ToLowerInvariant();
            if (mode is not "invoice" and not "statement")
            {
                return McpCommandResponse.FromError("mode", "Campo 'mode' deve ser 'invoice' ou 'statement'.");
            }

            var endDate = model.EndDate ?? DateTime.UtcNow.Date;
            var startDate = model.StartDate ?? endDate.AddMonths(-1);

            if (startDate > endDate)
            {
                return McpCommandResponse.FromError("dateRange", "'startDate' não pode ser maior que 'endDate'.");
            }

            var result = await _transactionService.GetTransactionsAsync(
                userId,
                ToUtc(startDate),
                ToUtc(endDate),
                mode,
                model.AccountId,
                model.CardId,
                model.CategoryId,
                model.SearchText,
                model.Sort,
                model.PageNumber ?? 1,
                model.PageSize ?? 20);

            if (result.IsError)
            {
                return McpCommandResponse.FromError("command", result.Error.ErrorMessage);
            }

            return McpCommandResponse.FromSuccess(new ListTransactionsToolResponse
            {
                Transactions = result.Value
            });
        }

        private async Task<McpCommandResponse> ExecuteCreateTransactionAsync(
            JsonElement? payload,
            int userId,
            CancellationToken cancellationToken)
        {
            var deserializeError = TryDeserializePayload(payload, requiredPayload: true, out CreateTransactionPayload? model);
            if (deserializeError is not null || model is null)
            {
                return deserializeError ?? McpCommandResponse.FromError("payload", "Payload inválido.");
            }

            var validationErrors = ValidateModel(model);
            if (validationErrors.Count > 0)
            {
                return McpCommandResponse.FromErrors(validationErrors);
            }

            if (!model.Type.HasValue)
            {
                return McpCommandResponse.FromError("type", "Campo 'type' não informado.");
            }

            if (model.Type is not TransactionTypeEnum.EXPENSE and not TransactionTypeEnum.INCOME)
            {
                return McpCommandResponse.FromError("type", "Campo 'type' deve ser EXPENSE ou INCOME.");
            }

            var request = new CreateTransactionRequest
            {
                AccountId = model.AccountId,
                Amount = model.Amount,
                PurchaseDate = ToUtc(model.PurchaseDate),
                Destination = model.Destination,
                Description = model.Description ?? string.Empty,
                Observations = model.Observations,
                CategoryId = model.CategoryId,
                Type = model.Type.Value,
                JustForRecord = model.JustForRecord ?? false
            };

            return await ToMcpResponseAsync(_transactionService.CreateTransactionAsync(request, userId));
        }

        private async Task<McpCommandResponse> ExecuteCreateCreditPurchaseAsync(
            JsonElement? payload,
            int userId,
            CancellationToken cancellationToken)
        {
            var deserializeError = TryDeserializePayload(payload, requiredPayload: true, out CreateCreditPurchasePayload? model);
            if (deserializeError is not null || model is null)
            {
                return deserializeError ?? McpCommandResponse.FromError("payload", "Payload inválido.");
            }

            var validationErrors = ValidateModel(model);
            if (validationErrors.Count > 0)
            {
                return McpCommandResponse.FromErrors(validationErrors);
            }

            if (model.InstallmentsPaid != 0)
            {
                return McpCommandResponse.FromError("installmentsPaid", "Campo 'installmentsPaid' deve ser 0 na criação.");
            }

            var request = new CreateCreditPurchaseRequest
            {
                CreditCardId = model.CreditCardId,
                TotalAmount = model.TotalAmount,
                TotalInstallment = model.TotalInstallment,
                InstallmentsPaid = model.InstallmentsPaid,
                PurchaseDate = ToUtc(model.PurchaseDate),
                Destination = model.Destination,
                Description = model.Description,
                CategoryId = model.CategoryId
            };

            return await ToMcpResponseAsync(_creditCardService.CreateCreditPurchaseAsync(request, userId));
        }

        private async Task<McpCommandResponse> ExecuteUpdateTransactionAsync(
            JsonElement? payload,
            int userId,
            CancellationToken cancellationToken)
        {
            var deserializeError = TryDeserializePayload(payload, requiredPayload: true, out UpdateTransactionPayload? model);
            if (deserializeError is not null || model is null)
            {
                return deserializeError ?? McpCommandResponse.FromError("payload", "Payload inválido.");
            }

            var validationErrors = ValidateModel(model);
            if (validationErrors.Count > 0)
            {
                return McpCommandResponse.FromErrors(validationErrors);
            }

            var hasAnyUpdateField = model.Amount.HasValue
                                    || model.PurchaseDate.HasValue
                                    || model.Destination is not null
                                    || model.Description is not null
                                    || model.Observations is not null
                                    || model.JustForRecord.HasValue
                                    || model.CategoryId.HasValue;

            if (!hasAnyUpdateField)
            {
                return McpCommandResponse.FromError("payload", "Informe ao menos um campo para atualização.");
            }

            var request = new UpdateTransactionRequest
            {
                Id = model.TransactionId,
                Amount = model.Amount,
                PurchaseDate = model.PurchaseDate.HasValue ? ToUtc(model.PurchaseDate.Value) : null,
                Destination = model.Destination,
                Description = model.Description,
                Observations = model.Observations,
                JustForRecord = model.JustForRecord,
                CategoryId = model.CategoryId
            };

            return await ToMcpResponseAsync(_transactionService.UpdateTransactionAsync(request, userId));
        }

        private async Task<McpCommandResponse> ExecuteDeleteTransactionAsync(
            JsonElement? payload,
            int userId,
            CancellationToken cancellationToken)
        {
            var deserializeError = TryDeserializePayload(payload, requiredPayload: true, out DeleteTransactionPayload? model);
            if (deserializeError is not null || model is null)
            {
                return deserializeError ?? McpCommandResponse.FromError("payload", "Payload inválido.");
            }

            var validationErrors = ValidateModel(model);
            if (validationErrors.Count > 0)
            {
                return McpCommandResponse.FromErrors(validationErrors);
            }

            if (model.TransactionId > int.MaxValue)
            {
                return McpCommandResponse.FromError("transactionId", "Campo 'transactionId' excede o limite permitido.");
            }

            return await ToMcpResponseAsync(_transactionService.DeleteTransactionAsync((int)model.TransactionId, userId));
        }

        private async Task<McpCommandResponse> ExecuteListAccountsAsync(int userId)
        {
            return await ToMcpResponseAsync(_accountService.GetAccountsByUserIdAsync(userId));
        }

        private async Task<McpCommandResponse> ExecuteListCategoriesAsync(int userId)
        {
            return await ToMcpResponseAsync(_categoryService.GetAllCategoriesAsync(userId, null));
        }

        private async Task<McpCommandResponse> ExecuteListCreditCardsAsync(int userId)
        {
            return await ToMcpResponseAsync(_creditCardService.GetCreditCardInfo(userId));
        }

        private static string NormalizeValue(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private McpCommandResponse? TryDeserializePayload<T>(
            JsonElement? payload,
            bool requiredPayload,
            out T? model) where T : class, new()
        {
            model = null;

            if (!payload.HasValue || payload.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                if (requiredPayload)
                {
                    return McpCommandResponse.FromError("payload", "Payload é obrigatório para este comando.");
                }

                model = new T();
                return null;
            }

            if (payload.Value.ValueKind != JsonValueKind.Object)
            {
                return McpCommandResponse.FromError("payload", "Payload deve ser um objeto JSON.");
            }

            model = payload.Value.Deserialize<T>(_jsonSerializerOptions);

            if (model is null)
            {
                return McpCommandResponse.FromError("payload", "Não foi possível desserializar o payload.");
            }

            return null;
        }

        private static List<McpCommandError> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);

            return validationResults
                .SelectMany(result =>
                {
                    var members = result.MemberNames.Any()
                        ? result.MemberNames
                        : new[] { "payload" };

                    return members.Select(member => new McpCommandError
                    {
                        Field = ToCamelCase(member),
                        Message = result.ErrorMessage ?? "Campo inválido."
                    });
                })
                .ToList();
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "payload";
            }

            if (value.Length == 1)
            {
                return value.ToLowerInvariant();
            }

            return char.ToLowerInvariant(value[0]) + value[1..];
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value.ToUniversalTime()
            };
        }

        private static async Task<McpCommandResponse> ToMcpResponseAsync<T>(Task<Result<T>> domainTask)
        {
            var domainResult = await domainTask;

            if (domainResult.IsSuccess)
            {
                return McpCommandResponse.FromSuccess(domainResult.Value);
            }

            return McpCommandResponse.FromError("command", domainResult.Error.ErrorMessage);
        }
    }
}

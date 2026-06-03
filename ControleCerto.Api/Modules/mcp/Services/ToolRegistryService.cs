using System.Text.Json.Nodes;
using ControleCerto.Modules.Mcp.Models;
using ControleCerto.Modules.Mcp.Services.Interfaces;

namespace ControleCerto.Modules.Mcp.Services
{
    public class ToolRegistryService : IToolRegistryService
    {
        private static readonly IReadOnlyCollection<McpToolMetadata> ToolDefinitions = BuildToolDefinitions();

        public IReadOnlyCollection<McpToolMetadata> GetAllTools()
        {
            return ToolDefinitions;
        }

        public IReadOnlyCollection<McpToolMetadata> GetToolsForPermissions(IEnumerable<string> permissions)
        {
            var permissionSet = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);

            return ToolDefinitions
                .Where(tool => tool.RequiredPermissions.All(permissionSet.Contains))
                .ToArray();
        }

        public McpToolMetadata? GetByResourceAction(string resource, string action)
        {
            return ToolDefinitions.FirstOrDefault(tool =>
                string.Equals(tool.Resource, resource, StringComparison.OrdinalIgnoreCase)
                && string.Equals(tool.Action, action, StringComparison.OrdinalIgnoreCase));
        }

        public McpToolMetadata? GetByName(string name)
        {
            return ToolDefinitions.FirstOrDefault(tool =>
                string.Equals(tool.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static IReadOnlyCollection<McpToolMetadata> BuildToolDefinitions()
        {
            return new[]
            {
                new McpToolMetadata
                {
                    Name = "list_transactions",
                    Resource = "transaction",
                    Action = "list",
                    Description = "Retorna as transações do usuário atual para auxiliar a construção de comandos.",
                    RequiredPermissions = new[] { "transactions.read" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {
                            "startDate": {
                              "type": "string",
                              "format": "date",
                              "description": "Data inicial no formato YYYY-MM-DD."
                            },
                            "endDate": {
                              "type": "string",
                              "format": "date",
                              "description": "Data final no formato YYYY-MM-DD."
                            },
                            "mode": {
                              "type": "string",
                              "enum": ["invoice", "statement"],
                              "description": "Modo de listagem."
                            },
                            "accountId": {
                              "type": "integer",
                              "description": "Filtro opcional por conta."
                            },
                            "cardId": {
                              "type": "integer",
                              "description": "Filtro opcional por cartão de crédito."
                            },
                            "categoryId": {
                              "type": "integer",
                              "description": "Filtro opcional por categoria."
                            },
                            "searchText": {
                              "type": "string",
                              "description": "Texto de busca em descrição e destino."
                            },
                            "sort": {
                              "type": "string",
                              "description": "Ordenação no formato 'campo asc|desc'."
                            },
                            "pageNumber": {
                              "type": "integer",
                              "minimum": 1
                            },
                            "pageSize": {
                              "type": "integer",
                              "minimum": 1,
                              "maximum": 100
                            }
                          },
                          "required": []
                        }
                        """)!
                },
                new McpToolMetadata
                {
                    Name = "create_transaction",
                    Resource = "transaction",
                    Action = "create",
                    Description = "Cria uma nova transação financeira para o usuário.",
                    RequiredPermissions = new[] { "transactions.create" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {
                            "accountId": {
                              "type": "integer",
                              "description": "ID da conta onde a transação será registrada."
                            },
                            "amount": {
                              "type": "number",
                              "description": "Valor da transação com duas casas decimais. Deve ser positivo."
                            },
                            "purchaseDate": {
                              "type": "string",
                              "format": "date",
                              "description": "Data da transação no formato YYYY-MM-DD."
                            },
                            "destination": {
                              "type": "string",
                              "description": "Descrição ou estabelecimento da transação."
                            },
                            "description": {
                              "type": "string",
                              "maxLength": 100,
                              "description": "Título da transação."
                            },
                            "observations": {
                              "type": "string",
                              "maxLength": 300,
                              "nullable": true,
                              "description": "Observações internas da transação."
                            },
                            "categoryId": {
                              "type": "integer",
                              "description": "ID da categoria associada."
                            },
                            "type": {
                              "type": "string",
                              "enum": ["EXPENSE", "INCOME"],
                              "description": "Tipo da transação: 'EXPENSE' deve ser usado para qualquer saída de dinheiro (despesa, gasto) e 'INCOME' para qualquer entrada de dinheiro (receita, salário). Para compras em cartão de crédito, use a tool 'create_credit_purchase' para garantir o registro correto das parcelas e impacto nos saldos das contas."
                            },
                            "justForRecord": {
                              "type": "boolean",
                              "description": "Indica se a transação é apenas para registro. Por padrão mantenha o campo false a não ser que o usuário explicitamente queira criar uma transação que não impacta nos saldos das contas."
                            }
                          },
                          "required": [
                            "accountId",
                            "amount",
                            "purchaseDate",
                            "destination",
                            "categoryId",
                            "type",
                            "justForRecord"
                          ]
                        }
                        """)!
                },
                new McpToolMetadata
                {
                    Name = "create_credit_purchase",
                    Resource = "credit_purchase",
                    Action = "create",
                    Description = "Cria uma compra no crédito para o usuário.",
                    RequiredPermissions = new[] { "transactions.create" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {
                            "creditCardId": {
                              "type": "integer",
                              "description": "ID do cartão de crédito usado na compra."
                            },
                            "totalAmount": {
                              "type": "number",
                              "description": "Valor total da compra no crédito. Deve ser positivo."
                            },
                            "totalInstallment": {
                              "type": "integer",
                              "minimum": 1,
                              "description": "Número total de parcelas."
                            },
                            "installmentsPaid": {
                              "type": "integer",
                              "enum": [0],
                              "description": "Número de parcelas já pagas. Deve ser 0 na criação."
                            },
                            "purchaseDate": {
                              "type": "string",
                              "format": "date",
                              "description": "Data da compra no formato YYYY-MM-DD."
                            },
                            "destination": {
                              "type": "string",
                              "description": "Descrição ou estabelecimento da compra."
                            },
                            "description": {
                              "type": "string",
                              "maxLength": 100,
                              "nullable": true,
                              "description": "Título da compra no crédito."
                            },
                            "categoryId": {
                              "type": "integer",
                              "description": "ID da categoria associada."
                            }
                          },
                          "required": [
                            "creditCardId",
                            "totalAmount",
                            "totalInstallment",
                            "installmentsPaid",
                            "purchaseDate",
                            "destination",
                            "categoryId"
                          ]
                        }
                        """)!
                },
                new McpToolMetadata
                {
                    Name = "update_transaction",
                    Resource = "transaction",
                    Action = "update",
                    Description = "Atualiza uma transação existente do usuário.",
                    RequiredPermissions = new[] { "transactions.update" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {
                            "transactionId": {
                              "type": "integer",
                              "description": "ID da transação que será atualizada."
                            },
                            "amount": {
                              "type": "number",
                              "nullable": true
                            },
                            "purchaseDate": {
                              "type": "string",
                              "format": "date",
                              "nullable": true
                            },
                            "destination": {
                              "type": "string",
                              "maxLength": 80,
                              "nullable": true
                            },
                            "description": {
                              "type": "string",
                              "maxLength": 100,
                              "nullable": true
                            },
                            "observations": {
                              "type": "string",
                              "maxLength": 300,
                              "nullable": true
                            },
                            "justForRecord": {
                              "type": "boolean",
                              "nullable": true
                            },
                            "categoryId": {
                              "type": "integer",
                              "nullable": true
                            }
                          },
                          "required": ["transactionId"]
                        }
                        """)!
                },
                new McpToolMetadata
                {
                    Name = "delete_transaction",
                    Resource = "transaction",
                    Action = "delete",
                    Description = "Remove uma transação do usuário.",
                    RequiredPermissions = new[] { "transactions.delete" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {
                            "transactionId": {
                              "type": "integer",
                              "description": "ID da transação que será excluída."
                            }
                          },
                          "required": ["transactionId"]
                        }
                        """)!
                },
                new McpToolMetadata
                {
                    Name = "list_accounts",
                    Resource = "account",
                    Action = "list",
                    Description = "Retorna as contas do usuário para permitir seleção por ID em outras tools.",
                    RequiredPermissions = new[] { "accounts.read" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {},
                          "required": []
                        }
                        """)!
                },
                new McpToolMetadata
                {
                    Name = "list_categories",
                    Resource = "category",
                    Action = "list",
                    Description = "Retorna as categorias disponíveis para o usuário.",
                    RequiredPermissions = new[] { "categories.read" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {},
                          "required": []
                        }
                        """)!
                },
                new McpToolMetadata
                {
                    Name = "list_credit_cards",
                    Resource = "credit_card",
                    Action = "list",
                    Description = "Retorna os cartões de crédito do usuário para permitir seleção por ID.",
                    RequiredPermissions = new[] { "credit_cards.read" },
                    Parameters = JsonNode.Parse(
                        """
                        {
                          "type": "object",
                          "properties": {},
                          "required": []
                        }
                        """)!
                }
            };
        }
    }
}

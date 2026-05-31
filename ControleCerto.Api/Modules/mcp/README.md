# MCP Module - Especificação de Implementação

## Objetivo

Criar um módulo MCP dentro da API `ControleCerto.Api` para expor um servidor de ferramentas que:
- disponibiliza a lista de tools autorizadas ao usuário
- executa comandos específicos no backend
- valida payloads e permissions no servidor
- usa um token de autorização especial para acesso MCP, diferente do token de login normal

## Estrutura do módulo

O módulo será organizado em:

- `Controllers/`
  - `McpToolsController.cs` - lista de tools disponíveis para o usuário
  - `McpCommandsController.cs` - executa comandos tool
- `DTOs/`
  - `McpCommandRequest.cs` - request genérico do MCP
  - `McpCommandResponse.cs` - response genérico do MCP
  - `ToolMetadataResponse.cs` - metadata de tool para descoberta
  - `CreateTransactionPayload.cs` - payload específico de criação de transação
  - `ListTransactionsToolResponse.cs` - retorno da tool de listagem de transações
- `Services/`
  - `IMcpCommandService.cs`
  - `McpCommandService.cs`
  - `IToolRegistryService.cs`
  - `ToolRegistryService.cs`
  - `IMcpAuthorizationService.cs`
  - `McpAuthorizationService.cs`
- `Models/`
  - `McpToolMetadata.cs` - modelo de metadados de tool
  - `McpTokenClaims.cs` - modelo dos claims que o token especial deve carregar
- `Utils/` (opcional)
  - helpers de geração/validação de token MCP

## Passo a passo de implementação

### 1. Criar o contrato de autorização MCP

- Definir um token separado para uso MCP
- O token deve ser emitido por um endpoint interno (ou processo de geração especial)
- O token deve ter validade longa, por exemplo `1 mês`
- O token deve carregar:
  - `UserId`
  - `Permissions` ou `Roles` específicos para MCP
  - `IssuedAt`
  - `ExpiresAt`
  - `TokenType: "mcp"`

### 2. Criar o mecanismo de validação no backend

- `IMcpAuthorizationService`
- `McpAuthorizationService`
  - valida o token MCP
  - extrai claims para `HttpContext.Items`
  - garante que o token seja do tipo correto
  - valida se o usuário ainda existe e está ativo
  - valida permissions específicas da tool solicitada

### 3. Criar a lista de tools no servidor

- `IToolRegistryService`
- `ToolRegistryService`
  - retorna todas as tools possíveis
  - filtra tools por permission do usuário
  - mantém a definição de schema/parameters de cada tool
  - pode carregar tool metadata via configuração ou código estático

### 4. Criar controllers MCP

#### `McpToolsController`
- endpoint: `GET /api/mcp/tools`
- autenticação MCP obrigatória
- retorna a lista de tools disponíveis para o usuário autenticado
- cada tool inclui:
  - `name`
  - `description`
  - `parameters` (schema JSON)
  - `requiredPermissions`

#### `McpCommandsController`
- endpoint: `POST /api/mcp/commands`
- autenticação MCP obrigatória
- aceita `McpCommandRequest`
- valida o comando e o payload
- chama `IMcpCommandService.ExecuteAsync`
- retorna `McpCommandResponse`

### 5. Criar `McpCommandService`

- serviço central que decide qual tool executar
- usa `ToolRegistryService` para validar se a tool existe
- usa `McpAuthorizationService` para verificar autorização de execução
- faz desserialização do payload para o tipo correto
- chama o serviço de domínio apropriado (por exemplo, `ITransactionService`)
- retorna resultado padronizado ao controller

### 6. Criar a ferramenta de listagem de transações

- é uma tool read-only, usada para buscar transações do usuário
- deve retornar apenas dados que o usuário pode ver
- deve ser definida no registry com schema vazio ou parâmetros opcionais
- serve como exemplo de ferramenta disponível no MCP

## Exemplo de tool: Listar transações

### Metadata da tool

- `name`: `list_transactions`
- `description`: `Retorna as transações do usuário atual para auxiliar a construção de comandos.`
- `parameters`:
  - `type`: `object`
  - `properties`:
    - `startDate`: `{ type: "string", format: "date", description: "Data inicial no formato YYYY-MM-DD." }`
    - `endDate`: `{ type: "string", format: "date", description: "Data final no formato YYYY-MM-DD." }`
    - `mode`: `{ type: "string", enum: ["invoice", "statement"], description: "Modo de listagem." }`
  - `required`: []
- `requiredPermissions`: `["transactions.read"]`

### Request MCP

```json
{
  "resource": "transaction",
  "action": "list",
  "payload": {
    "startDate": "2026-05-01",
    "endDate": "2026-05-30",
    "mode": "statement"
  }
}
```

### Response esperada

```json
{
  "success": true,
  "data": [
    {
      "id": 123,
      "amount": 150.00,
      "date": "2026-05-25",
      "destination": "Supermercado",
      "categoryId": 10,
      "type": "expense"
    }
  ]
}
```

### Implementação ideal da tool

- `McpCommandService` detecta `resource == "transaction"` e `action == "list"`
- valida permissions via `McpAuthorizationService`
- chama o serviço de transação existente para buscar dados
- retorna o resultado em `McpCommandResponse`

## Autorização com token especial

### Token MCP versus token de login

- token de login normal continua sendo usado para a UI padrão e fluxos normais
- token MCP é um token distinto, emitido especificamente para o servidor de ferramentas
- token MCP deve conter dados suficientes para autorizar tools sem precisar usar o login padrão a cada chamada
- validade sugerida: `30 dias`
- formato sugerido: JWT ou token assinado com claims

### Claims mínimos do token MCP

- `sub` / `userId`
- `type`: `mcp`
- `exp`
- `iat`
- `permissions`: `["transactions.read", "transactions.create", ...]`
- `tenantId` / `accountId` (se houver suporte multi-tenant)

### Fluxo de uso do token MCP

1. cliente solicita um token MCP em rota segura ou processo de geração
2. servidor emite token especial somente se o usuário e o contexto permitirem
3. cliente que roda LLM usa token MCP para chamar `GET /api/mcp/tools`
4. servidor devolve as tools permitidas
5. cliente usa token MCP para chamar `POST /api/mcp/commands`
6. servidor valida token MCP e executa a tool

### Regras de segurança

- não aceite token MCP no mesmo pipeline de autenticação JWT normal sem verificação de `type`
- sempre validar `TokenType == "mcp"`
- nunca aceitar tokens expirados
- não use token MCP para ações que exigem autenticação normal de usuário sem validação adicional
- logar auditoria de cada comando executado via MCP

## Próximos passos

1. criar a pasta `ControleCerto.Api\Modules\mcp\Controllers`
2. criar `McpToolsController.cs` e `McpCommandsController.cs`
3. criar `DTOs/Mcp/McpCommandRequest.cs`, `McpCommandResponse.cs`, `ToolMetadataResponse.cs`
4. criar `Services/Interfaces/IMcpCommandService.cs`, `IToolRegistryService.cs`, `IMcpAuthorizationService.cs`
5. criar implementações de serviços MCP
6. criar `ToolRegistryService` com a tool `list_transactions` e estrutura de metadata
7. registrar serviços em `Program.cs`
8. criar endpoint de emissão/renovação de token MCP se necessário

## Observações

- A lista de tools deve ser autoritativa no backend, não no front-end.
- O token especial é gerenciado pelo MCP e deve ser o único usado para operar esse endpoint.
- O backend deve fornecer apenas as ferramentas que o usuário tem direito de usar.
- A tool de listagem de transações é um bom exemplo porque mostra como um recurso de consulta pode ser exposto antes da execução de comandos de criação.

---

> Este documento é um ponto de partida. Você pode estender com exemplos de `ToolMetadata`, `schema JSON` de cada tool e a estrutura exata dos serviços conforme for avançando na implementação.

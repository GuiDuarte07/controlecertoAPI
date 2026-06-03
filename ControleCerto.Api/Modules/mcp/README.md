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

### 5.1. Delegação para services do domínio

- Cada tool do MCP deve delegar a execução ao service responsável pela ação.
- Não implementar regras de negócio de transação, conta, categoria ou cartão diretamente no MCP.
- Todos os services do sistema estão em `ControleCerto.Api\Services\Interfaces`, e o MCP deve consultar essas interfaces para saber quais métodos existem.
- A LLM que implementar o MCP deve inspecionar as interfaces desses services antes de montar a chamada de comando. Isso reduz consumo de token, evita adivinhações e preserva a camada de domínio existente.
- Exemplos de delegação:
  - `list_transactions` -> `ITransactionService` / método de listagem de transações
  - `create_transaction` -> `ITransactionService` / método de criação de transação
  - `list_accounts` -> `IAccountService` / método de listagem de contas
  - `list_categories` -> `ICategoryService` / método de listagem de categorias
  - `list_credit_cards` -> `ICreditCardService` / método de listagem de cartões de crédito

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

## Ferramentas planejadas

Essas tools iniciais contemplam as necessidades iniciais para criar transações, desde que o fluxo de descoberta de IDs seja usado corretamente. A tool `create_transaction` é a principal para criação de lançamentos, enquanto `list_accounts` e `list_categories` dão suporte para obter IDs válidos antes de enviar o comando. `list_transactions` é útil para inspecionar o contexto atual do usuário e confirmar resultados.

### 1. list_transactions
- `name`: `list_transactions`
- `resource`: `transaction`
- `action`: `list`
- `description`: `Retorna as transações do usuário atual para auxiliar a construção de comandos.`
- `requiredPermissions`: `["transactions.read"]`
- `parameters`:
  - `startDate`: `string` (YYYY-MM-DD)
  - `endDate`: `string` (YYYY-MM-DD)
  - `mode`: `string` (`"invoice"` | `"statement"`)
- `response.data`: lista de transações com campos como `id`, `amount`, `date`, `destination`, `categoryId`, `type` (ControleCerto.Api\DTOs\Transaction\InfoTransactionResponse.cs)

### 2. create_transaction
```json
{
  "name": "create_transaction",
  "description": "Cria uma nova transação financeira para o usuário.",
  "resource": "transaction",
  "action": "create",
  "requiredPermissions": ["transactions.create"],
  "parameters": {
    "type": "object",
    "properties": {
      "accountId": {
        "type": "integer",
        "description": "ID da conta onde a transação será registrada. Se necessário, use list_accounts para buscar o ID correto."
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
        "description": "Título da transação.",
        "maxLength": 100,
      },
      "observations": {
        "type": "string",
        "description": "Observações internas da transação.",
        "maxLength": 300,
        "nullable": true
      },
      "categoryId": {
        "type": "integer",
        "description": "ID da categoria associada. Se necessário, use list_categories para buscar o ID correto."
      },
      "type": {
        "type": "string",
        "enum": ["EXPENSE", "INCOME"],
        "description": "Tipo de transação. Use EXPENSE para despesas e INCOME para receitas." 
      },
      "justForRecord": {
        "type": "boolean",
        "description": "Indica se a transação é apenas para registro. Considerar sempre false desde que o usuário não indique o contrário."
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
}
```

#### Request exemplo para `create_transaction`

```json
{
  "resource": "transaction",
  "action": "create",
  "payload": {
    "accountId": 12,
    "amount": 1200.50,
    "purchaseDate": "2026-05-31",
    "destination": "Mercado Central",
    "description": "Compra de supermercado",
    "observations": "Feira do mês, carnes, produtos de limpeza e higiene e alimentos.",
    "categoryId": 5,
    "type": "EXPENSE",
    "justForRecord": false
  }
}
```

### 3. create_credit_purchase
```json
{
  "name": "create_credit_purchase",
  "description": "Cria uma compra no crédito para o usuário.",
  "resource": "credit_purchase",
  "action": "create",
  "requiredPermissions": ["transactions.create"],
  "parameters": {
    "type": "object",
    "properties": {
      "creditCardId": {
        "type": "integer",
        "description": "ID do cartão de crédito usado na compra. Use list_credit_cards para obter IDs válidos."
      },
      "totalAmount": {
        "type": "number",
        "description": "Valor total da compra no crédito. Deve ser positivo."
      },
      "totalInstallment": {
        "type": "integer",
        "description": "Número total de parcelas da compra no crédito.",
        "minimum": 1
      },
      "installmentsPaid": {
        "type": "integer",
        "description": "Número de parcelas já pagas. Deve ser 0 na criação.",
        "enum": [0]
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
        "description": "Título da compra no crédito.",
        "maxLength": 100,
        "nullable": true
      },
      "categoryId": {
        "type": "integer",
        "description": "ID da categoria associada. Se necessário, use list_categories para buscar o ID correto."
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
}
```

### 4. update_transaction
```json
{
  "name": "update_transaction",
  "description": "Atualiza uma transação existente do usuário.",
  "resource": "transaction",
  "action": "update",
  "requiredPermissions": ["transactions.update"],
  "parameters": {
    "type": "object",
    "properties": {
      "transactionId": {
        "type": "integer",
        "description": "ID da transação que será atualizada."
      },
      "amount": {
        "type": "number",
        "description": "Novo valor da transação. Deve ser positivo se informado.",
        "nullable": true
      },
      "purchaseDate": {
        "type": "string",
        "format": "date",
        "description": "Nova data da transação, no formato YYYY-MM-DD.",
        "nullable": true
      },
      "destination": {
        "type": "string",
        "description": "Novo estabelecimento ou destino da transação.",
        "maxLength": 80,
        "nullable": true
      },
      "description": {
        "type": "string",
        "description": "Nova descrição da transação.",
        "maxLength": 100,
        "nullable": true
      },
      "observations": {
        "type": "string",
        "description": "Observações internas atualizadas da transação.",
        "maxLength": 300,
        "nullable": true
      },
      "justForRecord": {
        "type": "boolean",
        "description": "Indica se a transação deve ser marcada apenas para registro.",
        "nullable": true
      },
      "categoryId": {
        "type": "integer",
        "description": "Novo ID da categoria associada. Se necessário, use list_categories para buscar o ID correto.",
        "nullable": true
      }
    },
    "required": ["transactionId"]
  }
}
```

### 5. delete_transaction
```json
{
  "name": "delete_transaction",
  "description": "Remove uma transação do usuário.",
  "resource": "transaction",
  "action": "delete",
  "requiredPermissions": ["transactions.delete"],
  "parameters": {
    "type": "object",
    "properties": {
      "transactionId": {
        "type": "integer",
        "description": "ID da transação que será excluída."
      }
    },
    "required": ["transactionId"]
  }
}
```

### 6. list_accounts
```json
{
  "name": "list_accounts",
  "description": "Retorna as contas do usuário para permitir seleção por ID em outras tools.",
  "resource": "account",
  "action": "list",
  "requiredPermissions": ["accounts.read"],
  "parameters": {
    "type": "object",
    "properties": {},
    "required": []
  }
}
```

### 7. list_categories
```json
{
  "name": "list_categories",
  "description": "Retorna as categorias disponíveis para o usuário.",
  "resource": "category",
  "action": "list",
  "requiredPermissions": ["categories.read"],
  "parameters": {
    "type": "object",
    "properties": {},
    "required": []
  }
}
```

### 8. list_credit_cards
```json
{
  "name": "list_credit_cards",
  "description": "Retorna os cartões de crédito do usuário para permitir seleção por ID ou uso em outras operações.",
  "resource": "credit_card",
  "action": "list",
  "requiredPermissions": ["credit_cards.read"],
  "parameters": {
    "type": "object",
    "properties": {},
    "required": []
  }
}
```

## Contratos genéricos de ferramenta

Todas as ferramentas expostas pelo MCP seguem o mesmo envelope de request:

```json
{
  "resource": "<resource>",
  "action": "<action>",
  "payload": { ... }
}
```

Internamente, o MCP pode mapear o comando recebido por `resource` + `action` para a tool registrada. O campo `name` no registro é a chave de metadata usada para descoberta e documentação, mas a execução segue o contrato genérico `resource/action/payload`.

E retornam um envelope uniforme:

```json
{
  "success": true,
  "data": { ... },
  "errors": []
}
```

Erros de validação ou autorização ficam em `errors` com objetos do tipo:

```json
{
  "field": "<campo>",
  "message": "<mensagem>"
}
```

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

### Fluxo de uso do token MCP

1. cliente solicita um token MCP em rota segura ou processo de geração
2. servidor emite token especial somente se o usuário e o contexto permitirem
3. cliente que roda LLM usa token MCP para chamar `GET /api/mcp/tools`
4. servidor devolve as tools permitidas
5. cliente usa token MCP para chamar `POST /api/mcp/commands`
6. servidor valida token MCP e executa a tool

### Rota de geração do token MCP

- rota: `POST /api/mcp/token`
- módulo: `ControleCerto.Api\Modules\mcp`
- responsabilidade: gerar e retornar um bearer token MCP para o usuário autenticado
- entrada: nenhum dado necessário
- saída: `{ "accessToken": "Bearer ...", "expiresAt": "2026-06-30T..." }`
- o serviço de geração deve preencher o claim `permissions` com os privilégios MCP disponíveis para o usuário no momento da emissão
- garanti que o token contenha `type: "mcp"`, `sub/userId`, `iat`, `exp` e `permissions`
- atenção: se novas tools forem adicionadas, pode ser necessário renovar o token para refletir o novo conjunto de permissões, a menos que o backend trate permissões dinamicamente em tempo de execução


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

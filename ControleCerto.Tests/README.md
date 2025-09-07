# ControleCerto.Tests

Este projeto contém todos os testes unitários e de integração para o sistema ControleCerto.

## Estrutura de Pastas

```
ControleCerto.Tests/
├── Controllers/          # Testes para Controllers
├── Services/            # Testes para Services
├── Validations/         # Testes para Validações de Negócio
├── Utils/              # Testes para Utilitários
└── README.md           # Este arquivo
```

## Tecnologias Utilizadas

- **xUnit**: Framework de testes
- **FluentAssertions**: Biblioteca para assertions mais legíveis
- **Moq**: Framework para mocking
- **Entity Framework InMemory**: Para testes de integração com banco de dados

## Como Executar os Testes

### Via Terminal
```bash
dotnet test
```

### Via Visual Studio
1. Abra o Test Explorer (Test > Test Explorer)
2. Execute todos os testes ou testes específicos

### Com Cobertura de Código
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Convenções de Nomenclatura

### Classes de Teste
- Nome da classe: `{ClasseSendoTestada}Tests`
- Exemplo: `RecurringTransactionValidationsTests`

### Métodos de Teste
- Padrão: `{Método}_{Cenário}_{ResultadoEsperado}`
- Exemplo: `ValidateRecurrenceRuleRequest_WithValidDailyRule_ShouldReturnSuccess`

### Organização dos Testes
- Use `#region` para agrupar testes relacionados
- Separe cenários de sucesso e erro
- Inclua casos extremos (edge cases)

## Exemplo de Teste

```csharp
[Fact]
public void ValidateRecurrenceRuleRequest_WithValidDailyRule_ShouldReturnSuccess()
{
    // Arrange
    var request = new CreateRecurrenceRuleRequest
    {
        Frequency = RecurrenceFrequencyEnum.DAILY,
        IsEveryDay = true,
        Interval = 1
    };

    // Act
    var result = RecurringTransactionValidations.ValidateRecurrenceRuleRequest(request);

    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

## Boas Práticas

1. **AAA Pattern**: Arrange, Act, Assert
2. **Testes Independentes**: Cada teste deve ser independente
3. **Nomes Descritivos**: Nomes que explicam o que está sendo testado
4. **Cobertura Completa**: Teste cenários de sucesso, erro e casos extremos
5. **Mocks**: Use mocks para dependências externas
6. **Dados de Teste**: Use dados realistas mas simples

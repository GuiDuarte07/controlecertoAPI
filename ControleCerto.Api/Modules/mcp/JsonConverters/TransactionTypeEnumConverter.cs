using System.Text.Json;
using System.Text.Json.Serialization;
using ControleCerto.Enums;

namespace ControleCerto.Modules.Mcp.JsonConverters
{
    public class TransactionTypeEnumConverter : JsonConverter<TransactionTypeEnum?>
    {
        public override TransactionTypeEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var stringValue = reader.GetString()?.ToUpperInvariant();
                    return stringValue switch
                    {
                        "EXPENSE" => TransactionTypeEnum.EXPENSE,
                        "INCOME" => TransactionTypeEnum.INCOME,
                        "CREDITEXPENSE" => TransactionTypeEnum.CREDITEXPENSE,
                        "TRANSFERENCE" => TransactionTypeEnum.TRANSFERENCE,
                        "INVOICEPAYMENT" => TransactionTypeEnum.INVOICEPAYMENT,
                        _ => null
                    };

                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out var intValue))
                    {
                        return (TransactionTypeEnum)intValue;
                    }
                    break;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, TransactionTypeEnum? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString().ToUpperInvariant());
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

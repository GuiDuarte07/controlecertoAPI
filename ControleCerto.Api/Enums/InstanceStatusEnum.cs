namespace ControleCerto.Enums
{
    public enum InstanceStatusEnum
    {
        PENDING,    // Aguardando confirmação do usuário
        CONFIRMED,  // Usuário confirmou, Transaction criada
        REJECTED,   // Usuário rejeitou
        SKIPPED,    // Sistema pulou (ex: fim do período)
        ERROR       // Erro na geração da Transaction
    }
}

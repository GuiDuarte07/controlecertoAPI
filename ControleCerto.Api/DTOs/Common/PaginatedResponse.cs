namespace ControleCerto.DTOs.Common
{
    /// <summary>
    /// Resposta genérica paginada para qualquer tipo de dados.
    /// Compatível com paginadores de React/shadcn.
    /// </summary>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Lista de itens da página atual
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Metadados de paginação
        /// </summary>
        public PaginationMetadata Pagination { get; set; } = new();
    }
}

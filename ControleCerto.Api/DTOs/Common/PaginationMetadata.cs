namespace ControleCerto.DTOs.Common
{
    /// <summary>
    /// Metadados de paginação para respostas paginadas.
    /// Compatível com componentes de paginação de UI (React, shadcn, etc).
    /// </summary>
    public class PaginationMetadata
    {
        /// <summary>
        /// Página atual (começa em 1)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Quantidade de itens por página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total de itens disponíveis (sem paginação)
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total de páginas calculado
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indica se há próxima página
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Indica se há página anterior
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        public PaginationMetadata() { }

        public PaginationMetadata(int currentPage, int pageSize, int totalItems)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }
    }
}

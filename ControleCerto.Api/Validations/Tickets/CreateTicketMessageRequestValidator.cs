using ControleCerto.Modules.Tickets.DTOs;
using FluentValidation;

namespace ControleCerto.Validations.Tickets
{
    public class CreateTicketMessageRequestValidator : AbstractValidator<CreateTicketMessageRequest>
    {
        private const int MaxAttachmentsPerRequest = 10;
        private const long MaxAttachmentSizeBytes = 10 * 1024 * 1024;

        public CreateTicketMessageRequestValidator()
        {
            RuleFor(x => x.Body)
                .NotEmpty();

            When(x => x.Attachments is not null && x.Attachments.Length > 0, () =>
            {
                RuleFor(x => x.Attachments)
                    .Must(files => files is not null && files.Length <= MaxAttachmentsPerRequest)
                    .WithMessage("Quantidade máxima de anexos excedida.");

                RuleForEach(x => x.Attachments)
                    .Must(file => file is not null && file.Length > 0 && file.Length <= MaxAttachmentSizeBytes)
                    .WithMessage("Um ou mais anexos excedem o tamanho máximo permitido.");
            });
        }
    }
}


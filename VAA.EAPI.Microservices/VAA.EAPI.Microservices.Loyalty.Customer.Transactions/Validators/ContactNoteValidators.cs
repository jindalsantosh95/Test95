using FluentValidation;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class ContactNoteCreateValidator : AbstractValidator<ContactNote>
    {

        public ContactNoteCreateValidator()
        {
            Include(new ContactNoteContentValidator());
        }
    }

    public class ContactNoteDeleteValidator : AbstractValidator<ContactNote>
    {
        public ContactNoteDeleteValidator()
        {
            Include(new ContactNoteIDValidator());
        }
    }

    public class ContactNotePatchValidator : AbstractValidator<ContactNote>
    {
        public ContactNotePatchValidator()
        {
            Include(new ContactNoteIDValidator());
            Include(new ContactNoteContentValidator());
        }
    }

    public class ContactNoteIDValidator : AbstractValidator<ContactNote>
    {
        public ContactNoteIDValidator()
        {
            RuleFor(x => x.Id.Trim()).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.ContactNoteIdInvalid);
        }

    }
    public class ContactNoteContentValidator : AbstractValidator<ContactNote>
    {
        private readonly string[] notesTypeGroupArr = { "TOP50", "SPEC2", "SPEC1", "GEN", "FOOD", "SEAT", "FWY", "MISC" };
        private readonly string[] noteTypeArr = { "GEN", "INFO", "PREF", "SPEC" };

        public ContactNoteContentValidator()
        {
            RuleFor(x => x.Group.Trim()).NotNull().NotEmpty().Must(item => notesTypeGroupArr.Contains(item)).WithMessage(Constants.ErrorCodes.ContactNoteTypeGroupInvalid);
            RuleFor(x => x.Type.Trim()).NotNull().NotEmpty().Must(item => noteTypeArr.Contains(item)).WithMessage(Constants.ErrorCodes.ContactNoteTypeInvalid);
            RuleFor(x => x.Text.Trim()).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.ContactNoteTextInvalid);
        }
    }


}

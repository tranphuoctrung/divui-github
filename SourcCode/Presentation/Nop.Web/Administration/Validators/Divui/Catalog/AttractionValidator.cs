using FluentValidation;
using Nop.Admin.Models.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Catalog
{
    public class AttractionValidator : BaseNopValidator<AttractionModel>
    {
        public AttractionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attractions.Fields.Name.Required"));
        }
    }
}
using System.ComponentModel.DataAnnotations;
using ModernWPF.Client.Features.Validation;
using PropertyChanged;

namespace ModernWPF.Client.Features.Application.Customer
{
    public class CustomerDetailsViewModel : ValidatableViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string FavouriteFood { get; set; }
    }
}
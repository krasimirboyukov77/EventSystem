using EventSystem.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.EventViewModel
{
    public class EditEventViewModel
    {
        [Required]
        public Guid id { get; set; }

        [Required(ErrorMessage = "This field is required!")]
        [StringLength(
            EventValidationConstants.EventNameMaxLength,
            ErrorMessage = "Event name must be between 2 and 256 characters!",
            MinimumLength = EventValidationConstants.EventNameMinLength)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "This field is required!")]
        [StringLength(
            EventValidationConstants.EventDescriptionMaxLength,
            ErrorMessage = "Event Description must be between 2 and 500 characters!",
            MinimumLength = EventValidationConstants.EventDescriptionMinLength)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "This field is required!")]
        public string Date { get; set; } = null!;

        [Required(ErrorMessage = "This field is required!")]
        [StringLength(
            EventValidationConstants.EventLocationMaxLength
            , ErrorMessage = "Event Location must be between 2 and 256 characters!"
            , MinimumLength = EventValidationConstants.EventLocationMinLength)]
        public string Location { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;

namespace TestContainerDemo.Common
{
    public class CreatePersonRequest
    {
        [Required]
        public string FirstName { get; init; } = string.Empty;

        [Required]
        public string LastName { get; init; } = string.Empty;

        [Required]
        public string Occupation { get; init; } = string.Empty;

        public void Validate()
        {
            var validator = new CreatePersonRequestValidator();
            var result = validator.Validate(this);

            if(!result.IsValid)
            {
                throw new FluentValidation.ValidationException(result.Errors);
            }
        }
    }
}

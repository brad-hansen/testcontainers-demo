using System.ComponentModel.DataAnnotations;

namespace TestContainerDemo.Common
{
    public class PersonResponse
    {
        public int Id { get; init; }

        [Required]
        public string FirstName { get; init; } = string.Empty;

        [Required]
        public string LastName { get; init; } = string.Empty;

        [Required]
        public string Occupation { get; init; } = string.Empty;

    }
}
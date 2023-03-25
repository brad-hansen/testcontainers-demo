namespace TestContainerDemo.API.Models
{
    public sealed class Person
    {
        public int Id { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public string Occupation { get; init; } = string.Empty;
    }
}

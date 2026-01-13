using CSharpFunctionalExtensions;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new(
            @"^[\w\.\-]+@([\w\-]+\.)+[A-Za-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public string Address { get; }
        public string User { get; }
        public string Host { get; }

        private Email(string address, string user, string host)
        {
            Address = address;
            User = user;
            Host = host;
        }

        public static Result<Email> Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Result.Failure<Email>("Email cannot be empty.");

            string normalized = input.Trim().ToLowerInvariant();

            if (!EmailRegex.IsMatch(normalized))
                return Result.Failure<Email>($"Invalid email: {input}");

            var parts = normalized.Split('@');
            if (parts.Length != 2)
                return Result.Failure<Email>($"Invalid email: {input}");

            return Result.Success(new Email(
                address: normalized,
                user: parts[0],
                host: parts[1]
            ));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Address;
        }

        public override string ToString() => Address;
    }
}
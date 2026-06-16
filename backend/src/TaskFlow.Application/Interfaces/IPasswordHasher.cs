namespace TaskFlow.Application.Interfaces;

/// <summary>Hashes and verifies passwords.</summary>
public interface IPasswordHasher
{
    /// <summary>Returns a BCrypt hash of the given plaintext password.</summary>
    string Hash(string password);

    /// <summary>Verifies a plaintext password against a stored hash.</summary>
    bool Verify(string password, string hash);
}

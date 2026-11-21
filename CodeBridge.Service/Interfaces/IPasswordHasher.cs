namespace CodeBridge.Service.Interfaces;

/// <summary>انتزاع هش و بررسی رمز عبور؛ پیاده‌سازی بیرون از لایه‌ی سرویس قرار می‌گیرد.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}

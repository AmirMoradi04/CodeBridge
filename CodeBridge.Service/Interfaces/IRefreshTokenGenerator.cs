namespace CodeBridge.Service.Interfaces;

/// <summary>انتزاع تولید توکن تازه‌سازی و زمان انقضای آن.</summary>
public interface IRefreshTokenGenerator
{
    string Generate();

    DateTime GetExpiryDate();
}

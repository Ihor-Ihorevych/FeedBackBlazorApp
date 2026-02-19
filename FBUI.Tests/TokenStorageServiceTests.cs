using FBUI.Services;

namespace FBUI.Tests;

public sealed class TokenStorageServiceTests
{
    private ITokenStorageService _tokenStorage;

    [SetUp]
    public void SetUp()
    {
        _tokenStorage = new TokenStorageService();
    }

    [TearDown]
    public void TearDown() 
    { 
        _tokenStorage = null!;
    }


    [Test]
    public void NewInstance_HasNullTokens()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_tokenStorage.AccessToken, Is.Null);
            Assert.That(_tokenStorage.RefreshToken, Is.Null);
        }
    }

    [Test]
    public void SetTokens_SetsAccessAndRefreshTokens()
    {
        _tokenStorage.SetTokens("access", "refresh");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_tokenStorage.AccessToken, Is.EqualTo("access"));
            Assert.That(_tokenStorage.RefreshToken, Is.EqualTo("refresh"));
        }
    }

    [Test]
    public void SetTokens_AllowsNullValues()
    {

        _tokenStorage.SetTokens(null, "refresh");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_tokenStorage.AccessToken, Is.Null);
            Assert.That(_tokenStorage.RefreshToken, Is.EqualTo("refresh"));
        }

        _tokenStorage.SetTokens("access", null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_tokenStorage.AccessToken, Is.EqualTo("access"));
            Assert.That(_tokenStorage.RefreshToken, Is.Null);
        }
    }

    [Test]
    public void ClearTokens_SetsBothTokensToNull()
    {
        _tokenStorage.SetTokens("access", "refresh");
        _tokenStorage.ClearTokens();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_tokenStorage.AccessToken, Is.Null);
            Assert.That(_tokenStorage.RefreshToken, Is.Null);
        }
    }
}

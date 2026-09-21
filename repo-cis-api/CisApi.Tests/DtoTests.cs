using CisApi.Business.DTOs;

namespace CisApi.Tests;

public class DtoTests
{
    [Fact]
    public void ErrorResponseDTO_DefaultValues_AreInitialized()
    {
        var dto = new ErrorResponseDTO();

        Assert.Equal(string.Empty, dto.Error);
        Assert.True(dto.Timestamp <= DateTime.UtcNow);
    }
}

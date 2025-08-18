using MediatR;
using ModulebankProject.Features.Accounts;
using ModulebankProject.Features.Accounts.CreateAccount;
using ModulebankProject.Features.Accounts.EditAccount;
using ModulebankProject.MbResult;
using Moq;

namespace ModulebankProject.Tests.ModuleTests;

public class AccountServiceTests
{
    [Fact]
    public async Task CreateAccount_ShouldInitializeWithZeroBalance()
    {
        //Arrange
        var mock = new Mock<IMediator>();
        var command = new CreateAccountCommand
        (
            Guid.NewGuid(),
            AccountType.Checking,
            "rub",
            5,
            DateTime.UtcNow.AddDays(1)
        );

        var expectedAccountDto = new AccountDto
        (
            Guid.NewGuid(),
            Guid.NewGuid(), 
            AccountType.Checking,
            "rub",
            0M,
            5,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1)
        );
        var expectedResult = MbResult<AccountDto, ApiError>.Success(expectedAccountDto);

        mock.Setup(m => m.Send(It.IsAny<CreateAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        //Act
        var result = await mock.Object.Send(command);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAccountDto.Currency, result.Result!.Currency);
        Assert.Equal(expectedAccountDto.Balance, result.Result!.Balance);
        Assert.Equal(expectedAccountDto.InterestRate, result.Result!.InterestRate);
        mock.Verify(m => m.Send(It.IsAny<CreateAccountCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task EditAccountTest()
    {
        //Arrange
        var mock = new Mock<IMediator>();
        var command = new EditAccountCommand
        (
            Guid.NewGuid(),
            "usd",
            5,
            DateTime.UtcNow.AddDays(5)
        );

        var expectedAccountDto = new AccountDto
        (
            Guid.NewGuid(),
            Guid.NewGuid(),
            AccountType.Checking,
            "usd",
            0M,
            5,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(5)
        );
        var expectedResult = MbResult<AccountDto, ApiError>.Success(expectedAccountDto);

        mock.Setup(m => m.Send(It.IsAny<EditAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        //Act
        var result = await mock.Object.Send(command);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAccountDto.Currency, result.Result!.Currency);
        Assert.Equal(expectedAccountDto.Balance, result.Result!.Balance);
        Assert.Equal(expectedAccountDto.CloseDate, result.Result!.CloseDate);
        mock.Verify(m => m.Send(It.IsAny<EditAccountCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
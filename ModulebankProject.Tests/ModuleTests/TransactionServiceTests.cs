using MediatR;
using ModulebankProject.Features.Transactions;
using ModulebankProject.Features.Transactions.RegisterTransaction;
using ModulebankProject.MbResult;
using Moq;

namespace ModulebankProject.Tests.ModuleTests
{
    public class TransactionServiceTests
    {
        [Fact]
        public async Task RegisterTransaction_ShouldInitializeWithZeroBalance()
        {
            //Arrange
            var mock = new Mock<IMediator>();
            var accountId = Guid.NewGuid();
            var command = new RegisterTransactionCommand
            (
                accountId,
                null,
                1000M,
                "rub",
                TransactionType.Debit,
                ""
            );

            var expectedTransactionDto = new TransactionDto
            (
                Guid.NewGuid(),
                accountId,
                Guid.Empty, 
                1000M,
                "rub",
                TransactionType.Debit,
                "",
                DateTime.UtcNow,
                TransactionStatus.Registered
            );
            var expectedResult = MbResult<TransactionDto, ApiError>.Success(expectedTransactionDto);

            mock.Setup(m => m.Send(It.IsAny<RegisterTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            //Act
            var result = await mock.Object.Send(command);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTransactionDto.Currency, result.Result!.Currency);
            Assert.Equal(expectedTransactionDto.Amount, result.Result!.Amount);
            Assert.Equal(expectedTransactionDto.TransactionType, result.Result!.TransactionType);
            mock.Verify(m => m.Send(It.IsAny<RegisterTransactionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

using AutoFixture.Xunit2;

using FluentAssertions;

using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;

using Moq;

using Xunit;

namespace Moneybox.App.Tests.Features
{
    public sealed class TransferMoneyTests
    {
        private Mock<IAccountRepository> mockedAccountRepository;
        private Mock<INotificationService> mockedNotificationService;

        [Theory]
        [AutoData]
        public void GivenFromAccountHasNoBalance_WhenExecutingMoneyTransfer_ExpectInvalidOperationException(
            [Range(int.MinValue, -1)] decimal fromAccountBalance,
            Account fromAccount,
            Account toAccount)
        {
            // Arrange
            fromAccount.Balance = fromAccountBalance;
            const decimal amount = 0;
            var sut = this.SetupSut(fromAccount);

            // Act
            void SutCall() => sut.Execute(fromAccount.Id, toAccount.Id, amount);
            Action sutCall = SutCall;

            // Assert
            sutCall.Should().ThrowExactly<InvalidOperationException>("Account has not balance to transfer from.");
        }

        [Theory]
        [AutoData]
        public void GivenFromAccountHasLowFunds_WhenExecutingMoneyTransfer_ExpectLowFundsNotification(
            [Range(0, 500)] decimal fromAccountBalance,
            Account fromAccount,
            Account toAccount)
        {
            // Arrange
            fromAccount.Balance = fromAccountBalance;
            const decimal amount = 0;
            var sut = this.SetupSut(fromAccount);

            // Act
            sut.Execute(fromAccount.Id, toAccount.Id, amount);

            // Assert
            this.mockedNotificationService.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Exactly(1));
        }

        private TransferMoney SetupSut(Account fromAccount)
        {
            this.mockedAccountRepository = CreateAccountRepositoryMock(fromAccount);
            this.mockedNotificationService = CreateNotificationServiceMock();

            return new TransferMoney(this.mockedAccountRepository.Object, this.mockedNotificationService.Object);
        }

        private static Mock<IAccountRepository> CreateAccountRepositoryMock(Account fromAccount)
        {
            var mockedAccountRepository = new Mock<IAccountRepository>();
            mockedAccountRepository
                .Setup(repository => repository.GetAccountById(It.IsAny<Guid>()))
                .Returns(fromAccount);

            return mockedAccountRepository;
        }

        private static Mock<INotificationService> CreateNotificationServiceMock()
        {
            var mockedNotificationService = new Mock<INotificationService>();
            mockedNotificationService
                .Setup(service => service.NotifyFundsLow(It.IsAny<string>()))
                .Verifiable("A low funds notification is expected.");

            return mockedNotificationService;
        }
    }
}

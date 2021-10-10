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
            var sut = this.SetupSut(fromAccount, toAccount);

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
            var sut = this.SetupSut(fromAccount, toAccount);

            // Act
            sut.Execute(fromAccount.Id, toAccount.Id, amount);

            // Assert
            this.mockedNotificationService.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Exactly(1));
        }

        private TransferMoney SetupSut(
            Account fromAccount,
            Account toAccount)
        {
            this.mockedAccountRepository = CreateAccountRepositoryMock(fromAccount, toAccount);
            this.mockedNotificationService = CreateNotificationServiceMock();

            return new TransferMoney(this.mockedAccountRepository.Object, this.mockedNotificationService.Object);
        }

        private static Mock<IAccountRepository> CreateAccountRepositoryMock(
            Account fromAccount,
            Account toAccount)
        {
            var mockedAccountRepository = new Mock<IAccountRepository>();
            mockedAccountRepository
                .Setup(repository => repository.GetAccountById(fromAccount.Id))
                .Returns(fromAccount);

            mockedAccountRepository
                .Setup(repository => repository.GetAccountById(toAccount.Id))
                .Returns(toAccount);

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

        [Theory]
        [AutoData]
        public void GivenTransferExceedsPayInLimit_WhenExecutingMoneyTransfer_ExpectInvalidOperationException(
            Account fromAccount,
            Account toAccount)
        {
            // Arrange
            const decimal amount = 1;
            fromAccount.Balance = amount;
            toAccount.PaidIn = Account.PayInLimit;
            var sut = this.SetupSut(fromAccount, toAccount);

            // Act
            void SutCall() => sut.Execute(fromAccount.Id, toAccount.Id, amount);
            Action sutCall = SutCall;

            // Assert
            sutCall.Should().ThrowExactly<InvalidOperationException>("Account exceeds the limit of money it can receive.");
        }
    }
}

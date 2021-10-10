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

        [Theory]
        [AutoData]
        public void GivenToAccountExceedsPayInLimit_WhenExecutingMoneyTransfer_ExpectInvalidOperationException(
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

        [Theory]
        [AutoData]
        public void GivenToAccountNearPayInLimit_WhenExecutingMoneyTransfer_ExpectLowFundsNotification(
            [Range((int)Account.PayInLimit - 500, (int)Account.PayInLimit)] decimal toAccountPaidIn,
            Account fromAccount,
            Account toAccount)
        {
            // Arrange
            toAccount.PaidIn = toAccountPaidIn;
            const decimal amount = 0;
            fromAccount.Balance = amount;
            var sut = this.SetupSut(fromAccount, toAccount);

            // Act
            sut.Execute(fromAccount.Id, toAccount.Id, amount);

            // Assert
            this.mockedNotificationService.Verify(service => service.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Exactly(1));
        }

        [Theory]
        [AutoData]
        public void GivenMoneyIsTransferredSuccessfully_WhenExecutingMoneyTransfer_ExpectAmountTransferred(
            Account fromAccount,
            Account toAccount,
            [Range(1, (int)Account.PayInLimit - 500)]decimal amount)
        {
            // Arrange
            fromAccount.Balance = amount;
            var expectedFromBalance = fromAccount.Balance - amount;
            var expectedFromWithdrawn = fromAccount.Withdrawn - amount;
            var expectedToBalance = toAccount.Balance + amount;
            var expectedToPaidIn = toAccount.PaidIn + amount;
            var sut = this.SetupSut(fromAccount, toAccount);

            // Act
            sut.Execute(fromAccount.Id, toAccount.Id, amount);

            // Assert
            fromAccount.Balance.Should().Be(expectedFromBalance, "The amount is expected to be removed from the origin account balance");
            fromAccount.Withdrawn.Should().Be(expectedFromWithdrawn, "The amount is expected to be removed from the origin account withdrawn value");
            toAccount.Balance.Should().Be(expectedToBalance, "The amount is expected to be added to the destination account balance");
            toAccount.PaidIn.Should().Be(expectedToPaidIn, "The amount is expected to be added tp the destination account paid in value");
        }

        [Theory]
        [AutoData]
        public void GivenFromAccountNotfound_WhenExecutingMoneyTransfer_ExpectArgumentException(
            Guid fromAccountId,
            Account toAccount)
        {
            // Arrange
            const decimal amount = 0;
            var sut = this.SetupSut(default, toAccount);
            // Act
            void SutCall() => sut.Execute(fromAccountId, toAccount.Id, amount);
            Action sutCall = SutCall;

            // Assert
            sutCall.Should().ThrowExactly<ArgumentException>("No account to transfer funds from was found.");
        }

        [Theory]
        [AutoData]
        public void GivenTransferIsSuccessful_WhenExecutingMoneyTransfer_ExpectValuesArePersisted(
            Account fromAccount,
            Account toAccount)
        {
            // Arrange
            const decimal amount = 0;
            fromAccount.Balance = amount;
            var sut = this.SetupSut(fromAccount, toAccount);

            // Act
            sut.Execute(fromAccount.Id, toAccount.Id, amount);

            // Assert
            this.mockedAccountRepository.Verify(repository => repository.Update(fromAccount), Times.Exactly(1));
            this.mockedAccountRepository.Verify(repository => repository.Update(toAccount), Times.Exactly(1));
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
                .Setup(repository => repository.Update(fromAccount))
                .Verifiable("Persistence is expected when operation is successful");

            mockedAccountRepository
                .Setup(repository => repository.GetAccountById(toAccount.Id))
                .Returns(toAccount);

            mockedAccountRepository
                .Setup(repository => repository.Update(toAccount))
                .Verifiable("Persistence is expected when operation is successful");

            return mockedAccountRepository;
        }

        private static Mock<INotificationService> CreateNotificationServiceMock()
        {
            var mockedNotificationService = new Mock<INotificationService>();
            mockedNotificationService
                .Setup(service => service.NotifyFundsLow(It.IsAny<string>()))
                .Verifiable("A low funds notification is expected.");

            mockedNotificationService
                .Setup(service => service.NotifyApproachingPayInLimit(It.IsAny<string>()))
                .Verifiable("A pay in limit notification is expected");

            return mockedNotificationService;
        }
    }
}

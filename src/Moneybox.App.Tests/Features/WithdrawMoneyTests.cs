using System;
using System.ComponentModel.DataAnnotations;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;

using Moq;

using Xunit;

namespace Moneybox.App.Tests.Features
{
    public sealed class WithdrawMoneyTests
    {
        private Mock<IAccountRepository> mockedAccountRepository;
        private Mock<INotificationService> mockedNotificationService;

        [Theory]
        [AutoData]
        public void GivenFromAccountHasNoBalance_WhenExecutingMoneyWithdraw_ExpectInvalidOperationException(
            [Range(int.MinValue, -1)] decimal fromAccountBalance,
            Account fromAccount,
            Account toAccount)
        {
            // Arrange
            fromAccount.Balance = fromAccountBalance;
            const decimal amount = 0;
            var sut = this.SetupSut(fromAccount, toAccount);

            // Act
            void SutCall() => sut.Execute(fromAccount.Id, amount);
            Action sutCall = SutCall;

            // Assert
            sutCall.Should().ThrowExactly<InvalidOperationException>("Account has not balance to withdraw from.");
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
            sut.Execute(fromAccount.Id, amount);

            // Assert
            this.mockedNotificationService.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Exactly(1));
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
            var sut = this.SetupSut(fromAccount, toAccount);

            // Act
            sut.Execute(fromAccount.Id, amount);

            // Assert
            fromAccount.Balance.Should().Be(expectedFromBalance, "The amount is expected to be removed from the origin account balance");
            fromAccount.Withdrawn.Should().Be(expectedFromWithdrawn, "The amount is expected to be removed from the origin account withdrawn value");
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
            void SutCall() => sut.Execute(fromAccountId, amount);
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
            sut.Execute(fromAccount.Id, amount);

            // Assert
            this.mockedAccountRepository.Verify(repository => repository.Update(fromAccount), Times.Exactly(1));
        }

        private WithdrawMoney SetupSut(
            Account fromAccount,
            Account toAccount)
        {
            this.mockedAccountRepository = CreateAccountRepositoryMock(fromAccount, toAccount);
            this.mockedNotificationService = CreateNotificationServiceMock();

            return new WithdrawMoney(this.mockedAccountRepository.Object, this.mockedNotificationService.Object);
        }

        private static Mock<IAccountRepository> CreateAccountRepositoryMock(
            Account fromAccount,
            Account toAccount)
        {
            var mockedAccountRepository = new Mock<IAccountRepository>();
            SetupMockedAccountRepository(mockedAccountRepository, fromAccount);
            SetupMockedAccountRepository(mockedAccountRepository, toAccount);

            return mockedAccountRepository;
        }

        private static void SetupMockedAccountRepository(
            Mock<IAccountRepository> mockedAccountRepository,
            Account? account)
        {
            var accountId = account?.Id ?? new Fixture().Create<Guid>();

            mockedAccountRepository
                .Setup(repository => repository.GetAccountById(accountId))
                .Returns(account);

            mockedAccountRepository
                .Setup(repository => repository.Update(account))
                .Verifiable("Persistence is expected when operation is successful");
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

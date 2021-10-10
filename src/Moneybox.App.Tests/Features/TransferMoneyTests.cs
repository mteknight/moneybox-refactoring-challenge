using System;

using FluentAssertions;

using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moneybox.App.Tests.Features.TestData;

using Moq;

using Xunit;

namespace Moneybox.App.Tests.Features
{
    public sealed class TransferMoneyTests
    {
        [Theory]
        [MemberData(nameof(TransferMoneyTestData.NoBalanceAccountCase), MemberType= typeof(TransferMoneyTestData))]
        public void GivenFromAccountHasNoBalance_WhenExecutingMoneyTransfer_ExpectInvalidOperationException(
            decimal fromAccountBalance,
            Guid fromAccountId,
            Guid toAccountId,
            decimal amount)
        {
            // Arrange
            var fromAccount = new Account { Balance = fromAccountBalance };

            var mockedAccountRepository = new Mock<IAccountRepository>();
            mockedAccountRepository
                .Setup(repository => repository.GetAccountById(It.IsAny<Guid>()))
                .Returns(fromAccount);

            var mockedNotificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(mockedAccountRepository.Object, mockedNotificationService.Object);

            // Act
            void SutCall() => sut.Execute(fromAccountId, toAccountId, amount);
            Action sutCall = SutCall;

            // Assert
            sutCall.Should().ThrowExactly<InvalidOperationException>("Account has not balance to transfer from.");

        }
    }
}

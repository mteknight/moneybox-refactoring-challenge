using System;

using Dawn;

using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private readonly IAccountRepository accountRepository;
        private readonly INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = Guard.Argument(accountRepository, nameof(accountRepository)).NotNull().Value;
            this.notificationService = Guard.Argument(notificationService, nameof(notificationService)).NotNull().Value;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var fromAccount = this.accountRepository.GetAccountById(fromAccountId);
            if (fromAccount is null)
            {
                throw new ArgumentException($"No account was found for id '{fromAccount}'");
            }

            var toAccount = this.accountRepository.GetAccountById(toAccountId);
            if (toAccount is null)
            {
                throw new ArgumentException($"No account was found for id '{toAccount}'");
            }

            fromAccount.TransferMoney(toAccount, amount, this.notificationService);

            this.accountRepository.Update(fromAccount);
            this.accountRepository.Update(toAccount);
        }
    }
}

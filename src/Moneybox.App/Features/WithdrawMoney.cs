using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

using Dawn;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private readonly IAccountRepository accountRepository;
        private readonly INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = Guard.Argument(accountRepository, nameof(accountRepository)).NotNull().Value;
            this.notificationService = Guard.Argument(notificationService, nameof(notificationService)).NotNull().Value;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var fromAccount = this.accountRepository.GetAccountById(fromAccountId);
            if (fromAccount is null)
            {
                throw new ArgumentException($"No account was found for id '{fromAccount}'");
            }

            fromAccount.WithdrawMoney(amount, this.notificationService);

            this.accountRepository.Update(fromAccount);
        }
    }
}

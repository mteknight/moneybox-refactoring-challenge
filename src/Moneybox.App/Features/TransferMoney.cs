using System;

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
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var fromAccount = this.accountRepository.GetAccountById(fromAccountId);
            var toAccount = this.accountRepository.GetAccountById(toAccountId);

            fromAccount.TransferMoney(toAccount, amount, this.notificationService);

            this.accountRepository.Update(fromAccount);
            this.accountRepository.Update(toAccount);
        }
    }
}

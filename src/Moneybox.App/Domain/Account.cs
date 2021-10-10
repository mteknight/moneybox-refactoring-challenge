using System;

using Moneybox.App.Domain.Services;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public bool HasEnoughFunds(
            decimal amount,
            INotificationService notificationService)
        {
            var fromBalance = this.Balance - amount;
            if (fromBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            if (fromBalance < 500m)
            {
                notificationService.NotifyFundsLow(this.User.Email);
            }

            return true;
        }

        public bool CanReceiveMoney(
            decimal amount,
            INotificationService notificationService)
        {
            var paidIn = this.PaidIn + amount;
            if (paidIn > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            if (PayInLimit - paidIn < 500m)
            {
                notificationService.NotifyApproachingPayInLimit(this.User.Email);
            }

            return true;
        }

        public void TransferMoney(
            Account toAccount,
            decimal amount,
            INotificationService notificationService)
        {
            this.HasEnoughFunds(amount, notificationService);
            toAccount.CanReceiveMoney(amount, notificationService);

            this.Balance -= amount;
            this.Withdrawn -= amount;

            toAccount.ReceiveMoney(amount);
        }

        private void ReceiveMoney(decimal amount)
        {
            this.Balance += amount;
            this.PaidIn += amount;
        }

        public void WithdrawMoney(
            decimal amount,
            INotificationService notificationService)
        {
            this.HasEnoughFunds(amount, notificationService);

            this.Balance -= amount;
            this.Withdrawn -= amount;
        }
    }
}

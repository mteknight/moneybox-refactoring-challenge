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

        public bool CanTransferMoney(
            decimal amount,
            INotificationService notificationService)
        {
            var fromBalance = this.Balance - amount;
            if (fromBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
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
    }
}

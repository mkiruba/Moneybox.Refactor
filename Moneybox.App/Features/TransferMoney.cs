using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

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
            var from = this.accountRepository.GetAccountById(fromAccountId);
            var to = this.accountRepository.GetAccountById(toAccountId);

            from.CheckInsufficientFunds(amount);
            if (from.CheckLowFunds(amount))
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }

            to.CheckPayInLimitReached(amount);
            if (to.CheckPayInLimit(amount))
            {
                this.notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }

            from.WithDraw(amount);
            to.PayIn(amount);

            this.accountRepository.Update(from);
            this.accountRepository.Update(to);
        }
    }
}
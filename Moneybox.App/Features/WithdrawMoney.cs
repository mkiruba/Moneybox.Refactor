using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private readonly IAccountRepository accountRepository;
        private readonly INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);

            from.CheckInsufficientFunds(amount);
           
            if (from.CheckLowFunds(amount))
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }
            
            from.WithDraw(amount);

            this.accountRepository.Update(from);
        }
    }
}
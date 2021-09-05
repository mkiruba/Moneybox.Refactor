using System;

namespace Moneybox.App
{
    public class Account
    {
        private const decimal PayInLimit = 4000m;
        
        private const decimal LowFundLimit = 500m;
        
        private const decimal NoBalanceLimit = 0m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }
        
        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public void CheckInsufficientFunds(decimal amount)
        {
            if (Balance - amount < NoBalanceLimit)
            {
                throw new InvalidOperationException("Insufficient funds");
            }
        }
        
        public void CheckPayInLimitReached(decimal amount)
        {
            if (PaidIn + amount > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }
        }
        
        public bool CheckPayInLimit(decimal amount)
        {
            return PayInLimit - (PaidIn + amount) < LowFundLimit;
        }
        
        public bool CheckLowFunds(decimal amount)
        {
            return Balance - amount < LowFundLimit;
        }
        
        public void WithDraw(decimal amount)
        {
            Balance -= amount;
            Withdrawn -= amount;
        }
        
        public void PayIn(decimal amount)
        {
            Balance += amount;
            PaidIn += amount;
        }
    }
}
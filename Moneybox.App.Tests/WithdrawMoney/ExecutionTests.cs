using System;
using AutoFixture;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moq;
using Xunit;

namespace Moneybox.App.Tests.WithdrawMoney
{
    public class ExecutionTests : BaseFixture
    {
        [Theory]
        [InlineData(2000, 100)]
        [InlineData(2000, 500)]
        public void Should_WithdrawMoney_Success(decimal fromBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            
            var fromGuid = Fixture.Create<Guid>();
            var fromAccount = CreateTestAccount(fromBalance, mockAccountRepository, fromGuid);

            var withdrawMoney = new Features.WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);
            
            //Act
            withdrawMoney.Execute(fromGuid, amount);
            
            //Assert
            Assert.Equal(fromAccount.Withdrawn, -amount);
            Assert.Equal(fromAccount.Balance, fromBalance - amount);
            mockNotificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Never);
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Once);
        }
        
        [Theory]
        [InlineData(500, 1000, 100)]
        public void Should_TransferMoney_Success_With_FundsLow_Notification(decimal fromBalance, decimal toBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            
            var fromGuid = Fixture.Create<Guid>();
            var fromAccount = CreateTestAccount(fromBalance, mockAccountRepository, fromGuid);
        
            var withdrawMoney = new Features.WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);
            
            //Act
            withdrawMoney.Execute(fromGuid, amount);
            
            //Assert
            Assert.Equal(fromAccount.Withdrawn, -amount);
            Assert.Equal(fromAccount.Balance, fromBalance - amount);
            mockNotificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Once);
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Once);
        }
        
        [Theory]
        [InlineData(100, 1000, 500)]
        public void Should_Fail_TransferMoney_When_InsufficientFund(decimal fromBalance, decimal toBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            
            var fromGuid = Fixture.Create<Guid>();
            var fromAccount = CreateTestAccount(fromBalance, mockAccountRepository, fromGuid);
        
            var withdrawMoney = new Features.WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);

            //Act
            var exception = Record.Exception(() => withdrawMoney.Execute(fromGuid, amount));
            
            //Assert
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("Insufficient funds", exception.Message);
        }
        
        private Account CreateTestAccount(decimal fromBalance,
            Mock<IAccountRepository> mockAccountRepository,
            Guid fromGuid)
        {
            var fromAccount = Fixture.Build<Account>()
                .With(x => x.Id, fromGuid)
                .With(x => x.Balance, fromBalance)
                .With(x => x.Withdrawn, 0)
                .With(x => x.PaidIn, 0)
                .Create();
           
            mockAccountRepository.Setup(x => x.GetAccountById(fromGuid))
                .Returns(fromAccount);
           
            return fromAccount;
        }
    }
}
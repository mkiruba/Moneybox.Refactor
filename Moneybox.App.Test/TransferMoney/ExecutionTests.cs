using System;
using AutoFixture;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moq;
using Xunit;

namespace Moneybox.App.Test.TransferMoney
{
    public class ExecutionTests : BaseFixture
    {
        [Theory]
        [InlineData(2000, 1000, 100)]
        [InlineData(2000, 0, 100)]
        public void Should_TransferMoney_Success(decimal fromBalance, decimal toBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            
            var fromGuid = Fixture.Create<Guid>();
            var toGuid = Fixture.Create<Guid>();
            var (fromAccount, toAccount) = CreateTestAccounts(fromBalance, toBalance, mockAccountRepository, fromGuid, toGuid);

            var transferMoney = new Features.TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            
            //Act
            transferMoney.Execute(fromGuid, toGuid, amount);
            
            //Assert
            Assert.Equal(fromAccount.Withdrawn, -amount);
            Assert.Equal(toAccount.PaidIn, amount);
            Assert.Equal(fromAccount.Balance, fromBalance - amount);
            Assert.Equal(toAccount.Balance, toBalance + amount);
            mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(toAccount.User.Email), Times.Never);
            mockNotificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Never);
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        }
        
        [Theory]
        [InlineData(5000, 1000, 3600)]
        public void Should_TransferMoney_Success_With_PayInLimit_Notification(decimal fromBalance, decimal toBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();

            var fromGuid = Fixture.Create<Guid>();
            var toGuid = Fixture.Create<Guid>();
            var (fromAccount, toAccount) = CreateTestAccounts(fromBalance, toBalance, mockAccountRepository, fromGuid, toGuid);

            var transferMoney = new Features.TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            
            //Act
            transferMoney.Execute(fromGuid, toGuid, amount);
            
            //Assert
            Assert.Equal(fromAccount.Withdrawn, -amount);
            Assert.Equal(toAccount.PaidIn, amount);
            Assert.Equal(fromAccount.Balance, fromBalance - amount);
            Assert.Equal(toAccount.Balance, toBalance + amount);
            mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(toAccount.User.Email), Times.Once);
            mockNotificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Never);
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        }
        
        [Theory]
        [InlineData(500, 1000, 100)]
        public void Should_TransferMoney_Success_With_FundsLow_Notification(decimal fromBalance, decimal toBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            
            var fromGuid = Fixture.Create<Guid>();
            var toGuid = Fixture.Create<Guid>();
            var (fromAccount, toAccount) = CreateTestAccounts(fromBalance, toBalance, mockAccountRepository, fromGuid, toGuid);

            var transferMoney = new Features.TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            
            //Act
            transferMoney.Execute(fromGuid, toGuid, amount);
            
            //Assert
            Assert.Equal(fromAccount.Withdrawn, -amount);
            Assert.Equal(toAccount.PaidIn, amount);
            Assert.Equal(fromAccount.Balance, fromBalance - amount);
            Assert.Equal(toAccount.Balance, toBalance + amount);
            mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(toAccount.User.Email), Times.Never);
            mockNotificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Once);
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        }

        [Theory]
        [InlineData(100, 1000, 500)]
        public void Should_Fail_TransferMoney_When_InsufficientFund(decimal fromBalance, decimal toBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            
            var fromGuid = Fixture.Create<Guid>();
            var toGuid = Fixture.Create<Guid>();
            var (fromAccount, toAccount) = CreateTestAccounts(fromBalance, toBalance, mockAccountRepository, fromGuid, toGuid);
            
            var transferMoney = new Features.TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            
            //Act
            var exception = Record.Exception(() => transferMoney.Execute(fromGuid, toGuid, amount));
            
            //Assert
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("Insufficient funds", exception.Message);
        }
        
        [Theory]
        [InlineData(5000, 1000, 4001)]
        public void Should_Fail_TransferMoney_When_ReachedPayInLimit(decimal fromBalance, decimal toBalance, decimal amount)
        {
            //Arrange
            var mockAccountRepository = new Mock<IAccountRepository>();
            var mockNotificationService = new Mock<INotificationService>();
            
            var fromGuid = Fixture.Create<Guid>();
            var toGuid = Fixture.Create<Guid>();
            var (fromAccount, toAccount) = CreateTestAccounts(fromBalance, toBalance, mockAccountRepository, fromGuid, toGuid);
            
            var transferMoney = new Features.TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            
            //Act
            var exception = Record.Exception(() => transferMoney.Execute(fromGuid, toGuid, amount));
            
            //Assert
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("Account pay in limit reached", exception.Message);
        }
        
        private Tuple<Account, Account> CreateTestAccounts(decimal fromBalance, decimal toBalance, 
            Mock<IAccountRepository> mockAccountRepository,
            Guid fromGuid,  Guid toGuid)
        {
            
            var fromAccount = Fixture.Build<Account>()
                .With(x => x.Id, fromGuid)
                .With(x => x.Balance, fromBalance)
                .With(x => x.Withdrawn, 0)
                .With(x => x.PaidIn, 0)
                .Create();
            var toAccount = Fixture.Build<Account>()
                .With(x => x.Id, toGuid)
                .With(x => x.Balance, toBalance)
                .With(x => x.Withdrawn, 0)
                .With(x => x.PaidIn, 0)
                .Create();
            mockAccountRepository.Setup(x => x.GetAccountById(fromGuid))
                .Returns(fromAccount);
            mockAccountRepository.Setup(x => x.GetAccountById(toGuid))
                .Returns(toAccount);
            return new Tuple<Account, Account>(fromAccount, toAccount);
        }

    }
}
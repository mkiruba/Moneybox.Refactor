using System;
using Xunit;

namespace Moneybox.App.Test
{
    public class ExecutionTests
    {
        [Fact]
        public void Should_TransferMoney()
        {
            //Arrange
            var mockPilotBookingCommandHandler = new Mock<IPilotBookingCommandHandler>();
            var mockPilotBookingRepository = new Mock<IPilotBookingRepository>();
            mockPilotBookingRepository.Setup(x => x.GetPilotBookingById(id))
                .ReturnsAsync(pilotBooking);
            
            var transferMoney = new TransferMoney(mockPilotBookingRepository.Object, mockPilotBookingCommandHandler.Object);
            
            //Act
            
            //Assert
            
        }
    }
}
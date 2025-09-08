using HotelBooking.Core;
using Moq;
using Xunit;

public class BookingManagerTests
{
        private readonly Mock<IRepository<Booking>> bookingRepoMock;
        private readonly Mock<IRepository<Room>> roomRepoMock;
        private readonly BookingManager bookingManager;

        public BookingManagerTests()
        {
            bookingRepoMock = new Mock<IRepository<Booking>>();
            roomRepoMock = new Mock<IRepository<Room>>();
            bookingManager = new BookingManager(bookingRepoMock.Object, roomRepoMock.Object);
        }

        // -------------------------------
        // CreateBooking tests
        // -------------------------------
        [Fact]
        public async void CreateBooking_ShouldWork_WhenRoomFree()
        {
            var rooms = new List<Room>
            {
                new Room { Id = 1 }
            };

            var bookings = new List<Booking>();

            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);

            var booking = new Booking
            {
                CustomerId = 1,
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(4)
            };
            var result = await bookingManager.CreateBooking(booking);
            Assert.True(result);
            Assert.True(booking.IsActive);
            bookingRepoMock.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
            Assert.Equal(1, booking.RoomId);
        }

        [Fact]
        public async void CreateBooking_ShouldFail_WhenNoRoomsFree()
        {
            var rooms = new List<Room>
            {
                new Room { Id = 1 }
            };

            var bookings = new List<Booking>
            {
                new Booking
                {
                    RoomId = 1,
                    IsActive = true,
                    StartDate = DateTime.Today.AddDays(5),
                    EndDate = DateTime.Today.AddDays(10)
                }
            };
            var booking = new Booking
            {
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(7)
            };
            
            roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
            
            var result = await bookingManager.CreateBooking(booking);
            
            Assert.False(result);
            Assert.False(booking.IsActive);
        }

        [Fact]
        public async void CreateBooking_ShouldHandleInvalidBooking()
        {
            throw new NotImplementedException();
        }

        // -------------------------------
        // FindAvailableRoom tests
        // -------------------------------
        [Fact]
        public async void FindAvailableRoom_ShouldReturnRoomId_WhenRoomFree()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async void FindAvailableRoom_ShouldReturnMinusOne_WhenAllRoomsBooked()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async void FindAvailableRoom_ShouldThrow_WhenDatesInvalid()
        {
            throw new NotImplementedException();
        }

        // -------------------------------
        // Data-driven test example
        // -------------------------------
        [Theory]
        [InlineData(1, 3)]
        [InlineData(2, 5)]
        [InlineData(5, 5)]
        public async void FindAvailableRoom_DataDriven_Test(int startOffset, int endOffset)
        {
            throw new NotImplementedException();
        }

        // -------------------------------
        // GetFullyOccupiedDates tests
        // -------------------------------
        [Fact]
        public async void GetFullyOccupiedDates_ShouldBeEmpty_WhenNoBookings()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async void GetFullyOccupiedDates_ShouldReturnDates_WhenAllRoomsFull()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async void GetFullyOccupiedDates_ShouldBeEmpty_WhenSomeRoomsFree()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async void GetFullyOccupiedDates_ShouldThrow_WhenStartAfterEnd()
        {
            throw new NotImplementedException();
        }
    }
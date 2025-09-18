using HotelBooking.Core;
using Moq;

public class BookingManagerTests
{
    private readonly BookingManager bookingManager;
    private readonly Mock<IRepository<Booking>> bookingRepoMock;
    private readonly Mock<IRepository<Room>> roomRepoMock;

    public BookingManagerTests()
    {
        bookingRepoMock = new Mock<IRepository<Booking>>();
        roomRepoMock = new Mock<IRepository<Room>>();
        bookingManager = new BookingManager(bookingRepoMock.Object, roomRepoMock.Object);
    }

    
    ////////////////////////////////////////////////////////////////////////////////
    //                         CreateBooking Tests
    ////////////////////////////////////////////////////////////////////////////////
    [Fact]
    public async Task CreateBooking_ShouldWork_WhenRoomFree()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1 }
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
    public async Task CreateBooking_ShouldFail_WhenNoRoomsFree()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1 }
        };

        var bookings = new List<Booking>
        {
            new()
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
    public async Task CreateBooking_ShouldHandleInvalidBooking()
    {
        var booking = new Booking
        {
            CustomerId = 1,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today.AddDays(1)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => bookingManager.CreateBooking(booking));
        bookingRepoMock.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    
    ////////////////////////////////////////////////////////////////////////////////
    //                         FindAvailableRoom Tests
    ////////////////////////////////////////////////////////////////////////////////
    [Fact]
    public async Task FindAvailableRoom_ShouldReturnRoomId_WhenRoomFree()
    {
        var rooms = new List<Room>
        {
            new Room { Id = 1 },
            new Room { Id = 2 }
        };

        var bookings = new List<Booking>
        {
            new Booking
            {
                RoomId = 1,
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(4),
                IsActive = true
            }
        };
        roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        
        var startDate = DateTime.Today.AddDays(2);
        var endDate = DateTime.Today.AddDays(4);
        
        var result = await bookingManager.FindAvailableRoom(startDate, endDate);
        
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task FindAvailableRoom_ShouldReturnMinusOne_WhenAllRoomsBooked()
    {        
        var rooms = new List<Room>
        {
            new Room { Id = 1 },
            new Room { Id = 2 }
        };

        var bookings = new List<Booking>
        {
            new Booking
            {
                RoomId = 1,
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(4),
                IsActive = true
            },
            new Booking
            {
                RoomId = 2,
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(4),
                IsActive = true
            }
        };
        roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        
        var startDate = DateTime.Today.AddDays(2);
        var endDate = DateTime.Today.AddDays(4);
        
        var result = await bookingManager.FindAvailableRoom(startDate, endDate);
        
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task FindAvailableRoom_ShouldThrow_WhenDatesInvalid()
    {
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today;
        
        await Assert.ThrowsAsync<ArgumentException>(() => bookingManager.FindAvailableRoom(startDate, endDate));
    }

    
    ////////////////////////////////////////////////////////////////////////////////
    //                         Data-Driven Test
    ////////////////////////////////////////////////////////////////////////////////
    [Theory]
    [InlineData(1, 3)]
    [InlineData(2, 5)]
    [InlineData(5, 5)]
    public async Task FindAvailableRoom_DataDriven_Test(int startOffset, int endOffset)
    {
        var rooms = new List<Room> { new Room{ Id = 1 }, new Room { Id = 2 } };
        var bookings = new List<Booking>();
        
        roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        
        var startDate = DateTime.Today.AddDays(startOffset);
        var endDate = DateTime.Today.AddDays(endOffset);
        
        var roomId = await bookingManager.FindAvailableRoom(startDate, endDate);
        
        Assert.True(roomId > 0);
    }
    

    ////////////////////////////////////////////////////////////////////////////////
    //                         GetFullyOccupiedDates Tests
    ////////////////////////////////////////////////////////////////////////////////
    [Fact]
    public async Task GetFullyOccupiedDates_ShouldBeEmpty_WhenNoBookings()
    {
        var rooms = new List<Room>{ new Room { Id = 1 }, new Room { Id = 2 } };
        roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());
        
        var dates = await bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today.AddDays(2));
        
        Assert.Empty(dates);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_ShouldReturnDates_WhenAllRoomsFull()
    {
        var rooms = new List<Room>{ new Room { Id = 1 }, new Room { Id = 2 } };
        var bookings = new List<Booking>
        {
            new Booking
            {
                RoomId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true
            },
            new Booking
            {
                RoomId = 2, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true
            }
        };
        
        roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        
        var dates = await bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today.AddDays(2));
        
        Assert.Equal(3, dates.Count);
        Assert.Contains(DateTime.Today, dates);
        Assert.Contains(DateTime.Today.AddDays(1), dates);
        Assert.Contains(DateTime.Today.AddDays(2), dates);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_ShouldBeEmpty_WhenSomeRoomsFree()
    {
        var rooms = new List<Room>
        {
            new Room { Id = 1 }, 
            new Room { Id = 2 }
        };

        var bookings = new List<Booking>
        {
            new Booking
            {
                RoomId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2),
                IsActive = true
            }
        };
        roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        bookingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        
        var result = await bookingManager.GetFullyOccupiedDates(DateTime.Today, DateTime.Today.AddDays(2));
        
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_ShouldThrow_WhenStartAfterEnd()
    {
        var startDate = DateTime.Today.AddDays(5);
        var endDate = DateTime.Today.AddDays(3);
        
        await Assert.ThrowsAsync<ArgumentException>(() => bookingManager.GetFullyOccupiedDates(startDate, endDate));
    }
}
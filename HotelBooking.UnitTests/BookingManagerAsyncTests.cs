using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class BookingManagerAsyncTests
{
    private readonly IBookingManager _bookingManager;
    private readonly Mock<IRepository<Booking>> _bookingRepoMock;
    private readonly DateTime _occupiedEnd;
    private readonly DateTime _occupiedStart;
    private readonly Mock<IRepository<Room>> _roomRepoMock;
    private readonly DateTime _today;

    public BookingManagerAsyncTests()
    {
        _roomRepoMock = new Mock<IRepository<Room>>();
        _bookingRepoMock = new Mock<IRepository<Booking>>();
        _bookingManager = new BookingManager(_bookingRepoMock.Object, _roomRepoMock.Object);

        _today = DateTime.Today;
        _occupiedStart = _today.AddDays(10);
        _occupiedEnd = _today.AddDays(20);
    }

    // Invalid Class I1: StartDate <= Today
    [Fact]
    public async Task CreateBookingAsync_StartDateBeforeToday_ThrowsArgumentException()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _today.AddDays(-1),
            EndDate = _today.AddDays(5),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    [Fact]
    public async Task CreateBookingAsync_StartDateEqualToToday_ThrowsArgumentException()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _today,
            EndDate = _today.AddDays(5),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    // Invalid Class I2: StartDate > EndDate
    [Fact]
    public async Task CreateBookingAsync_StartDateAfterEndDate_ThrowsArgumentException()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _today.AddDays(10),
            EndDate = _today.AddDays(5),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    [Fact]
    public async Task CreateBookingAsync_StartDateEqualToEndDate_ReturnsTrue()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _today.AddDays(5),
            EndDate = _today.AddDays(5),
            CustomerId = 1
        };
        await SetupAvailableRoomAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.True(result);
    }

    // Valid Class V1: StartDate > Today AND StartDate <= EndDate AND no overlap
    [Fact]
    public async Task CreateBookingAsync_ValidDatesNoOverlap_ReturnsTrue()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _today.AddDays(1),
            EndDate = _today.AddDays(3),
            CustomerId = 1
        };
        await SetupAvailableRoomAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.True(result);
    }

    // Invalid Class I3: Date range overlaps with existing booking
    [Fact]
    public async Task CreateBookingAsync_OverlapsExistingBooking_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(1),
            EndDate = _occupiedStart.AddDays(3),
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateBookingAsync_StartDateTodayPlusOne_EndDateTodayPlusOne_ReturnsTrue()
    {
        // Arrange - Boundary: Minimum valid start date
        var booking = new Booking
        {
            StartDate = _today.AddDays(1),
            EndDate = _today.AddDays(1),
            CustomerId = 1
        };
        await SetupAvailableRoomAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateBookingAsync_StartDateToday_EndDateTodayPlusOne_ThrowsArgumentException()
    {
        // Arrange - Boundary: Today (invalid)
        var booking = new Booking
        {
            StartDate = _today,
            EndDate = _today.AddDays(1),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    [Fact]
    public async Task CreateBookingAsync_StartDateTodayMinusOne_EndDateTodayPlusOne_ThrowsArgumentException()
    {
        // Arrange - Boundary: Today - 1 (invalid)
        var booking = new Booking
        {
            StartDate = _today.AddDays(-1),
            EndDate = _today.AddDays(1),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    [Fact]
    public async Task CreateBookingAsync_StartDateTodayPlusTwo_EndDateTodayPlusOne_ThrowsArgumentException()
    {
        // Arrange - Boundary: StartDate > EndDate
        var booking = new Booking
        {
            StartDate = _today.AddDays(2),
            EndDate = _today.AddDays(1),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }
    
    // Test Case 1: SD in B, ED in B → Booking Created
    [Fact]
    public async Task CreateBookingAsync_SDInB_EDInB_ReturnsTrue()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(-5),
            EndDate = _occupiedStart.AddDays(-1),
            CustomerId = 1
        };
        await SetupAvailableRoomAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.True(result);
    }

    // Test Case 2: SD in B, ED in O → Booking Denied
    [Fact]
    public async Task CreateBookingAsync_SDInB_EDInO_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(-2),
            EndDate = _occupiedStart.AddDays(5),
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    // Test Case 3: SD in O, ED in O → Booking Denied
    [Fact]
    public async Task CreateBookingAsync_SDInO_EDInO_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(2),
            EndDate = _occupiedStart.AddDays(8),
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    // Test Case 4: SD in B (day before), ED in O (first day) → Booking Denied
    [Fact]
    public async Task CreateBookingAsync_SDInB_EDFirstDayOfO_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(-1),
            EndDate = _occupiedStart,
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    // Test Case 5: SD in B (day before), ED in O (last day) → Booking Denied
    [Fact]
    public async Task CreateBookingAsync_SDInB_EDLastDayOfO_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(-1),
            EndDate = _occupiedEnd,
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    // Test Case 6: SD in O, ED in B → Throws ArgumentException (invalid date range)
    [Fact]
    public async Task CreateBookingAsync_SDInO_EDInB_ThrowsArgumentException()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(1),
            EndDate = _occupiedStart.AddDays(-1),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    // Test Case 7: SD in O, ED in A → Booking Denied (overlaps occupied)
    [Fact]
    public async Task CreateBookingAsync_SDInO_EDInA_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedEnd.AddDays(-1),
            EndDate = _occupiedEnd.AddDays(3),
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    // Test Case 8: SD in A, ED in B → Throws ArgumentException (invalid date range)
    [Fact]
    public async Task CreateBookingAsync_SDInA_EDInB_ThrowsArgumentException()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedEnd.AddDays(5),
            EndDate = _occupiedStart.AddDays(-1),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    // Test Case 9: SD in A, ED in O → Throws ArgumentException (invalid date range)
    [Fact]
    public async Task CreateBookingAsync_SDInA_EDInO_ThrowsArgumentException()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedEnd.AddDays(5),
            EndDate = _occupiedStart.AddDays(5),
            CustomerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.CreateBooking(booking));
    }

    // Test Case 10: SD in A, ED in A → Booking Created
    [Fact]
    public async Task CreateBookingAsync_SDInA_EDInA_ReturnsTrue()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedEnd.AddDays(1),
            EndDate = _occupiedEnd.AddDays(5),
            CustomerId = 1
        };
        await SetupAvailableRoomAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateBookingAsync_EndDateEqualsOccupiedStartDate_ReturnsFalse()
    {
        // Arrange - Ends when occupied period starts
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(-3),
            EndDate = _occupiedStart,
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateBookingAsync_StartDateEqualsOccupiedEndDate_ReturnsTrue()
    {
        // Arrange - Starts when occupied period ends
        var booking = new Booking
        {
            StartDate = _occupiedEnd,
            EndDate = _occupiedEnd.AddDays(2),
            CustomerId = 1
        };
        await SetupAvailableRoomAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateBookingAsync_CompletelyOverlapsOccupiedPeriod_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = _occupiedStart.AddDays(-2),
            EndDate = _occupiedEnd.AddDays(2),
            CustomerId = 1
        };
        await SetupOccupiedPeriodAsync();

        // Act
        var result = await _bookingManager.CreateBooking(booking);

        // Assert
        Assert.False(result);
    }

    private async Task SetupAvailableRoomAsync()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Description = "Test Room" }
        };
        var bookings = new List<Booking>();

        _roomRepoMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(rooms);
        _bookingRepoMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(bookings);
        _bookingRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Booking>()))
            .Returns(Task.CompletedTask);
    }

    private async Task SetupOccupiedPeriodAsync()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Description = "Test Room" }
        };
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                StartDate = _occupiedStart,
                EndDate = _occupiedEnd,
                IsActive = true,
                RoomId = 1,
                CustomerId = 1
            }
        };

        _roomRepoMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(rooms);
        _bookingRepoMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(bookings);
    }
}
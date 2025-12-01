using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class BookingManagerWhiteboxTests
{
    private readonly BookingManager _bookingManager;
    private readonly Mock<IRepository<Booking>> _mockBookingRepo;
    private readonly Mock<IRepository<Room>> _mockRoomRepo;

    public BookingManagerWhiteboxTests()
    {
        _mockBookingRepo = new Mock<IRepository<Booking>>();
        _mockRoomRepo = new Mock<IRepository<Room>>();
        _bookingManager = new BookingManager(_mockBookingRepo.Object, _mockRoomRepo.Object);
    }

    // ================ FindAvailableRoom Tests (4 paths) ================

    [Fact]
    public async Task FindAvailableRoom_PastStartDate_ThrowsArgumentException()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-1); // Past date
        var endDate = DateTime.Today.AddDays(3);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.FindAvailableRoom(startDate, endDate));
    }

    [Fact]
    public async Task FindAvailableRoom_StartDateAfterEndDate_ThrowsArgumentException()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(5);
        var endDate = DateTime.Today.AddDays(3); // startDate > endDate

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.FindAvailableRoom(startDate, endDate));
    }

    [Fact]
    public async Task FindAvailableRoom_NoRoomsAvailable_ReturnsMinusOne()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        // No rooms in repository
        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room>());
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(new List<Booking>());

        // Act
        var result = await _bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task FindAvailableRoom_RoomAvailable_ReturnsRoomId()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);
        var expectedRoomId = 101;

        var rooms = new List<Room> { new() { Id = expectedRoomId, Description = "Room 101" } };
        var bookings = new List<Booking>(); // No bookings

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.Equal(expectedRoomId, result);
    }

    [Fact]
    public async Task FindAvailableRoom_AllRoomsBooked_ReturnsMinusOne()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        var rooms = new List<Room>
        {
            new() { Id = 101, Description = "Room 101" },
            new() { Id = 102, Description = "Room 102" }
        };

        // Both rooms have overlapping bookings
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                RoomId = 101,
                StartDate = startDate.AddDays(-1),
                EndDate = endDate.AddDays(1),
                IsActive = true
            },
            new()
            {
                Id = 2,
                RoomId = 102,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true
            }
        };

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.FindAvailableRoom(startDate, endDate);

        // Assert
        Assert.Equal(-1, result);
    }

    // ================ GetFullyOccupiedDates Tests (7 paths) ================

    [Fact]
    public async Task GetFullyOccupiedDates_InvalidDates_ThrowsArgumentException()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(5);
        var endDate = DateTime.Today.AddDays(3); // startDate > endDate

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookingManager.GetFullyOccupiedDates(startDate, endDate));
    }

    [Fact]
    public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        var rooms = new List<Room> { new() { Id = 1 } };
        var bookings = new List<Booking>(); // No bookings

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_PartialOccupancy_ReturnsEmptyList()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        var rooms = new List<Room>
        {
            new() { Id = 1 },
            new() { Id = 2 } // 2 rooms total
        };

        // Only 1 room booked (not fully occupied)
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                RoomId = 1,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true
            }
        };

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_SingleFullyOccupiedDate_ReturnsThatDate()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);
        var fullyOccupiedDate = DateTime.Today.AddDays(2);

        var rooms = new List<Room>
        {
            new() { Id = 1 },
            new() { Id = 2 } // 2 rooms total
        };

        var bookings = new List<Booking>
        {
            // Room 1 booked for entire period
            new() { Id = 1, RoomId = 1, StartDate = startDate, EndDate = endDate, IsActive = true },
            // Room 2 booked only on day 2
            new() { Id = 2, RoomId = 2, StartDate = fullyOccupiedDate, EndDate = fullyOccupiedDate, IsActive = true }
        };

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(fullyOccupiedDate, result[0]);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_MultipleFullyOccupiedDates_ReturnsThoseDates()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(4);

        var rooms = new List<Room>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        var bookings = new List<Booking>
        {
            // Both rooms booked on days 2 and 3
            new()
            {
                Id = 1, RoomId = 1, StartDate = startDate.AddDays(1), EndDate = startDate.AddDays(2), IsActive = true
            },
            new()
            {
                Id = 2, RoomId = 2, StartDate = startDate.AddDays(1), EndDate = startDate.AddDays(2), IsActive = true
            }
        };

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Equal(2, result.Count); // Days 2 and 3
        Assert.Contains(startDate.AddDays(1), result);
        Assert.Contains(startDate.AddDays(2), result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_AllDatesFullyOccupied_ReturnsAllDates()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        var rooms = new List<Room>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        var bookings = new List<Booking>
        {
            // Both rooms booked for entire period
            new() { Id = 1, RoomId = 1, StartDate = startDate, EndDate = endDate, IsActive = true },
            new() { Id = 2, RoomId = 2, StartDate = startDate, EndDate = endDate, IsActive = true }
        };

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Equal(3, result.Count); // All 3 days
        for (var d = startDate; d <= endDate; d = d.AddDays(1)) Assert.Contains(d, result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_InactiveBookings_ReturnsEmptyList()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        var rooms = new List<Room> { new() { Id = 1 } };

        // Only inactive bookings
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                RoomId = 1,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = false // Inactive!
            }
        };

        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Empty(result); // Inactive bookings should be ignored
    }
}
# White-box Analysis Using DD-Path Graphs

In the provided graphs, sequential statements (linear code without branching) are grouped into single nodes. This is the standard DD-path representation: each node represents a process block, and edges connect decision nodes, forming the complete DD-path structure.

## 1. DD-Paths for FindAvailableRoom

A DD-path is an execution path between two decision nodes (or start/exit nodes).

**Path Mapping:**

| Path ID | From Node           | To Node             | Description (Code Block)                                          |
| ------- | ------------------- | ------------------- | ----------------------------------------------------------------- |
| 1       | Start               | Decision (Dates)    | Entry to first validation                                         |
| 2       | Decision (Dates)    | Throw Exception     | True branch: `startDate <= DateTime.Today OR startDate > endDate` |
| 3       | Throw Exception     | End                 | Exit via exception                                                |
| 4       | Decision (Dates)    | Initialize Data     | False branch: Get bookings, filter active, get rooms              |
| 5       | Initialize Data     | Loop (Foreach)      | Enter room iteration                                              |
| 6       | Loop (Foreach)      | Check Room Bookings | Has next room: Get bookings for current room                      |
| 7       | Check Room Bookings | Return Room ID      | True: No overlapping bookings found, return `room.Id`             |
| 8       | Return Room ID      | End                 | Exit with room ID                                                 |
| 9       | Check Room Bookings | Loop (Foreach)      | False: Room has overlapping bookings, continue to next room       |
| 10      | Loop (Foreach)      | Return -1           | Done: No more rooms, loop finished                                |
| 11      | Return -1           | End                 | Exit with -1 (no room available)                                  |

**Cyclomatic Complexity Calculation:**

- Decision Points:
  1. `if (startDate <= DateTime.Today || startDate > endDate)`
  2. `foreach (var room in rooms)` (implicit entry/exit)
  3. `if (activeBookingsForCurrentRoom.All(...))`
  4. Implicit: continue loop or exit
- V(G) = Number of binary decisions + 1 = 4 + 1 = **5**

## 2. DD-Paths for GetFullyOccupiedDates

**Path Mapping:**

| Path ID | From Node                  | To Node                    | Description (Code Block)                            |
| ------- | -------------------------- | -------------------------- | --------------------------------------------------- |
| 1       | Start                      | Decision (Dates)           | Entry to date validation                            |
| 2       | Decision (Dates)           | Throw Exception            | True branch: `startDate > endDate`                  |
| 3       | Throw Exception            | End                        | Exit via exception                                  |
| 4       | Decision (Dates)           | Initialize                 | False branch: Initialize list, get rooms & bookings |
| 5       | Initialize                 | Decision (Any Bookings?)   | Check if any bookings exist                         |
| 6       | Decision (Any Bookings?)   | Return Empty List          | False: No bookings, return empty list               |
| 7       | Return Empty List          | End                        | Exit with empty list                                |
| 8       | Decision (Any Bookings?)   | Init Loop (d=startDate)    | True: Setup date iteration                          |
| 9       | Init Loop                  | Decision (d <= endDate?)   | Enter loop condition check                          |
| 10      | Decision (d <= endDate?)   | Calculate Bookings         | True: Count active bookings for current date        |
| 11      | Calculate Bookings         | Decision (Fully Occupied?) | Check if count >= number of rooms                   |
| 12      | Decision (Fully Occupied?) | Add to List                | True: Date is fully occupied, add to list           |
| 13      | Add to List                | Increment Date             | Move to next date: `d = d.AddDays(1)`               |
| 14      | Decision (Fully Occupied?) | Increment Date             | False: Date not fully occupied, move to next        |
| 15      | Increment Date             | Decision (d <= endDate?)   | Loop back to check next date                        |
| 16      | Decision (d <= endDate?)   | Return List                | False: Loop finished                                |
| 17      | Return List                | End                        | Exit with fully occupied dates list                 |

**Cyclomatic Complexity Calculation:**

- Decision Points:
  1. `if (startDate > endDate)`
  2. `if (bookings.Any())`
  3. `for (DateTime d = startDate; d <= endDate; ...)` (loop condition)
  4. LINQ `where b.IsActive && ...` (implicit filtering)
  5. `if (noOfBookings.Count() >= noOfRooms)`
- V(G) = Number of binary decisions + 1 = 5 + 1 = **6**

## 3. White-box Test Derivation

### Basis Path Testing Methodology:

We used **basis path testing** where:

- **Minimum tests required = Cyclomatic Complexity (V(G))**
- Each test covers an **independent execution path**
- Tests derived directly from **DD-path analysis**

### FindAvailableRoom Test Mapping (5 tests for V(G) = 5):

1. **Test 1:** Covers Path 2-3 - Invalid dates (past date)
2. **Test 2:** Covers Path 2-3 - Invalid dates (start > end)
3. **Test 3:** Covers Path 4-5-6-9-10-11 - No rooms available
4. **Test 4:** Covers Path 4-5-6-7-8 - Room available
5. **Test 5:** Covers Path 4-5-6-9-(loop)-10-11 - All rooms booked

### GetFullyOccupiedDates Test Mapping (6 tests for V(G) = 6):

1. **Test 1:** Covers Path 2-3 - Invalid dates
2. **Test 2:** Covers Path 6-7 - No bookings
3. **Test 3:** Covers Path 8-9-10-11-14-15-16-17 - Partial occupancy (no dates fully occupied)
4. **Test 4:** Covers Path 8-9-10-11-12-13-15-16-17 - Single date fully occupied
5. **Test 5:** Covers Path 8-9-10-11-12-13-15-(loop)-16-17 - Multiple dates fully occupied
6. **Test 6:** Covers Path 8-9-10-11-12-13-15-(all dates)-16-17 - All dates fully occupied

## 4. Multiple Condition Coverage (MCC) Implementation

Following the assignment guidance for complex decisions, we applied **Multiple Condition Coverage (MCC)** to the `GetFullyOccupiedDates` method, specifically testing the LINQ filter condition:

```csharp
where b.IsActive && d >= b.StartDate && d <= b.EndDate
```

### Multiple Condition Coverage (MCC) Table

| Case | b.IsActive | d >= b.StartDate | d <= b.EndDate | b.IsActive && d >= b.StartDate && d <= b.EndDate (Result) | Description                                                                                                                           |
| :--: | :--------: | :--------------: | :------------: | :-------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------------------------------ |
|  1   |     T      |        T         |       T        |                             T                             | **Active and within the date range.**                                                                                                 |
|  2   |     T      |        T         |       F        |                             F                             | Active, started, but date $d$ is after EndDate.                                                                                       |
|  3   |     T      |        F         |       T        |                             F                             | Active, date $d$ is before StartDate, but also before/on EndDate (this is impossible, as $d < \text{Start} \implies d < \text{End}$). |
|  4   |     T      |        F         |       F        |                             F                             | Active, date $d$ is before StartDate and after EndDate (impossible).                                                                  |
|  5   |     F      |        T         |       T        |                             F                             | Inactive, but within the date range.                                                                                                  |
|  6   |     F      |        T         |       F        |                             F                             | Inactive, started, but date $d$ is after EndDate.                                                                                     |
|  7   |     F      |        F         |       T        |                             F                             | Inactive, date $d$ is before StartDate.                                                                                               |
|  8   |     F      |        F         |       F        |                             F                             | Inactive, date $d$ is before StartDate and after EndDate (impossible).                                                                |

---

# Explain about the DD-path

In the graphs provided, I grouped sequential statements (linear code that doesn't branch) into single nodes. This is the standard way to represent a DD-path in a visual graph: every node representing a process block and every edge connecting decisions constitutes the DD-path structure.
Here is the mapping of the DD-paths for both methods based on the Draw.io files provided.

1. DD-Paths for FindAvailableRoom

   A DD-path is a path of execution between two decision nodes (or start/exit).

   Path ID From Node To Node Description (Code Block)

   1. Start Decision (Dates) Entry to first if.
   2. Decision (Dates) Throw Ex True branch: startDate > endDate.
   3. Throw Ex End Exit path.
   4. Decision (Dates) Loop (Foreach) False branch: repo.GetAllAsync, Where, etc. (Sequential block).
   5. Loop (Foreach) Check Availability Has Next: Inside loop, filter bookings for current room.
   6. Check Availability Return ID True: All(...) condition met. Return room.Id.
   7. Return ID End Exit path.
   8. Check Availability Loop (Foreach) False: Condition not met, go to next iteration.
   9. Loop (Foreach) Return -1 Done: Loop finishes.
   10. Return -1 End Exit path.

   Cyclomatic Complexity: 4 (3 Decisions + 1)

2. DD-Paths for GetFullyOccupiedDates

   Path ID From Node To Node Description (Code Block)

   1. Start Decision (Dates) Entry to first if.

   2. Decision (Dates) Throw Ex True branch: startDate > endDate.
   3. Throw Ex End Exit path.
   4. Decision (Dates) Decision (Any?) False branch: Init List, Fetch Rooms, Fetch Bookings.
   5. Decision (Any?) Init Loop (d=Start) True: bookings.Any() is true. Setup loop.
   6. Decision (Any?) Return List False: Skip everything, return empty list.
   7. Init Loop Decision (d <= End) Enter the for loop logic.
   8. Decision (d <= End) Decision (Count) True: Calculate noOfBookings.
   9. Decision (Count) Add to List True: Count() >= noOfRooms.
   10. Add to List Increment (d++) Add date, then move to d.AddDays(1).
   11. Decision (Count) Increment (d++) False: Skip adding, move to d.AddDays(1).
   12. Increment (d++) Decision (d <= End) Loop back to check date condition.
   13. Decision (d <= End) Return List False: Loop finished.
   14. Return List End Exit path.

   Cyclomatic Complexity: 6 (5 Decisions + 1)

   Visual Representation in the Draw.io Files

   In the XML I provided:
   Rectangles represent the linear sequential blocks (the processing part of a DD-path).

   Diamonds represent the Decision nodes (the start/end of a DD-path).

   Arrows represent the flow connecting them.

   You can use these graphs directly for Basis Path Testing or calculating Cyclomatic Complexity.

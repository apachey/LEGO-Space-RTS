using System;

namespace LegoSpaceRTS.SimCore
{
/// <summary>
/// Deterministic minimum-cost one-to-one assignment for formation members and
/// their compatible destination slots. This runs once when a Move command is
/// accepted; it is not a persistent movement coordinator.
/// </summary>
public static class FormationSlotAssignment
{
    public static int[] Solve(long[,] costs)
    {
        if (costs == null) throw new ArgumentNullException(nameof(costs));
        int rows = costs.GetLength(0), columns = costs.GetLength(1);
        if (rows != columns) throw new ArgumentException("Formation slot assignment requires a square cost matrix.", nameof(costs));
        if (rows == 0) return Array.Empty<int>();

        // Successive shortest augmenting paths for the linear assignment
        // problem. Stable row order and the smallest-column tie break make the
        // selected optimum deterministic when several assignments cost the same.
        long[] rowPotential = new long[rows + 1];
        long[] columnPotential = new long[columns + 1];
        int[] matchedRowByColumn = new int[columns + 1];
        int[] previousColumn = new int[columns + 1];

        for (int row = 1; row <= rows; row++)
        {
            matchedRowByColumn[0] = row;
            int currentColumn = 0;
            long[] minimumReducedCost = new long[columns + 1];
            bool[] visited = new bool[columns + 1];
            for (int column = 1; column <= columns; column++) minimumReducedCost[column] = long.MaxValue;

            do
            {
                visited[currentColumn] = true;
                int currentRow = matchedRowByColumn[currentColumn];
                long delta = long.MaxValue;
                int nextColumn = 0;

                for (int column = 1; column <= columns; column++)
                {
                    if (visited[column]) continue;
                    long cost = costs[currentRow - 1, column - 1];
                    if (cost < 0) throw new ArgumentOutOfRangeException(nameof(costs), "Formation assignment costs cannot be negative.");
                    long reducedCost = checked(cost - rowPotential[currentRow] - columnPotential[column]);
                    if (reducedCost < minimumReducedCost[column])
                    {
                        minimumReducedCost[column] = reducedCost;
                        previousColumn[column] = currentColumn;
                    }
                    if (minimumReducedCost[column] < delta)
                    {
                        delta = minimumReducedCost[column];
                        nextColumn = column;
                    }
                }

                if (nextColumn == 0 || delta == long.MaxValue) throw new InvalidOperationException("Formation slot assignment has no complete solution.");
                for (int column = 0; column <= columns; column++)
                {
                    if (visited[column])
                    {
                        rowPotential[matchedRowByColumn[column]] = checked(rowPotential[matchedRowByColumn[column]] + delta);
                        columnPotential[column] = checked(columnPotential[column] - delta);
                    }
                    else if (column > 0)
                    {
                        minimumReducedCost[column] = checked(minimumReducedCost[column] - delta);
                    }
                }
                currentColumn = nextColumn;
            }
            while (matchedRowByColumn[currentColumn] != 0);

            do
            {
                int priorColumn = previousColumn[currentColumn];
                matchedRowByColumn[currentColumn] = matchedRowByColumn[priorColumn];
                currentColumn = priorColumn;
            }
            while (currentColumn != 0);
        }

        int[] assignedColumnByRow = new int[rows];
        for (int column = 1; column <= columns; column++) assignedColumnByRow[matchedRowByColumn[column] - 1] = column - 1;
        return assignedColumnByRow;
    }
}
}

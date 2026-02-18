using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
namespace Simple_Sheet_App.Models
{
    public class UndoRedoManager
    {
        Stack<Object> undoStack = new Stack<Object>();
        Stack<Object> redoStack = new Stack<Object>();
        private GridManager gridManager;
        public UndoRedoManager(GridManager gridManager)
        {
            this.gridManager = gridManager;
        }
        public void Register(Object newEntry)
        {
            undoStack.Push(newEntry);
            redoStack.Clear();
        }
        public async Task<(Cell cell, int row, int col)?> Undo()
        {
            if (undoStack.Count > 0)
            {
                var popped = undoStack.Pop();
                redoStack.Push(popped);

                if (popped is CellAction cellAction)
                {
                    await gridManager.SetCell(cellAction.row, cellAction.col, cellAction.oldValue);
                    int key = string.IsNullOrEmpty(cellAction.oldValue) ? -1
                        : gridManager.GetKey(cellAction.row, cellAction.col);
                    return (new Cell(cellAction.oldValue, DateTime.Now, key), cellAction.row, cellAction.col);
                }
                else if (popped is InsOrDelAction poppedAction)
                {
                    if (poppedAction.ActionType == "insertRow" || poppedAction.ActionType == "insertCol")
                    {
                        foreach (var tuple in poppedAction.NewValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, "");

                        foreach (var tuple in poppedAction.OldValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, tuple.Item3);
                    }
                    else if (poppedAction.ActionType == "deleteRow" || poppedAction.ActionType == "deleteCol")
                    {
                        foreach (var tuple in poppedAction.NewValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, "");

                        foreach (var tuple in poppedAction.OldValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, tuple.Item3);
                    }
                }
            }
            return null;
        }

        public async Task<(Cell cell, int row, int col)?> Redo()
        {
            if (redoStack.Count > 0)
            {
                var popped = redoStack.Pop();
                undoStack.Push(popped);

                if (popped is CellAction cellAction)
                {
                    await gridManager.SetCell(cellAction.row, cellAction.col, cellAction.newValue);
                    int key = string.IsNullOrEmpty(cellAction.newValue) ? -1
                        : gridManager.GetKey(cellAction.row, cellAction.col);
                    return (new Cell(cellAction.newValue, DateTime.Now, key), cellAction.row, cellAction.col);
                }
                else if (popped is InsOrDelAction poppedAction)
                {
                    if (poppedAction.ActionType == "insertRow" || poppedAction.ActionType == "insertCol")
                    {
                        foreach (var tuple in poppedAction.OldValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, "");

                        foreach (var tuple in poppedAction.NewValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, tuple.Item3);
                    }
                    else if (poppedAction.ActionType == "deleteRow" || poppedAction.ActionType == "deleteCol")
                    {
                        foreach (var tuple in poppedAction.OldValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, "");

                        foreach (var tuple in poppedAction.NewValues)
                            await gridManager.SetCell(tuple.Item1, tuple.Item2, tuple.Item3);
                    }
                }
            }
            return null;
        }
    }
}
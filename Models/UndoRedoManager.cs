using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Simple_Sheet_App.Models
{
    public class UndoRedoManager
    {
        Stack<CellAction> undoStack = new Stack<CellAction>();
        Stack<CellAction> redoStack = new Stack<CellAction>();
        private GridManager gridManager;

        public UndoRedoManager(GridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        public void Register(CellAction cellAction)
        {
            undoStack.Push(cellAction);
            redoStack.Clear();
        }

        public async Task<(Cell cell, int row, int col)?> Undo()
        {
            if (undoStack.Count > 0)
            {
                var popped = undoStack.Pop();
                redoStack.Push(popped);

                await gridManager.SetCell(popped.row, popped.col, popped.oldValue);
                int key = string.IsNullOrEmpty(popped.oldValue) ? -1
                  : gridManager.GetKey(popped.row, popped.col);

                Cell cell = new Cell(popped.oldValue, DateTime.Now, key);
                return (cell, popped.row, popped.col);
            }
            return null;
        }

        public async Task<(Cell cell, int row, int col)?> Redo()
        {
            if (redoStack.Count > 0)
            {
                var popped = redoStack.Pop();
                undoStack.Push(popped);

                await gridManager.SetCell(popped.row, popped.col, popped.newValue);
                int key = string.IsNullOrEmpty(popped.newValue) ? -1
                  : gridManager.GetKey(popped.row, popped.col);

                Cell cell = new Cell(popped.newValue, DateTime.Now, key);
                return (cell, popped.row, popped.col);
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
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

        public async Task<Cell?> Undo()
        {
            if (undoStack.Count > 0)
            {
                var popped = undoStack.Pop();
                redoStack.Push(popped);

                await gridManager.SetCell(popped.row, popped.col, popped.oldValue);

                Cell cell = new Cell(popped.row, popped.col, popped.oldValue, DateTime.Now);
                return cell;
            }
            return null;
        }

        public async Task<Cell?> Redo()
        {
            if (redoStack.Count > 0)
            {
                var popped = redoStack.Pop();
                undoStack.Push(popped);

                await gridManager.SetCell(popped.row, popped.col, popped.newValue);

                Cell cell = new Cell(popped.row, popped.col, popped.newValue, DateTime.Now);
                return cell;
            }
            return null;
        }
    }
}
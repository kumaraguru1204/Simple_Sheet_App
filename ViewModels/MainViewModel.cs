using Simple_Sheet_App.Models;
using System;
using System.Threading.Tasks;

namespace Simple_Sheet_App.ViewModels
{
    public class MainViewModel
    {
        private GridManager gridManager = new GridManager();
        private UndoRedoManager undoRedoManager;
        public MainViewModel()
        {
            undoRedoManager = new UndoRedoManager(gridManager);
        }

        public async Task LoadAsync() => await gridManager.PopulateDb();

        public async Task<Cell> InsertValue(int row, int col, string value)
        {
            string oldValue = gridManager.GetValue(row, col);
            await gridManager.SetCell(row, col, value);
            var action = new CellAction(row, col, oldValue, value);
            undoRedoManager.Register(action);

            int key = string.IsNullOrEmpty(value) ? -1 : gridManager.GetKey(row, col);
            return new Cell(value, DateTime.Now, key);
        }

        public string GetValue(int row, int col) => gridManager.GetValue(row, col);

        /*
        public Cell DeleteValue(int row, int col)
        {
            string oldValue = gridManager.GetValue(row, col);
            gridManager.DeleteValue(row, col);
            var action = new CellAction(row, col, oldValue, "");
            undoRedoManager.Register(action);

            return new Cell(row, col, "", DateTime.Now);
        } 
        */

        public async Task<(Cell cell, int row, int col)?> HandleUndo()
        {
            return await undoRedoManager.Undo();
        }

        public async Task<(Cell cell, int row, int col)?> HandleRedo()
        {
            return await undoRedoManager.Redo();
        }
    }
}
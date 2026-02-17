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
            if(action != null)
            {
            undoRedoManager.Register(action);
            }

            Cell cell = new Cell(row, col, value, DateTime.Now);
            return cell;
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

        public async Task<Cell?> HandleUndo()
        {
            return await undoRedoManager.Undo();
        }

        public async Task<Cell?> HandleRedo()
        {
            return await undoRedoManager.Redo();
        }
    }
}
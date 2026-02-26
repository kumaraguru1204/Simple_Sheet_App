using Microsoft.UI.Composition.Interactions;
using Simple_Sheet_App.Models;
using System;
using System.Collections.Generic;
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

        public void DeleteValue(int row, int col)
        {
            string oldValue = gridManager.GetValue(row, col);
            gridManager.DeleteValue(row, col);
            var action = new CellAction(row, col, oldValue, "");
            undoRedoManager.Register(action);
        }

        public async Task<(Cell cell, int row, int col)?> HandleUndo()
        {
            return await undoRedoManager.Undo();
        }

        public async Task<(Cell cell, int row, int col)?> HandleRedo()
        {
            return await undoRedoManager.Redo();
        }

        public async Task<bool> insertRow(int position)
        {
            var insertRow = await gridManager.insertRow(position);
            undoRedoManager.Register(insertRow);
            return true;
        }

        public async Task<bool> insertColumn(int position)
        {
            var insertCol = await gridManager.insertColumn(position);
            Dictionary<String, List<List<(int, int)>>> actionDict = new Dictionary<string, List<List<(int, int)>>>();
            undoRedoManager.Register(insertCol);
            return true;
        }

        public async Task<bool> deleteRow(int position)
        {
            var deleteRow = await gridManager.deleteRow(position);
            undoRedoManager.Register(deleteRow);
            return true;
        }

        public async Task<bool> deleteColumn(int position)
        {
            var deleteCol = await gridManager.deleteColumn(position);
            undoRedoManager.Register(deleteCol);
            return true;
        }
    }
}
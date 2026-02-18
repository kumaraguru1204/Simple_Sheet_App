using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple_Sheet_App;
using Windows.UI.Popups;
using Windows.Web.Syndication;
using MySqlConnector;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Security.Cryptography.Core;

namespace Simple_Sheet_App.Models
{
    public class GridManager
    {
        private String conStr = "server=localhost;user=root;password=mysqlunlocked69;database=sheetapp;";
        Dictionary<int, Cell> db = new Dictionary<int, Cell>();
        Dictionary<(int, int), int> cellKeys = new Dictionary<(int, int), int>();
        int nextKey;
        public async Task PopulateDb()
        {
            nextKey = 1;
            db.Clear();
            cellKeys.Clear();
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();

            String query = "SELECT cellValue, lastModified, cellKey FROM cell;";
            using (var cmd = new MySqlCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    String cellValue = reader.GetString("cellValue");
                    DateTime lastModified = reader.GetDateTime("lastModified");
                    int cellKey = reader.GetInt32("cellKey");

                    db[cellKey] = new Cell(cellValue, lastModified, cellKey);
                    if (cellKey >= nextKey) nextKey = cellKey + 1;
                }
            }

            String query1 = "SELECT cellKey, rowNum, colNum FROM cellKeys;";
            using (var cmd1 = new MySqlCommand(query1, conn))
            using (var reader1 = await cmd1.ExecuteReaderAsync())
            {
                while (await reader1.ReadAsync())
                {
                    int cellKey = reader1.GetInt32("cellKey");
                    int rowNum = reader1.GetInt32("rowNum");
                    int colNum = reader1.GetInt32("colNum");

                    cellKeys[(rowNum, colNum)] = cellKey;
                }
            }
        }


        public async Task SetCell(int row, int col, String val)
        {
            if (string.IsNullOrEmpty(val))
            {
                if (cellKeys.ContainsKey((row, col)))
                {
                    await DeleteDbValue(row, col);
                    int key = cellKeys[(row, col)];
                    db.Remove(key);
                    cellKeys.Remove((row, col));
                }
            }
            else if (cellKeys.ContainsKey((row, col)))
            {
                int key = cellKeys[(row, col)];
                db[key].UpdateValue(val);
                await UpdateDbValue(db[key]);
            }
            else
            {
                int key = nextKey++;
                Cell newCell = new Cell(val, DateTime.Now, key);
                db[key] = newCell;
                cellKeys[(row, col)] = key;
                await InsertDbValue(newCell, row, col);
            }
        }

        public String GetValue(int row, int col)
        {
            if (!cellKeys.ContainsKey((row, col))) return "";
            int key = cellKeys[(row, col)];
            if (!db.ContainsKey(key)) return "";
            return db[key].Value;
        }

        public void DeleteValue(int row, int col)
        {
            if (db.ContainsKey(cellKeys[(row, col)]))
            {
                db.Remove(cellKeys[(row, col)]);
                cellKeys.Remove((row, col));
            }
        }

        public int GetKey(int row, int col)
        {
            return cellKeys[(row, col)];
        }

        public async Task InsertDbValue(Cell newCell, int row, int col)
        {
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();

            String query = @"INSERT INTO cell (cellValue, lastModified, cellKey)
                                      VALUES(@val, @modified, @key) ";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@val", newCell.Value);
            cmd.Parameters.AddWithValue("@modified", newCell.LastModified);
            cmd.Parameters.AddWithValue("@key", newCell.CellKey);
            await cmd.ExecuteNonQueryAsync();

            String query1 = @"INSERT INTO cellKeys (cellKey, rowNum, colNum)
                                      VALUES(@key, @row, @col) ";
            using var cmd1 = new MySqlCommand(query1, conn);
            cmd1.Parameters.AddWithValue("@key", newCell.CellKey);
            cmd1.Parameters.AddWithValue("@row", row);
            cmd1.Parameters.AddWithValue("@col", col);
            await cmd1.ExecuteNonQueryAsync();
        }

        public async Task DeleteDbValue(int row, int col)
        {
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();

            String query = @"DELETE FROM cell
                             WHERE cellKey = @key";
            String query1 = @"DELETE FROM cellKeys
                              WHERE cellKey = @key1";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@key", cellKeys[(row,col)]);
            await cmd.ExecuteNonQueryAsync();

            using var cmd1 = new MySqlCommand(query1, conn);
            cmd1.Parameters.AddWithValue("@key1", cellKeys[(row, col)]);
            await cmd1.ExecuteNonQueryAsync();

        }

        public async Task UpdateDbValue(Cell cell)
        {
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();

            String query = @"UPDATE cell
                             SET cellValue = @val, lastModified = @modified
                             WHERE cellKey = @key";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@val", cell.Value);
            cmd.Parameters.AddWithValue("@modified", cell.LastModified);
            cmd.Parameters.AddWithValue("@key", cell.CellKey);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> insertRow(int position)
        {
            var keysToShift = cellKeys.Keys
                .Where(k => k.Item1 >= position)
                .ToList();

            foreach (var key in keysToShift)
            {
                int val = cellKeys[key];
                cellKeys.Remove(key);
                cellKeys[(key.Item1 + 1, key.Item2)] = val;
            }
            return true;
        }

        public async Task<bool> insertColumn(int position)
        {
            var keysToShift = cellKeys.Keys
                .Where(k => k.Item2 >= position)
                .ToList();

            foreach (var key in keysToShift)
            {
                int val = cellKeys[key];
                cellKeys.Remove(key);
                cellKeys[(key.Item1, key.Item2+1)] = val;
            }
            return true;
        }

        public async Task<bool> deleteRow(int position)
        {
            var keysToShift = cellKeys.Keys
                .Where(k => k.Item1 >= position)
                .ToList();

            foreach (var key in keysToShift)
            {
                int val = cellKeys[key];
                cellKeys.Remove(key);
                if(key.Item1 != position)
                {
                cellKeys[(key.Item1 - 1, key.Item2)] = val;
                }
            }
            return true;
        }

        public async Task<bool> deleteColumn(int position)
        {
            var keysToShift = cellKeys.Keys
                .Where(k => k.Item2 >= position)
                .ToList();

            foreach (var key in keysToShift)
            {
                int val = cellKeys[key];
                cellKeys.Remove(key);
                if (key.Item2 != position)
                {
                cellKeys[(key.Item1, key.Item2-1)] = val;
                }
            }
            return true;
        }
    }
}
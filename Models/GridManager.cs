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
        Dictionary<int, List<Cell>> db = new Dictionary<int, List<Cell>>();
        public async Task PopulateDb()
        {
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();
            String query = "SELECT rowNum, colNum, cellValue, lastModified FROM cell;";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            db.Clear();

            while (await reader.ReadAsync())
            {
                int rowNum = reader.GetInt32("rowNum") - 1;
                int colNum = reader.GetInt32("colNum") - 1;
                String cellValue = reader.GetString("cellValue");
                DateTime lastModified = reader.GetDateTime("lastModified");

                if (!db.ContainsKey(rowNum))
                {
                    db[rowNum] = new List<Cell>();
                }

                db[rowNum].Add(new Cell(rowNum, colNum, cellValue, lastModified));
            }
        }


        public async Task SetCell(int row, int col, String val)
        {
            if (!db.ContainsKey(row))
            {
                db[row] = new List<Cell>();
            }

            var rList = db[row];
            var cell = rList.FirstOrDefault(c => c.ColNum == col);

            if (cell == null)
            {
                if (!string.IsNullOrEmpty(val))
                {
                    Cell newCell = new Cell(row, col, val, DateTime.Now);
                    rList.Add(newCell);
                    await InsertDbValue(newCell);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(val))
                {
                    rList.Remove(cell);
                    await DeleteDbValue(row, col);
                }
                else
                {
                    cell.UpdateValue(val);
                    await UpdateDbValue(cell);
                }
            }
        }

        public String GetValue(int row, int col)
        {
            if (!db.ContainsKey(row))
            {
                return "";
            }
            else
            {
                foreach (var cell in db[row])
                {
                    if (cell.ColNum == col)
                    {
                        return cell.Value;
                    }
                }
                return "";
            }
        }

        public void DeleteValue(int row, int col)
        {
            if (db.ContainsKey(row))
            {
                foreach (var cell in db[row])
                {
                    if (cell.ColNum == col)
                    {
                        db[row].Remove(cell);
                        return;
                    }
                }
            }
        }

        public async Task InsertDbValue(Cell newCell)
        {
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();

            String query = @"INSERT INTO cell (rowNum, colNum, cellValue, lastModified)
                                      VALUES(@row, @col, @val, @modified) ";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@row", newCell.RowNum + 1);
            cmd.Parameters.AddWithValue("@col", newCell.ColNum + 1);
            cmd.Parameters.AddWithValue("@val", newCell.Value);
            cmd.Parameters.AddWithValue("@modified", newCell.LastModified);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteDbValue(int row, int col)
        {
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();

            String query = @"DELETE FROM cell
                             WHERE rowNum = @row and colNum = @col";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@row", row + 1);
            cmd.Parameters.AddWithValue("@col", col + 1);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateDbValue(Cell cell)
        {
            using var conn = new MySqlConnection(conStr);
            await conn.OpenAsync();

            String query = @"UPDATE cell
                             SET cellValue = @val, lastModified = @modified
                             WHERE rowNum = @row and colNum = @col";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@row", cell.RowNum + 1);
            cmd.Parameters.AddWithValue("@col", cell.ColNum + 1);
            cmd.Parameters.AddWithValue("@val", cell.Value);
            cmd.Parameters.AddWithValue("@modified", cell.LastModified);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProductivityTracker.Models
{
    public class ProductivityData
    {
        private readonly string _dataFile = "ProductivityData.json";
        private Dictionary<string, int> _productivityData = new Dictionary<string, int>();

        public ProductivityData() {
            LoadData();
        }
        public void LoadData()
        {
            if (File.Exists(_dataFile))
            {

                var json = File.ReadAllText(_dataFile);
                _productivityData = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            }
            else
            {
                _productivityData = new Dictionary<string, int>();

            }
        }

        public int TypingMinutes
        {

            get
            {
                var today = DateTime.Now.ToString("dd/MM/yyyy");
                return _productivityData.TryGetValue(today, out int minutes) ? minutes : 0;
            }
            set
            {
                var today = DateTime.Now.ToString("dd/MM/yyyy");
                _productivityData[today] = value;
                SaveData();
            }

        }

        public void SaveData()
        {
            var json = JsonSerializer.Serialize(_productivityData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dataFile, json);
        }
    }
}

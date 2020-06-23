using System;
using OfficeOpenXml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using WA_Test_V5.Interface.TreeView;

namespace WA_Test_V5.GetData.Excel
{
    public class GetExcelData
    {
        private ExcelPackage pack;
        private string path;
        public GetExcelData(string filePath)
        {
            path = filePath;
            FileInfo fInfo = new FileInfo(path);
            if (fInfo.Exists != true) throw new Exception();
            pack = new ExcelPackage(fInfo);
        }
        public List<TreeViewElements> GetSample()
        {
            var sheets = pack.Workbook.Worksheets;
            var dataSheet = sheets.First();
            var numberOfRows = dataSheet.Dimension.End.Row;
            var numberOfCols = dataSheet.Dimension.End.Column;
            ExcelRange Cells = dataSheet.Cells;
            var _SampleTreeView = new List<TreeViewElements>();
            for (int rowIterator = 2; rowIterator <= numberOfRows; rowIterator++)
            {
                var e = new TreeViewElements()
                {
                    ID = Cells[rowIterator, 1].Value.ToString(),
                    Parent_ID = Cells[rowIterator, 2].Value.ToString(),
                    Name = Cells[rowIterator, 3].Value.ToString(),
                    CID = Convert.ToInt32(Cells[rowIterator, 4].Value),
                };
                _SampleTreeView.Add(e);
            }
            return _SampleTreeView;
        }
    }
    public class GetExcelDataNew
    {
        private ExcelPackage pack;
        private string path;
        public GetExcelDataNew(string filePath)
        {
            path = filePath;
            FileInfo fInfo = new FileInfo(path);
            if (fInfo.Exists != true) throw new Exception();
            pack = new ExcelPackage(fInfo);
        }
        public List<TreeViewElements> GetNew()
        {
            var sheets = pack.Workbook.Worksheets;
            var dataSheet = sheets.First();
            var numberOfRows = dataSheet.Dimension.End.Row;
            var numberOfCols = dataSheet.Dimension.End.Column;
            ExcelRange Cells = dataSheet.Cells;
            // Сортировка данных таблицы, чтобы в дереве все значения были отсортированы
            Cells["A2:J"].Sort(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new bool[] { false, false, false, false, false, false, false, false, false, false });
            var _TreeViewNew = new List<TreeViewElements>();
            int count = 1; // Уникальный индекс (обычный счетчик-инкремент)

            /* newListElem - для хранения строки таблицы, чтобы потом можно было получить Parent_ID
             * Parent_ID для текущего значения - всегда ID предыдущего значения
             * вид - |значение_первого_столбика|значение второго_столбика|значение_третьего_столбика и так далее (для каждого уровня вложенности)
             * разделитель типа - " | " используется, чтобы можно было легко распарсить строку (отделить значени одного столбца от другого)
             */
            var newListElem = new Dictionary<string, string>();

            // Заполняем нулевые узлы дерева (у которых нет родителя) и запоминаем их в newListElem
            for (int rowIterator = 2; rowIterator <= numberOfRows; rowIterator++)
            {
                if (!_TreeViewNew.Exists(x => x.Name == Cells[rowIterator, 1].Value.ToString()))
                {
                    var e = new TreeViewElements()
                    {
                        ID = count.ToString(),
                        Parent_ID = "0",
                        Name = Cells[rowIterator, 1].Value.ToString(),
                        CID = -2,
                    };
                    _TreeViewNew.Add(e);
                    newListElem.Add("|" + Cells[rowIterator, 1].Value.ToString(), e.ID);
                    count++;
                }
            }

            // Присваиваем всем значениям ID, которые находятся на одном уровне вложенности, по столбцам
            // не трогаем последний столбец, т.к. CID у предпоследних элементов строк отличается от других
            for (int colIterator = 2; colIterator < numberOfCols - 1; colIterator++)
            {
                for (int rowIterator = 2; rowIterator <= numberOfRows; rowIterator++)
                {
                    // Переменная - строка для хранения промежуточных данных, которые потом запишутся в newListElem
                    var tempElem = "";
                    for (int j = 1; j <= colIterator; j++)
                    {
                        tempElem += "|" + Cells[rowIterator, j].Value.ToString();
                    }
                    if (!newListElem.ContainsKey(tempElem))
                    {
                        var e = new TreeViewElements()
                        {
                            ID = count.ToString(),
                            Parent_ID = newListElem[tempElem.Remove(tempElem.LastIndexOf("|"), tempElem.Length - tempElem.LastIndexOf("|"))],
                            Name = Cells[rowIterator, colIterator].Value.ToString(),
                            CID = -2,
                        };
                        _TreeViewNew.Add(e);
                        newListElem.Add(tempElem, e.ID);
                        count++;
                    }
                }
            }

            // Обходим последний столбец, чтобы значения CID записать в структуру последнего узла дерева
            for (int rowIterator = 2; rowIterator <= numberOfRows; rowIterator++)
            {
                // из переменной - строки получим Parent_ID для записи CID
                var tempElem = "";
                for (int j = 1; j <= numberOfCols-2; j++)
                {
                    tempElem += "|" + Cells[rowIterator, j].Value.ToString();
                }

                var e = new TreeViewElements()
                {
                    ID = count.ToString(),
                    Parent_ID = newListElem[tempElem],
                    Name = Cells[rowIterator, numberOfCols - 1].Value.ToString(),
                    CID = Convert.ToInt32(Cells[rowIterator, numberOfCols].Value),
                };
                _TreeViewNew.Add(e);
                count++;
            }
            return _TreeViewNew;
        }
    }
}
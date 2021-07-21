using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace WpfDataMatching
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int lengthID = 24;
        string path = "C:\\Users\\Public\\Documents\\DataMatching\\nom.xml";
        public MainWindow()
        {
            InitializeComponent();
            TxtBoxShipment.CharacterCasing = CharacterCasing.Upper;
            TxtBoxAdding.CharacterCasing = CharacterCasing.Upper;
            
            if (!File.Exists(path))
            {
                AddXmlFile();
            }
            //проверить на наличие элементов
            if (DataXmlFile().Count == 0)
            {
                //заполнить
                MessageBox.Show("Отсутствуют данные в файле номенклатуры. Заполните файл.");
                var nomenclature = new Nomenclature();
                nomenclature.Show();
            }
        }

        void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            DGAdding.ItemsSource = null;
            DGShipment.ItemsSource = null;
        }

        private void BtnNom_Click(object sender, RoutedEventArgs e)
        {
            var Nomenclature = new Nomenclature();
            Nomenclature.Show();
        }
        /*ПРИЕМ*/
        private void DGAdding_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Список из наименований и колва
                List<string[]> listId = CreateListCountTxtBox(ref DGAdding, ref TxtBoxAdding);
                /*
                 * Здесь надо вытащить данные другой таблицы и сравнить с этой
                 */
                CompareTwoData(ref DGShipment, ref listId);

                //по итогу получили список listID с ИД наименований и колвом
                PullOutTitles(ref listId);
                DGAdding.ItemsSource = null;          
                List<DataAdd> listData = new List<DataAdd>();
                foreach (string[] stringId in listId)
                {
                    DataAdd add = new DataAdd { Column1 = stringId[0], Column2 = stringId[1] };
                    listData.Add(add);
                }
                DGAdding.ItemsSource = listData;


            }
        }

        private void TxtBoxAdding_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTextBoxContent(TxtBoxAdding);
        }

       
        /*ПОГРУЗкА*/
        private void DGShipment_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                List<string[]> listId = CreateListCountTxtBox(ref DGShipment, ref TxtBoxShipment);
                CompareTwoData(ref DGAdding, ref listId);
                PullOutTitles(ref listId);
                DGShipment.ItemsSource = null;
                List<DataAdd> listData = new List<DataAdd>();
                foreach (string[] stringId in listId)
                {
                    DataAdd add = new DataAdd { Column1 = stringId[0], Column2 = stringId[1] };
                    listData.Add(add);
                }
                DGShipment.ItemsSource = listData;
            }
        }

        private void TxtBoxShipment_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTextBoxContent(TxtBoxShipment);
        }



        /*ДРУГОЕ*/
        //обработка вводимых символов
        private bool FilterTextBoxContent(TextBox txtBox)
        {
            string pattern = @"[^0-9^A-F^\s]";
            string text = txtBox.Text;
            //return Match.Success if doesn't match pattern
            Match match = Regex.Match(text, pattern);
            bool matched = false;

            int selectionStart = txtBox.SelectionStart; //индекс символа, кот опред начало фрагмента
            int selectionLength = txtBox.SelectionLength; //длина выд фрагмента в символах
            int leftShift = 0;

            if (match.Success)
            {
                matched = true;
                Capture capture = match.Captures[0]; //одно из совпадений

                int captureLength = capture.Length;
                int captureStart = capture.Index - leftShift;
                int captureEnd = captureStart + captureLength; //конец фрагмента

                int selectionEnd = selectionStart + selectionLength; //конец фрагмента

                //выбираем текст до фрагмента + текст после него
                text = text.Substring(0, captureStart) + text.Substring(captureEnd, text.Length - captureEnd);
                //отправляем в текстбокс
                txtBox.Text = text;

                //не меняет положение курсора
                int boundSelectionStart = selectionStart < captureStart ? -1 : (selectionStart < captureEnd ? 0 : 1);
                int boundSelectionEnd = selectionEnd < captureStart ? -1 : (selectionEnd < captureEnd ? 0 : 1);

                if (boundSelectionStart == -1)
                {
                    if (boundSelectionEnd == 0)
                    {
                        selectionLength -= selectionEnd - captureStart;
                    }
                    else if (boundSelectionEnd == 1)
                    {
                        selectionLength -= captureLength;
                    }
                }
                else if (boundSelectionStart == 0)
                {
                    if (boundSelectionEnd == 0)
                    {
                        selectionStart = captureStart;
                        selectionLength = 0;
                    }
                    else if (boundSelectionEnd == 1)
                    {
                        selectionStart = captureStart;
                        selectionLength -= captureEnd - selectionStart;
                    }
                }
                else if (boundSelectionStart == 1)
                {
                    selectionStart -= captureLength;
                }

                leftShift++;
            }
            else if (txtBox.Text.Length > 1 && txtBox.Text[txtBox.Text.Length - 1] == ' ')
            {
                if (txtBox.Text[txtBox.Text.Length - 2] == ' ')
                {
                    txtBox.Text = txtBox.Text.Substring(0, txtBox.Text.Length - 1);
                    txtBox.SelectionStart = txtBox.Text.Length;
                }
            }
            txtBox.SelectionStart = selectionStart;
            txtBox.SelectionLength = selectionLength;

            return matched;
        }
        //Забирает ИД + названия из ХМЛ
        private List<string[]> DataXmlFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            List<string[]> idNames = new List<string[]>();
            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode xmlNode1 in xmlNode.ChildNodes)
                {
                    if (xmlNode1.FirstChild != null && xmlNode1.LastChild != null)
                    {
                        string[] idname = new string[] { xmlNode1.FirstChild.InnerText, xmlNode1.LastChild.InnerText };
                        idNames.Add(idname);
                    }
                }
            }
            return idNames;
        }
        

        //Вытащить наименования из номенклатуры
        private void PullOutTitles(ref List<string[]> dataIds)
        {
            List<string[]> nomencl = DataXmlFile();
            //если не найдено, то удалить строку
            for (int i = 0; i < dataIds.Count(); i++)
            {
                var stringId = dataIds[i];
                string name = "";
                foreach (string[] idname in nomencl)
                {
                    if (stringId[0] == idname[0])
                    {
                        name = idname[1];
                    }
                }
                if (name.Length == 0)
                {
                    dataIds.RemoveAt(i);
                    i--;
                }
                else
                {
                    dataIds[i][0] = name;
                }
            }

        }
        //Вытащить ИД из номенклатуры
        private void PullOutID(ref List<string[]> dataNames)
        {
            List<string[]> nomencl = DataXmlFile();
            //если не найдено, то удалить строку
            for (int i = 0; i < dataNames.Count(); i++)
            {
                var stringName = dataNames[i];
                string id = "";
                foreach (string[] idname in nomencl)
                {
                    if (stringName[0] == idname[1])
                    {
                        id = idname[0];
                    }
                }
                if (id.Length == 0)
                {
                    dataNames.RemoveAt(i);
                }
                else
                {
                    dataNames[i][0] = id;
                }
            }
        }


        // Подсчет одинаковых ИД
        private void CountID(ref List<string[]> listId)
        {
            for (int i = 0; i < listId.Count; i++)
            {
                int count = Int32.Parse(listId[i][1]);
                for (int j = 0; j < i; j++)
                {
                    count = listId[i][0] == listId[j][0] ? count + Int32.Parse(listId[j][1]) : count;
                }
                if (count == Int32.Parse(listId[i][1]))
                {
                    for (int j = i + 1; j < listId.Count; j++)
                    {
                        count = listId[i][0] == listId[j][0] ? count + Int32.Parse(listId[j][1]) : count;
                    }
                    listId[i][1] = count.ToString();
                }
                else
                {
                    listId.RemoveAt(i);
                    i--;
                }
            }
        }
        //Собрать только нужные ид
        private List<string[]> ListId(string stringId)
        {
            List<string[]> listId = new List<string[]>();
            string[] enteredId = stringId.Split(' '); //массив из введенных строек
            int lenghtArr = 0;
            while (lenghtArr < enteredId.Length)
            {
                if (enteredId[lenghtArr].Length <= lengthID && enteredId[lenghtArr].Length != 0) // проверяем длину + увеличиваем, добавляя 0
                {
                    while (enteredId[lenghtArr].Length < lengthID)
                    {
                        enteredId[lenghtArr] = enteredId[lenghtArr].Insert(0, "0");
                    }
                    string[] helpStringArray = new string[] { enteredId[lenghtArr], "1" };
                    listId.Add(helpStringArray);
                }
                lenghtArr++;
            }
            return listId;
        }
        
        //выкинуть сборку списк в отдельный метод
        private List<string[]> CreateListCountTxtBox(ref DataGrid dataGrid, ref TextBox textBox)
        {
            List<string[]> dataGridStrings = new List<string[]>();
            CreateListCountDG(ref dataGrid, ref dataGridStrings);

            string stringId = textBox.Text;
            textBox.Text = "";
            List<string[]> listId = ListId(stringId);
            listId = listId.Concat(dataGridStrings).ToList();
            //только правильные ИД + колво
            CountID(ref listId);

            return listId;
        }

        //Забираем Наимен и колво из ДГ, меняем наим на ИД
        private void CreateListCountDG(ref DataGrid dataGrid, ref List<string[]> dataGridStrings)
        {            
            //Забираем список из датарид
            ItemCollection itemCollectionDataGrid = dataGrid.Items;
            if (itemCollectionDataGrid.Count != 0)
            {
                foreach (DataAdd dataAdd in itemCollectionDataGrid)
                {
                    string[] dataString = new string[] { dataAdd.Column1, dataAdd.Column2 };
                    dataGridStrings.Add(dataString);
                }
                //Вытащить ИД и добавить их в список
                PullOutID(ref dataGridStrings);                
            }
        }
        
        //Сравнение двух таблиц
        private void CompareTwoData(ref DataGrid anotherGrid, ref List<string[]> listId)
        {
            List<string[]> anotherTable = new List<string[]>();
            CreateListCountDG(ref anotherGrid, ref anotherTable);
            for (int i = 0; i < listId.Count; i++)
            {
                var id = listId[i];
                for (int j = 0; j < anotherTable.Count; j++)
                {
                    var idAnother = anotherTable[j];
                    /////
                    if (idAnother[0] == id[0])
                    {
                        int dif = Int32.Parse(listId[i][1]) - Int32.Parse(anotherTable[j][1]);
                        if (dif < 0)
                        {
                            anotherTable[j][1] = (-dif).ToString();
                            listId.RemoveAt(i);
                            i--;
                        }
                        else if (dif > 0)
                        {
                            anotherTable.RemoveAt(j);
                            listId[i][1] = dif.ToString();
                        }
                        else
                        {
                            anotherTable.RemoveAt(j);
                            listId.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            PullOutTitles(ref anotherTable);
            anotherGrid.ItemsSource = null;
            List<DataAdd> listData = new List<DataAdd>();
            foreach (string[] stringId in anotherTable)
            {
                DataAdd add = new DataAdd { Column1 = stringId[0], Column2 = stringId[1] };
                listData.Add(add);
            }
            anotherGrid.ItemsSource = listData;
        }


        //Создание пустого файла
        private void AddXmlFile()
        {
            
            XmlDocument XmlDoc = new XmlDocument();
            XmlDeclaration XmlDec = XmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlDoc.AppendChild(XmlDec);

            XmlElement ElementDatabase = XmlDoc.CreateElement("database");
            ElementDatabase.SetAttribute("name", "Nomenclature");
            XmlDoc.AppendChild(ElementDatabase);

            XmlElement ElementTable_Structure = XmlDoc.CreateElement("table_structure");
            ElementTable_Structure.SetAttribute("name", "titles");
            ElementDatabase.AppendChild(ElementTable_Structure);

            XmlElement ElementField0 = XmlDoc.CreateElement("field");
            ElementField0.SetAttribute("Field", "id");
            ElementField0.SetAttribute("type", "string");
            ElementTable_Structure.AppendChild(ElementField0);

            XmlElement ElementField1 = XmlDoc.CreateElement("field");
            ElementField1.SetAttribute("Field", "title");
            ElementField1.SetAttribute("type", "string");
            ElementTable_Structure.AppendChild(ElementField1);

            XmlElement ElementTable_Data = XmlDoc.CreateElement("table_data");
            ElementTable_Data.SetAttribute("name", "titles");
            ElementDatabase.AppendChild(ElementTable_Data);

            //добавление
            
            //XmlElement ElementFieldId = XmlDoc.CreateElement("field");
            //ElementFieldId.SetAttribute("name", "id");
            //ElementFieldId.InnerText = "123";
            //ElementRow.AppendChild(ElementFieldId);

            //XmlElement ElementFieldId1 = XmlDoc.CreateElement("field");
            //ElementFieldId1.SetAttribute("name", "title");
            //ElementFieldId1.InnerText = "123";
            //ElementRow.AppendChild(ElementFieldId1);

            XmlDoc.Save(path);
        }

       

        
    }

    public class DataAdd
    {
        public string Column1
        {
            get; set;
        }
        public string Column2
        {
            get; set;
        }
    }



}

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
    /// Логика взаимодействия для Nomenclature.xaml
    /// </summary>
    public partial class Nomenclature : Window
    {
        int lengthID = 24;
        string path = @"nom.xml";


        public Nomenclature()
        {
            InitializeComponent();
            List<string[]> listId = DataXmlFile();
            List<DataAdd> listData = new List<DataAdd>();
            foreach (string[] stringId in listId)
            {
                DataAdd add = new DataAdd { Column1 = stringId[0], Column2 = stringId[1] };
                listData.Add(add);
            }
            DGNom.ItemsSource = listData;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            //сохранить
            EditXmlFile(DataFromDataGrid());
            
            List<string[]> listId = DataXmlFile();
            List<DataAdd> listData = new List<DataAdd>();
            foreach (string[] stringId in listId)
            {
                DataAdd add = new DataAdd { Column1 = stringId[0], Column2 = stringId[1] };
                listData.Add(add);
            }
            DGNom.ItemsSource = null;
            DGNom.ItemsSource = listData;
        }




        private string AddID(string lastId)
        {
            if (lastId == "")
            {
                lastId = "0";
            }
            string id = "";
            int dec = Convert.ToInt32(lastId, 16);
            dec++;
            var temp = 0;
            while (dec >= 15 || dec > 0)
            {
                temp = dec % 16;
                dec = (dec - temp) / 16;
                switch (temp)
                {
                    case 10:
                        id = "A" + id;
                        break;
                    case 11:
                        id = "B" + id;
                        break;
                    case 12:
                        id = "C" + id;
                        break;
                    case 13:
                        id = "D" + id;
                        break;
                    case 14:
                        id = "E" + id;
                        break;
                    case 15:
                        id = "F" + id;
                        break;
                    default:
                        id = Convert.ToString(temp) + id;
                        break;
                }
               
            }
            //24
            while (id.Length < lengthID)
            {
                id = id.Insert(0, "0");
            }            
            return id;
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
        
        //Изменить файл
        private void EditXmlFile(List<string[]> stringData)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(path);
            XmlNode ElementTable_Data = XmlDoc.GetElementsByTagName("table_data")[0];
            ElementTable_Data.RemoveAll();

            //цикл - польз вводит ИД и имя или редачит
            foreach (string[] str in stringData)
            {
                XmlNode ElementRow = XmlDoc.CreateElement("row");
                ElementTable_Data.AppendChild(ElementRow);
                XmlElement ElementFieldId = XmlDoc.CreateElement("field");
                ElementFieldId.SetAttribute("name", "id");
                ElementFieldId.InnerText = str[0];
                ElementRow.AppendChild(ElementFieldId);

                XmlElement ElementFieldId1 = XmlDoc.CreateElement("field");
                ElementFieldId1.SetAttribute("name", "title");
                ElementFieldId1.InnerText = str[1];
                ElementRow.AppendChild(ElementFieldId1);

                //XmlDoc.Save(path);
            }
            XmlDoc.Save(path);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string message = "Сохранить данные в файл нуменклатуры?";
            string caption = "Сохранение изменений";
            MessageBoxButton btn = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show(message, caption, btn);
            switch (result)
            {
                case MessageBoxResult.No:
                    {
                        break;
                    }
                case MessageBoxResult.Yes:
                    {                        
                        EditXmlFile(DataFromDataGrid());
                        break;
                    }
            }            
        }
        //Данные из датагрид
        private List<string[]> DataFromDataGrid()
        {
            ItemCollection itemCollectionDataGrid = DGNom.Items;

            List<string[]> dataGridStrings = new List<string[]>();

            if (itemCollectionDataGrid.Count != 0)
            {
                string lastId = "";
                
                for (int i = 0; i < itemCollectionDataGrid.Count - 1; i++)
                {
                    int k = 1;
                    if (itemCollectionDataGrid[i] is DataAdd)
                    {      
                        DataAdd dataAdd = (DataAdd)itemCollectionDataGrid[i];
                        if (dataAdd.Column2 != "")
                        {
                            if (dataAdd.Column1 == "" || dataAdd.Column1 == null)
                            {
                                dataAdd.Column1 = AddID(lastId);
                            }
                            string[] dataString = new string[] { dataAdd.Column1, dataAdd.Column2 };
                            dataGridStrings.Add(dataString);
                            lastId = dataAdd.Column1;
                        }
                        
                    }
                    
                }
            }
            //проверка на одинаковые наименования
            if (dataGridStrings.Count != 0)
            {

                for (int i = 0; i < dataGridStrings.Count - 1; i++)
                {
                    int count = 0;
                    for (int j = 0; j < dataGridStrings.Count; j++)
                    {
                        bool help = dataGridStrings[i][1] == dataGridStrings[j][1];
                        count = help ? count + 1 : count;
                    }
                    if (count != 1)
                    {
                        dataGridStrings.RemoveAt(i);
                        i--;
                    }
                }
            }
            return dataGridStrings;
        }


    }
}

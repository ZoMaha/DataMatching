using System;
using System.Collections.Generic;
using System.Data;
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

namespace WpfDataMatching
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int lengthID = 6;
        public MainWindow()
        {
            InitializeComponent();
            TxtBoxShipment.CharacterCasing = CharacterCasing.Upper;
            TxtBoxAdding.CharacterCasing = CharacterCasing.Upper;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {

        }

        /*ПРИЕМ*/
        private void GridAdding_KeyUp(object sender, KeyEventArgs e)
        {
            //Считать строку -> разбить на подстроки черех ПРОБЕЛ
            //Добавить изменение нижнего регистра на верхний (а -> А) DONE
            //Проверка ид
            if (e.Key == Key.Enter)
            {
                string addingStringId = TxtBoxAdding.Text;
                TxtBoxAdding.Text = "";
                List<string[]> listId = ListId(addingStringId); //только правильные ИД
                //по итогу получили список listID с ИД наименований
                CountID(ref listId);
                //и количеством повторений
                

                //Добавление в таблицу
                foreach (string[] stringId in listId)
                {
                    DataAdd add = new DataAdd { Column1 = stringId[0], Column2 = stringId[1] };
                    DataGridAdding.Items.Add(add);
                }
               

                //TEST
                
            }

        }

        private void TxtBoxAdding_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTextBoxContent(TxtBoxAdding);
        }


        /*ПОГРУЗкА*/
        private void GridShipment_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string shipmentStringId = TxtBoxShipment.Text;
                List<string[]> listId = ListId(shipmentStringId);

                //поиск и вывод
                foreach (string[] stringId in listId)
                {
                    DataAdd add = new DataAdd { Column1 = stringId[0], Column2 = stringId[1] };
                    DataGridAdding.Items.Add(add);
                }
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
        // Подсчет одинаковых ИД
        private void CountID(ref List<string[]> listId)
        {
            for (int i = 0; i < listId.Count; i++)
            {
                int count = 1;
                for (int j = 0; j < i; j++)
                {
                    count = listId[i][0] == listId[j][0] ? count + 1 : count;
                }
                if (count == 1)
                {
                    for (int j = i + 1; j < listId.Count; j++)
                    {
                        count = listId[i][0] == listId[j][0] ? count + 1 : count;
                    }
                    //string[] helpStringArray = new string[] { listId[i], count.ToString() };
                    //vs.Add(helpStringArray);
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
                    string[] helpStringArray = new string[] { enteredId[lenghtArr], " " };
                    listId.Add(helpStringArray);
                }
                lenghtArr++;
            }
            return listId;
        }
        
      
        
        /*ДОБАВЛЕНИЕ АВТОМАТИЧЕСКОЕ*/


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

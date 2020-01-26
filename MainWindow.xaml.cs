using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace Proto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog DDG;
        //
        int offset = 0;
        int LineNumber;
        //
        int Number_Of_Lines;
        //lines of file
        string[] lines;
        List<String> ListLines;
        //command line words
        string[] words;
        //
        List<String> SuffixCommands;
        //

        int Current_Line;

        public MainWindow()
        {
            InitializeComponent();
            Number_Of_Lines = 10;
            
           
            TerminalSymbol.AppendText("=====>");

            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        }

        private void Loader()
        {
            CleanBoxData();

            if (Number_Of_Lines > ListLines.Count) {
                Number_Of_Lines = ListLines.Count;
            }
            LineNumber = 0;
            ListLines.Insert(0,"=================== Start Of The File ===================");
            ListLines.Add( "=================== End Of The File ===================");

            for (int i=0;i<Number_Of_Lines;i++) {
                rtbEditor.AppendText("=> "+ListLines[i]);
                rtbEditor.AppendText("\n");

                SuffixColumns.AppendText(i+ " ===== ");
                SuffixColumns.AppendText("\n");

                SuffixBox.AppendText(i+":");
                SuffixBox.AppendText("\n");
            }
            Current_Line = 1;
            Highlight(1);

        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Text Format (*.txt)|*.txt|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                //FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                lines = File.ReadAllLines(dlg.FileName);
                ListLines = new List<String>(lines);
                DDG = dlg;
                //TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                //lines = range.Text.Split('\n');
                
                Loader();
                //range.Load(fileStream, DataFormats.Rtf);
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ReWrite();
            for (int i = 0; i < ListLines.Count; i++) {
                ListLines[i] =  Regex.Replace(ListLines[i], @"\r\n?|\n", "");
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text Format (*.txt)|*.txt|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                using (var stream = File.OpenWrite(dlg.FileName))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    if (ListLines.Count > 0)
                    {
                        for (int i = 0; i < ListLines.Count - 1; i++)
                        {
                            writer.WriteLine(ListLines[i]);
                            
                        }
                        writer.Write(ListLines[ListLines.Count - 1]);
                    }
                }
            }
        }


        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
                rtbEditor.FontFamily =  (FontFamily)cmbFontFamily.SelectedItem;
                
        }

        private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            rtbEditor.FontSize = Convert.ToDouble( cmbFontSize.Text);
            
        }

 

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
           string text="<!!!> Save = Entered to terminal withourt argument, prompts for file select \n" +
                       "<!!!> Save_as = Entered to the terminal, will save changes to the opened file, overwrites the file \n" +
                       "<!!!> Open = Entered to terminal without argument, prompts for file select \n" +
                       "<!!!> Search = entered as:find searchedtext number, ex find as 4, searches the given text from setcl to setcl+number, the line will be highlighted \n " +
                       "search and clean is sensitive \n" +
                       "<!!!> # = once a number is entered skips to that line ex:4 , will go to the 4th line \n" +
                       "<!!!> Forward = Entered to terminal without argument, Will move a screenfull \n" +
                       "<!!!> Back = Entered to terminal without argument, Forward in backwards \n" +
                       "<!!!> Left # = Entered to terminal as left number, ex left 4.Will move to left if possible, may need big numbers for clear effect \n" +
                       "<!!!> Right # = Entered to terminal as right number, left in reverse" +
                       "<!!!> Up # = Moves up in the file ex, up 4 will move four lines+" +
                       "<!!!> Down # = Moves down in the file, Up in reverse" +
                       "<!!!> Setcl # = Sets the current line, highlights it, if there is any text and in the textbox ,ex setcl 3 \n" +
                       "<!!!> Help = in terminal calls this page \n" +
                       "<!!!> Change = entered as change oldtext newtext number, ex change is mama 5 will change the first found is to mama, from the current line to 5 down the file\n" +
                       "" +
                          " <Suffix> i# = inserts lines to the current suffix \n" +
                          " <Suffix> a = inserts line after the current line \n" +
                          " <Suffix> b = inserts line before the current line \n" +
                          " <Suffix> \" # \" = number given inside double quates will  duplicated the line (current line) ex \"3\" will duplicate the current line three times\n";
                       MessageBox.Show(text);

            string tex = "How to enter suffix commands\n" +
                "suffix commands are entered after the number: , ex is 0:<here>=====0+" +
                "suffix commands can not be deleted, once pressed enter it will be refreshed \n+" +
                "if command does not work it will do nothing, commands will work oinly in one direction";
                        MessageBox.Show(tex);
            tex = "< !!!> lines # = will change the shown number of lines, default is 10, will effect the future operations\n";
            MessageBox.Show(tex);
        }

        //Load line to terminal
        private void LoadLine() {

            int number;
           
            bool isNumeric = int.TryParse(words[1], out number);

            if (!isNumeric) {
                MessageBox.Show("Expected line Number", "Error");
                return;
            }

            if (number > ListLines.Count || number < 0) {
                MessageBox.Show("Entered line number does not exist "+number, "Error");
                return;
            }

            Current_Line = number;
            Highlight(number);

        }

        private Tuple<TextPointer, TextPointer> GetLine(int Number)
        {



            TextPointer resetpos = rtbEditor.Document.ContentStart;
            rtbEditor.CaretPosition = resetpos;


            TextPointer startPos = rtbEditor.CaretPosition.GetLineStartPosition(Number);
            TextPointer endPos = rtbEditor.CaretPosition.GetLineStartPosition(Number + 1);


            //Error


            return Tuple.Create(startPos, endPos);
        }

        private void Highlight(int Number){


            rtbEditor.Background = Brushes.White;
            TextRange clean =  new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            clean.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
            //clean
            var holder = GetLine(Number);
            //

            if (string.IsNullOrEmpty(ListLines[Number]))
            {
                return;
            }

            if (Number > Number_Of_Lines || Number < Current_Line) {
                return;
            }
            TextPointer startPos = holder.Item1;
            TextPointer endPos = holder.Item2;



            
            
            TextRange range = new TextRange(startPos, endPos);
            
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);

            //Highlight Suffix

            

        }

        private void Search() {
            

            string keyword = string.Join(" ",words);
            
            
            int until = Convert.ToInt32(words[words.Length-1]) +    Current_Line;

            keyword = keyword.Replace(words[0],"");
            keyword = keyword.Replace(words[words.Length- 1],"");
           
            
            keyword = keyword.Remove(0,1);
            keyword = keyword.Remove(keyword.Length - 1,1);
            
            //is not working as expected
            for (int index = Current_Line; index < until; index++) {

                if (index > Number_Of_Lines) {
                    return;
                }
               
                if (ListLines[index].Contains(keyword)) {
                    Highlight(index);
                    Current_Line = index;
                    
                    break;
                }
            }
 

            
            //Unable to find

        }

        private void Change()
        {


            string keyword = string.Join(" ", words);
            string replace = words[words.Length - 2];

            int until = Convert.ToInt32(words[words.Length - 1]) + Current_Line;

            keyword = keyword.Replace(words[0], "");
            keyword = keyword.Replace(words[words.Length - 1], "");
            keyword = keyword.Replace(words[words.Length - 2], "");

           

            keyword = keyword.Remove(0, 1);
            keyword = keyword.Remove(keyword.Length - 2,2);

            

            //is not working as expected
            for (int index = Current_Line; index < until; index++)
            {


                if (ListLines[index].Contains(keyword))
                {
                    
                    TextRange range;
                    ListLines[index] = ListLines[index].Replace(keyword, words[words.Length - 2])+ '\n';
                   
                    var holder = GetLine(index);
                    TextPointer startPos = holder.Item1;
                    TextPointer endPos = holder.Item2;
                    range = new TextRange(startPos, endPos);
                    range.Text = "=> "+ListLines[index];
                    Highlight(index);
                    Current_Line = index;

                    break;
                }
            }



            //Unable to find

        }



        private void Update(int start,int end) {
            CleanBoxData();

            if (end > ListLines.Count)
            {
                end = ListLines.Count;
                
            }

            if (end <= 0) {
                end = 2;
            }

            if (start >= end)
            {
                start = end - 1;
            }

            if (start < 0) {
                start = 0;
            }



            for (int i = start; i < end; i++)
            {
                rtbEditor.AppendText("=> "+ListLines[i]);
                rtbEditor.AppendText("\n");

                SuffixColumns.AppendText(i + " ===== ");
                SuffixColumns.AppendText("\n");

                SuffixBox.AppendText(i+":");
                SuffixBox.AppendText("\n");
            }
            
            //Highlight(0);//first item
        }

        private void LeftRight(int number)
        {
            offset += number;
            rtbEditor.ScrollToHorizontalOffset(offset);
        }

        private void ReWrite()
        {



            
            TextRange range;
            for ( int i=LineNumber; i < Number_Of_Lines ; i++) {

                if ( i == ListLines.Count-1) {
                    continue;
                }

                var holder = GetLine(i);
                TextPointer startPos = holder.Item1;
                TextPointer endPos = holder.Item2;
                range = new TextRange(startPos, endPos);

                string text = range.Text;
                text = text.Replace("=> ", "");
                

                
                ListLines[i] = text;
                

            }
            
            ListLines.RemoveAt(0);
            ListLines.RemoveAt(ListLines.Count - 1);
        }

        //Command Line
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Terminal.Text))
            {
                return;
            }

            words = Terminal.Text.Split(' ');


            int number;
            bool isNumeric = int.TryParse(words[0], out number);

            if (isNumeric) {//skip to line ////Iadasdasfaf
                Current_Line = number;
                Number_Of_Lines += Current_Line;
                Update(number,number+Number_Of_Lines);
                return;
            }

            words[0] = words[0].ToLower();
            string[] Line_Numbers = SuffixColumns.Text.Split(' ');
            LineNumber = Convert.ToInt32(Line_Numbers[0]);//Line Number at the start?


            try
            {
                switch (words[0])
                {
                    case "setcl":
                        LoadLine();
                        break;
                    case "save_as":
                        Button_Click(this, null);
                        break;
                    case "save":
                        Save_Executed(this, null);
                        break;
                    case "open":
                        Open_Executed(this, null);
                        break;
                    case "find":
                        Search();
                        break;
                    case "forward":
                        Update(LineNumber + Number_Of_Lines, LineNumber + 2 * Number_Of_Lines);
                        Number_Of_Lines += Number_Of_Lines;
                        break;
                    case "back":
                        Update(LineNumber - Number_Of_Lines, (LineNumber - Number_Of_Lines) + Number_Of_Lines);
                        Number_Of_Lines = (LineNumber - Number_Of_Lines) + Number_Of_Lines;
                        break;
                    case "up":
                        Update(LineNumber - Convert.ToInt32(words[1]), (LineNumber - Convert.ToInt32(words[1])) + Number_Of_Lines);
                        Number_Of_Lines = (LineNumber - Convert.ToInt32(words[1])) + Number_Of_Lines;
                        break;
                    case "down":
                        Update(LineNumber + Convert.ToInt32(words[1]), (LineNumber + Convert.ToInt32(words[1])) + Number_Of_Lines);
                        Number_Of_Lines = (LineNumber + Convert.ToInt32(words[1])) + Number_Of_Lines;
                        break;
                    case "left":
                        LeftRight(-1 * Convert.ToInt32(words[1]));//
                        break;
                    case "right":
                        LeftRight(Convert.ToInt32(words[1]));//
                        break;
                    case "help":
                        btnHelp_Click(this, null);
                        break;
                    case "change":
                        Change();
                        break;
                    case "lines":
                        Number_Of_Lines = (Convert.ToInt32(words[1]) );
                        break;

                    default:
                        MessageBox.Show("Command Not Understood");//change
                        break;



                }
            }
            catch(Exception ex) {
                MessageBox.Show("Format Error, see help");
            }
            Terminal.Text = "";




        }

        private void CleanBoxData() {
            
            rtbEditor.Document.Blocks.Clear();
            SuffixBox.Clear();
            SuffixColumns.Clear();
        }

        private void Suffix_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            String command = FindSuffix();
            LineNumber = command[0] - '0';
            string Command = command.Remove(0, 2);
            Command = Command.ToLower();
            char OP = Command[0];

            try
            {

                switch (OP)
                {
                    case 'a':
                        Insert(1, LineNumber + 1);
                        break;
                    case 'b':
                        Insert(1, LineNumber - 1);
                        break;
                    case 'i':
                        Command = Command.Remove(0, 1);
                        Insert(Convert.ToInt32(Command), LineNumber);
                        break;
                    case '"':
                        Command = Command.Remove(0, 1);
                        Command = Command.Remove(Command.Length - 1, 1);
                        Command = Command.Remove(Command.Length - 1, 1);
                        Copy(Convert.ToInt32(Command), LineNumber);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Format Error, see help");
            }
            SuffixBox.Clear();
            foreach (string text in SuffixCommands) {
                SuffixBox.AppendText(text);
                //SuffixBox.AppendText("\n");
            }

        }

        private string FindSuffix() {
            SuffixCommands = new List<String>();
            string text = "";
            bool found;
            bool flag = false;
            for (int i = 0; i < SuffixBox.LineCount; i++) {
                SuffixCommands.Add(SuffixBox.GetLineText(i));
                found = Regex.IsMatch(SuffixCommands[i], @"[a-zA-Z | "" ]");
                if (found && flag == false) {
                    text = SuffixCommands[i];
                    SuffixCommands[i] = i + ":"+"\n";
                    flag = true;
                    
                }

            }

            return text;
        }

        private void Insert(int amount,int index)
        {
            
            int start = SuffixCommands[0][0] - '0';
            int end = SuffixCommands[SuffixCommands.Count - 2][0] - '0';
            for (int i = 0; i < amount; i++) {
                ListLines.Insert(index," ");
                index++;
                
            }
            Update(start, end);
        }

        private void Copy(int amount, int index)
        {
            
            int start = SuffixCommands[0][0] - '0';
            int end = SuffixCommands[SuffixCommands.Count - 2][0] - '0';
            for (int i = 0; i < amount; i++)
            {
                ListLines.Insert(index, ListLines[index]);
                index++;

            }
            Update(start, end);
        }

        private void EmptyExe(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//Save as 
            ReWrite();
            
            for (int i = 0; i < ListLines.Count; i++)
            {
                ListLines[i] = Regex.Replace(ListLines[i], @"\r\n?|\n", "");
            }

            
            
                using (var stream = File.OpenWrite(DDG.FileName))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    if (ListLines.Count > 0)
                    {
                        for (int i = 0; i < ListLines.Count - 1; i++)
                        {
                            writer.WriteLine(ListLines[i]);

                        }
                        writer.Write(ListLines[ListLines.Count - 1]);
                    }
                }
            
        }

        //public ICommand EnterCommand() { get; set;  }
    }
}


/*search      future work       for (int index = 0; ; index += keyword.Length)
            {
                index = text.IndexOf(keyword, index);
                EndIndex = index + keyword.Length;




                if (index == -1)
                    break;


                MessageBox.Show("start:"+index+" end:"+ EndIndex);
                startPos = rtbEditor.CaretPosition.DocumentStart;
                for (int i = 0; i < index; i++) {
                    startPos = startPos.GetNextInsertionPosition(LogicalDirection.Forward);
                }

                endPos = rtbEditor.CaretPosition.DocumentStart;
                for (int i = 0; i < EndIndex; i++)
                {
                    endPos = endPos.GetNextInsertionPosition(LogicalDirection.Forward);
                }
                

                TextRange Founded = new TextRange(startPos, endPos);

                Founded.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
            }*/

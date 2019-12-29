using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
using EditorTools;
using Windows.UI.Text;

namespace Merdog_Windows
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            Lexer.init_lexer();
        }
        string input_buff;
        #region PAINT
        void paint_driver(int s, int l, Color c)
        {
            int saved_pos = Editor.Document.Selection.StartPosition;
            Editor.Document.Selection.SetRange(s, l + s);
            ChangeSectionColor(Editor.Document.Selection, c);
            Editor.Document.Selection.StartPosition = saved_pos;
            if(!Lexer.StringMode)
                Editor.Document.Selection.CharacterFormat.ForegroundColor = Colors.Black;
        }
        public void PaintText()
        {
            int startPos = Editor.Document.Selection.StartPosition;
            //int index = 0;
            string str = get_editor_content();
            Editor.Document.Selection.StartPosition=str.Length-1;
            Editor.Document.Selection.EndPosition = str.Length - 1;
            int startIndex = 0;
            MLexer lx = new MLexer(str);

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            List<Token> ls = lx.GetTokenStream();
            watch.Stop();
            //showInformation("刷漆成功", "时长:"+watch.ElapsedMilliseconds.ToString());
            foreach (var v in ls)
            {
                startIndex = v.startPos;
                int length = v.endPos - startIndex;
                switch (v.token_color)
                {
                    case Lexer.TokenColor.Default:
                        paint_driver(startIndex, length, Colors.Black);
                        break;
                    case Lexer.TokenColor.Blue:
                        paint_driver(startIndex, length, Colors.Blue);
                        break;
                    case Lexer.TokenColor.Wheat:
                        paint_driver(startIndex, length, Colors.Orange);
                        break;
                    case Lexer.TokenColor.Green:
                        paint_driver(startIndex, length, Colors.DarkGreen);
                        break;
                    case Lexer.TokenColor.Pink:
                        paint_driver(startIndex, length, Colors.DarkSeaGreen);
                        break;
                    case Lexer.TokenColor.DarkRed:
                        paint_driver(startIndex, length, Colors.DarkRed);
                        break;
                    case Lexer.TokenColor.Grey:
                        paint_driver(startIndex, length, Colors.Gray);
                        break;
                    case Lexer.TokenColor.Yellow:
                        paint_driver(startIndex, length, Colors.Gold);
                        break;
                }
            }
            Editor.Document.Selection.StartPosition = startPos;
            Editor.Document.Selection.EndPosition = startPos;
        }
        #endregion
        public async void showInformation(string title, string row)
        {
            ContentDialog textDia = new ContentDialog()
            {
                Title = title,
                Content = row,
                PrimaryButtonText = "Ok",
            };
            ContentDialogResult result = await textDia.ShowAsync();
        }
        public string get_editor_content()
        {
            string value = string.Empty;
            Editor.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out value);
            return value;
        }
        public void set_editor_content(string tmp)
        {
            Editor.Document.SetText(Windows.UI.Text.TextSetOptions.None, tmp);

        }
        private Windows.Storage.StorageFile current_file;
        char last_char;
        private async void open_file()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".mer");
            picker.FileTypeFilter.Add(".cc");
            picker.FileTypeFilter.Add(".cpp");
            picker.FileTypeFilter.Add(".c");
            current_file = await picker.PickSingleFileAsync();
            if (current_file != null)
            {
                SFileName.Text = current_file.Name;
                set_editor_content(await Windows.Storage.FileIO.ReadTextAsync(current_file));
            }
            else
            {
                showInformation("Error", "open filed failed");
            }
            PaintText();
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            open_file();
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            string tmp = "";
            MerdogRT.MerdogRunner rc = new MerdogRT.MerdogRunner(get_editor_content());
            rc.run();
            if (rc.have_error())
            {
                int line = rc.line_no();

                string str = get_editor_content();
                int index = 0;
                int line_count=1;
                while (index < str.Length && line_count < line) 
                { 
                    if (str[index] == '\n' || str[index] == '\r') 
                        line_count++;
                    index++; 
                }
                showInformation("ERROR",rc.get_error_content());
                for(int i = index; i < str.Length && str[i] != '\r' && str[i] != '\n'; i++)
                {
                    Editor.Document.Selection.SetRange(index, i+1);
                }
            }
            else
            {
                Classes.data.result += "\n" + rc.get_output_buf();
                Frame.Navigate(typeof(Pages.MerdogConsole));
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (current_file != null)
            {
                string value = string.Empty;
                Editor.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out value);
                await Windows.Storage.FileIO.WriteTextAsync(current_file, value);
                showInformation(current_file.Name, "保存成功");
            }
            else if (current_file == null)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation =
                    Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("merdog program", new List<string>() { ".mer" });
                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "new";

                current_file = await savePicker.PickSaveFileAsync();
                if (current_file != null)
                {
                    // Prevent updates to the remote version of the file until
                    // we finish making changes and call CompleteUpdatesAsync.
                    Windows.Storage.CachedFileManager.DeferUpdates(current_file);
                    // write to file
                    string value = string.Empty;
                    Editor.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out value);
                    await Windows.Storage.FileIO.WriteTextAsync(current_file, value);
                    // Let Windows know that we're finished changing the file so
                    // the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    Windows.Storage.Provider.FileUpdateStatus status =
                        await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(current_file);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        showInformation(current_file.Name, "保存文件成功");
                    }
                    else
                    {
                        showInformation(current_file.Name, "保存文件失败");
                    }
                }
                else
                {
                    showInformation("信息", "保存文件取消");
                }
            }
            if (current_file != null)
            {
                SFileName.Text = current_file.Name;
            }
        }
        private int tab_deepth = 0;
        private string generate_tab()
        {
            string ret = "\n";
            for (int i = 0; i < tab_deepth; i++)
                ret += '\t';
            return ret;
        }
        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("merdog program", new List<string>() { ".mer" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "new";

            current_file = await savePicker.PickSaveFileAsync();
            if (current_file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(current_file);
                // write to file
                string value = string.Empty;
                Editor.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out value);
                await Windows.Storage.FileIO.WriteTextAsync(current_file, value);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(current_file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    showInformation(current_file.Name, "保存文件成功");
                }
                else
                {
                    showInformation(current_file.Name, "保存文件失败");
                }
            }
            else
            {
                showInformation("信息", "保存文件取消");
            }
            if (current_file != null)
            {
                SFileName.Text = current_file.Name;
            }
        }

        private void Editor_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            RichEditBox richEditBox = sender as RichEditBox;
            if (richEditBox == null)
                return;
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                    richEditBox.Document.Selection.TypeText(generate_tab());
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Tab:
                    tab_deepth++;
                    richEditBox.Document.Selection.TypeText("\t");
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Back:
                    if (last_char == '\t' && tab_deepth > 0)
                        tab_deepth--;
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Space:
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.F1:
                    PaintText();
                    break;
                case Windows.System.VirtualKey.Shift:
                case Windows.System.VirtualKey.Control:
                    e.Handled = true;
                    break;
                default:
                    break;
            }


        }
        private void ChangeSectionColor(ITextSelection selectedText, Color color)
        {
            //ITextSelection selectedText = Editor.Document.Selection;
            if (selectedText != null)
            {
                selectedText.CharacterFormat.ForegroundColor = color;
            }
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.FAQs));
        }

        private void Editor_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (Editor.Document.Selection.StartPosition - 1 >= 0)
                last_char = get_editor_content()[Editor.Document.Selection.StartPosition - 1];
        }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}

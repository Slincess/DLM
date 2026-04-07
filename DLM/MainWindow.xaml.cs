using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Path = System.IO.Path;


namespace DLM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        LinksClass links = new();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            LoadLinks();

            Left = SystemParameters.WorkArea.Width - Width;
            Top = 0;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW;
            exStyle &= ~WS_EX_APPWINDOW;

            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        // drag window
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (LinkCreator.IsVisible) LinkCreator.Visibility = Visibility.Collapsed;
            else LinkCreator.Visibility = Visibility.Visible;
        }

        private void DoneButton(object sender, RoutedEventArgs e)
        {
            CreateLinkButton(Link_Text_box.Text, Name_Text_Box.Text);
            LinkCreator.Visibility = Visibility.Collapsed;
            ButtonL btnLink = new();
            btnLink.Links = Link_Text_box.Text;
            btnLink.Name = Name_Text_Box.Text; ;
            links.Linki.Add(btnLink);
            Name_Text_Box.Text = "name";
            Link_Text_box.Text = "link";
            SaveLinks();
        }

       private void CreateLinkButton(string Link,string Name )
       {
            Button btn = new();
            btn.Height = 60;
            btn.Margin = new Thickness(0, 15, 0, 15);
            btn.Tag = Link;
            btn.Click += openLink;
            btn.Content = Name;
            Button_Stackpanel.Children.Add(btn);
        }

        private void openLink(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            var url = btn.Tag as string;
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // log or show a user-friendly message
            }
        }

        private void LoadLinks()
        {
            string fileName = "Links.json";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(filePath))
            {
                links = JsonSerializer.Deserialize<LinksClass>(File.ReadAllText(filePath)) ?? new();
                if (links != null) {

                    foreach (var i in links.Linki)
                    {
                        CreateLinkButton(i.Links, i.Name);
                    }
                }
            }
            else
            {
                using FileStream createStream = File.Create(filePath);
                JsonSerializer.SerializeAsync(createStream, links);
            }
        }

        private void SaveLinks()
        {
            string fileName = "Links.json";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            string json = JsonSerializer.Serialize(links);
            File.WriteAllTextAsync(filePath, json);
        }

    }
}

public class LinksClass
{
    public List<ButtonL>? Linki { get; set; } = new();
}

public struct ButtonL
{
    public string Links { get; set; }
    public string Name { get; set; }
}
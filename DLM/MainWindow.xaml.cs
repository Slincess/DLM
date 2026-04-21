using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
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
using System.Xml.Serialization;
using Path = System.IO.Path;


namespace DLM
{
    public partial class MainWindow : Window
    {

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        CategoryList Categories = new();
        public Category DisplatedCategory = new Category();
        public Button SelectedButton;

        public Buttoninfo EditingButton;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            LoadLinks();
            LoadCategories();
            pingTest();
            

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

        #region Load Funcs

        //Loads the buttons in the Category to Button_Stackpanel
        private void LoadCategoryList(Category Category)
        {
            try
            {
                DisplatedCategory = Category;
                Button_Stackpanel.Children.Clear();
                foreach (var i in DisplatedCategory.Links.ToArray())
                {
                    CreateLinkButton(i.Links, i.Name,i);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        //Loads the Categories to CategoriePanel 
        private void LoadCategories()
        {
            int index = 0;
            try
            {
                foreach (var i in Categories.CatagoriesList)
                {
                    CreateCategoryButton(i.NameCategory, index);
                    index++;
                }
                if (CategoriePanel.Children.Count > 0)
                {
                    Button btn = (Button)CategoriePanel.Children[1];
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF454545"));
                    SelectedButton = btn;
                    LoadCategoryList(Categories.CatagoriesList[0]);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
        #endregion

        #region Create Funcs
        private void CreateCategoryButton(string categoryName, int index)
        {
            try
            {
                Button btn = new()
                {
                    Height = 60,
                    Width = 60,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(3, 0, 3, 0),
                    Content = categoryName,
                    Tag = index,
                    Background = new SolidColorBrush(Color.FromRgb(79, 79, 79)),
                    Foreground = Brushes.WhiteSmoke
                };
                btn.Click += CategoryButtonClicked;
                CategoriePanel.Children.Add(btn);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        //creates btn without making a new buttoninfo if it already exist
        private void CreateLinkButton(string Link, string Name,Buttoninfo info)
        {
            try
            {
                LinkButton btn = new();
                Button LinkBtn = btn.LinkOpenButton;
                Button EditBtn = btn.EditButton;
                LinkBtn.Tag = Link;
                LinkBtn.Content = Name;
                LinkBtn.Click += OpenLink;
                EditBtn.Click += OpenEditPanel;
                EditBtn.Tag = btn;
                btn.Margin = new Thickness(0, 6, 0, 7);
                btn.thisButtonInfo = info;
                Button_Stackpanel.Children.Add(btn);
                PingLinks(btn.thisButtonInfo);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        //creates btn making a new buttoninfo
        private void CreateLinkButton(string Link, string Name)
        {
            try
            {
                LinkButton btn = new();
                Button LinkBtn = btn.LinkOpenButton;
                Button EditBtn = btn.EditButton;
                LinkBtn.Tag = Link;
                LinkBtn.Content = Name;
                LinkBtn.Click += OpenLink;
                EditBtn.Click += OpenEditPanel;
                EditBtn.Tag = btn;
                btn.Margin = new Thickness(0, 6, 0, 7);
                Buttoninfo btninfo = new();
                btninfo.Links = Link_Text_box.Text;
                btninfo.Name = Name_Text_Box.Text;
                btn.thisButtonInfo = btninfo;
                DisplatedCategory.Links.Add(btn.thisButtonInfo);
                Button_Stackpanel.Children.Add(btn);
                PingLinks(btn.thisButtonInfo);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void CreateCategory(string Name)
        {
            try
            {
                Category categoryNew = new();
                categoryNew.NameCategory = Name;
                Categories.CatagoriesList.Add(categoryNew);
                CreateCategoryButton(Name, Categories.CatagoriesList.Count);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #endregion

        #region Click Events
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (LinkCreator.IsVisible) LinkCreator.Visibility = Visibility.Collapsed;
            else LinkCreator.Visibility = Visibility.Visible;

            EditPanel.Visibility = Visibility.Collapsed;
        }

        private void DoneButton(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsCategoryCheck.IsChecked.Value)
                {
                    CreateCategory(Name_Text_Box.Text);
                }
                else
                {
                    CreateLinkButton(Link_Text_box.Text, Name_Text_Box.Text);
                    LinkCreator.Visibility = Visibility.Collapsed;
                }
                Name_Text_Box.Text = "name";
                Link_Text_box.Text = "link";
                IsCategoryCheck.IsChecked = false;
                LinkCreator.Visibility = Visibility.Collapsed;
                Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void CategoryButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                SelectedButton.Background = btn.Background;
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF454545"));
                btn.Foreground = Brushes.WhiteSmoke;
                SelectedButton = btn;
                LoadCategoryList(Categories.CatagoriesList[(int)btn.Tag]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            LinkCreator.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Collapsed;
        }

        private void OpenLink(object sender, RoutedEventArgs e)
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
            }
        }

        private void IsCategory(object sender, RoutedEventArgs e)
        {
            Link_Text_box.IsEnabled = IsCategoryCheck.IsChecked.Value;
        }

        private void OpenEditPanel(object sender, RoutedEventArgs e)
        {
            if (EditPanel.IsVisible) EditPanel.Visibility = Visibility.Collapsed;
            else EditPanel.Visibility = Visibility.Visible;
            LinkCreator.Visibility = Visibility.Collapsed;


            Button btn = (Button)sender;
            LinkButton linkbtn = (LinkButton)btn.Tag;
            Edit_Link_Box.Text = linkbtn.GetLinkButton().Tag.ToString();
            Edit_Name_Box.Text = linkbtn.GetLinkButton().Content.ToString();
            EditPanel.Tag = linkbtn;
        }

        private void EditDone(object sender, RoutedEventArgs e)
        {
            LinkButton linkbtn = (LinkButton)EditPanel.Tag;
            linkbtn.Edit(Edit_Link_Box.Text,Edit_Name_Box.Text);
            Edit_Link_Box.Text = "link";
            Edit_Name_Box.Text = "name";
            EditPanel.Visibility = Visibility.Collapsed;
            PingLinks(linkbtn.thisButtonInfo);
            Save();
        }

        #endregion

        private async Task PingLinks(Buttoninfo btn)
        {
            try
            {
                using (Ping pingSender = new Ping())
                {

                    string host;

                    if (Uri.TryCreate(btn.Links, UriKind.Absolute, out Uri uri))
                    {
                        host = uri.Host;
                    }
                    else
                    {
                        host = btn.Links;
                    }

                    PingReply reply = await pingSender.SendPingAsync(host);



                    if (reply.Status == IPStatus.Success)
                    {
                        await btn.isOnlineindicator.Dispatcher.InvokeAsync(() =>
                        {
                            btn.isOnlineindicator.Background = Brushes.Green;
                            
                        });
                        
                    }
                    else
                    {
                        await btn.isOnlineindicator.Dispatcher.InvokeAsync(() =>
                        {
                            btn.isOnlineindicator.Background = Brushes.Red;
                            
                        });
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await btn.isOnlineindicator.Dispatcher.InvokeAsync(() =>
                {
                    btn.isOnlineindicator.Background = Brushes.Red;
                });
                return;
            }
        }

        private async Task pingTest()
        {
            try
            {
                foreach (var category in Categories.CatagoriesList.ToArray())
                {
                    foreach (var link in category.Links.ToArray())
                    {
                        await Task.Run(() => PingLinks(link));
                        Debug.WriteLine(link.Links + "  pinging");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            await Task.Delay(300000);
            pingTest();
        }

        #region Save Load Json
        private void LoadLinks()
        {
            string fileName = "Links.json";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(filePath))
            {
                Categories = JsonSerializer.Deserialize<CategoryList>(File.ReadAllText(filePath)) ?? new();
            }
            else
            {
                using FileStream createStream = File.Create(filePath);
                JsonSerializer.SerializeAsync(createStream, Categories);
            }
        }

        private void Save()
        {
            string fileName = "Links.json";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            string json = JsonSerializer.Serialize(Categories);
            File.WriteAllTextAsync(filePath, json);
        }
        #endregion

    }
}

public class Category
{
    public List<Buttoninfo>? Links { get; set; } = new();
    public string NameCategory { get; set; }
}

public class CategoryList
{
    public List<Category>? CatagoriesList { get; set; } = new();
}

public class Buttoninfo
{
    public string Links { get; set; }
    public string Name { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public Border isOnlineindicator { get; set; }
}
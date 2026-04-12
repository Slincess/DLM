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

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            LoadLinks();
            LoadCategories();
            

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
                    ButtonL btnLink = new();
                    btnLink.Links = Link_Text_box.Text;
                    btnLink.Name = Name_Text_Box.Text; ;
                    DisplatedCategory.Links.Add(btnLink);
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

       private void CreateLinkButton(string Link,string Name)
       {
            try
            {
                LinkButton btn = new();
                Button LinkBtn = btn.LinkOpenButton;
                Button EditBtn = btn.EditButton;
                LinkBtn.Tag = Link;
                LinkBtn.Content = Name;
                LinkBtn.Click += openLink;
                btn.Margin = new Thickness(0,6,0,7);
                Button_Stackpanel.Children.Add(btn);
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
                CreateCategoryButton(Name,Categories.CatagoriesList.Count);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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
            }
        }

        private void LoadCategoryList(Category Category)
        {
            try
            {
                DisplatedCategory = Category;
                Button_Stackpanel.Children.Clear();
                foreach (var i in DisplatedCategory.Links)
                {
                    CreateLinkButton(i.Links, i.Name);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void LoadCategories()
        {
            int index = 0;
            try
            {
                foreach (var i in Categories.CatagoriesList)
                {
                    CreateCategoryButton(i.NameCategory,index);
                    index++;
                }
                if (CategoriePanel.Children.Count > 0)
                {
                    Button btn = (Button)CategoriePanel.Children[1];
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC"));
                    SelectedButton = btn;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void CreateCategoryButton(string categoryName,int index)
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
                    Tag = index
                };
                btn.Click += CategoryButtonClicked;
                CategoriePanel.Children.Add(btn);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void CategoryButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                SelectedButton.Background = btn.Background;
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC"));
                SelectedButton = btn;
                LoadCategoryList(Categories.CatagoriesList[(int)btn.Tag]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

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

        private void IsCategory(object sender, RoutedEventArgs e)
        {
            Link_Text_box.IsEnabled = IsCategoryCheck.IsChecked.Value;
        }
    }
}

public class Category
{
    public List<ButtonL>? Links { get; set; } = new();
    public string NameCategory { get; set; }
}

public class CategoryList
{
    public List<Category>? CatagoriesList { get; set; } = new();
}

public struct ButtonL
{
    public string Links { get; set; }
    public string Name { get; set; }
}
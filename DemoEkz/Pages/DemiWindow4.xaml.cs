
using DemoEkz.AdminPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoEkz.Pages
{
    /// <summary>
    /// Логика взаимодействия для DemiWindow4.xaml
    /// </summary>
    public partial class DemiWindow4 : Window
    {

        private ImageSource selectedCaptchaImage = null;
        private int selectedCaptchanNumber = 0;

        private Random random = new Random();


        public DemiWindow4()
        {
            InitializeComponent();
            LoadCaptcha();
        }
        private User GetUserByLogin(string login)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                return db.Users.Include("Role").AsEnumerable().FirstOrDefault(x => x.Login == login);
            }
        }
        private void LoadCaptcha()
        {
            Border[] selectedBorders = new Border[] { part1, part2, part3, part4 };
            Image[] selectedImages = new Image[] { partImage1, partImage2, partImage3, partImage4 };

            ClearCaptcha();
            ClearBorders();

            int[] numbers = new int[] { 1, 2, 3, 4 };

            numbers = numbers.OrderBy(x => random.Next()).ToArray();

            for (int i = 0; i < selectedBorders.Length; i++)
            {
                int number = numbers[i];

                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $"\\Captcha\\png{number}.png");

                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                selectedBorders[i].Tag = number;
                selectedImages[i].Source = bitmap;

                selectedBorders[i].BorderBrush = Brushes.LightGray;
                selectedBorders[i].BorderThickness = new Thickness(1);
            }
        }
        private void ClearCaptcha()
        {
            slot1.Tag = null;
            slot2.Tag = null;
            slot3.Tag = null;
            slot4.Tag = null;

            slotImage1 = null;
            slotImage2 = null;
            slotImage3 = null;
            slotImage4 = null;
        }

        private void ClearBorders()
        {

            Border[] selectedBorders = new Border[] { part1, part2, part3, part4 };

            foreach (Border border in selectedBorders)
            {
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = Brushes.LightGray;
            }
        }
        private void Slot_Click(object sender, MouseButtonEventArgs e)
        {
            Border selectedBorder = sender as Border;

            if (selectedBorder == null)
                return;

            Grid grid = selectedBorder.Child as Grid;

            foreach (var el in grid.Children)
            {
                if (el is Image)
                {
                    Image image = el as Image;
                    image.Source = selectedCaptchaImage;
                    selectedBorder.Tag = selectedCaptchanNumber;
                    break;
                }
            }
        }

        private void Part_Click(object sender, MouseButtonEventArgs e)
        {
            Border selectedBorders = sender as Border;

            if (selectedBorders == null)
                return;

            Image selectedImage = selectedBorders.Child as Image;

            if (selectedImage == null) return;

            ClearBorders();

            selectedCaptchaImage = selectedImage.Source;
            selectedCaptchanNumber = (int)selectedBorders.Tag;

            selectedBorders.BorderThickness = new Thickness(3);
            selectedBorders.BorderBrush = Brushes.Blue;
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            User user = GetUserByLogin(loginTextBox.Text.Trim());

            if (user == null) return;

            else
            {
                bool canAuth = true;
                using (DatabaseContext db = new DatabaseContext())
                {

                    if (!IsCaptchaCorrect())
                        canAuth = false;
                    if (user.Password != passBox.Password.Trim())
                        canAuth = false;

                    if (canAuth)
                    {
                        if (user.IsBlocked)
                        {
                            MessageBox.Show("U has been blocked");
                        }
                        else
                        {
                            if (user.Role.Name == "Пользователь")
                            {
                                MessageBox.Show("Пользователь");
                            }
                            else if (user.Role.Name == "Администратор")
                            {
                                MessageBox.Show("Администратор");
                                AdminWindow adminWindow = new AdminWindow();
                                adminWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Менеджер");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Password or captcha is incorrect");
                        db.Users.AsEnumerable().FirstOrDefault(x => x.Login == user.Login).FailledAttemp++;
                        db.SaveChanges();

                        if (user.FailledAttemp > 2)
                        {
                            db.Users.AsEnumerable().FirstOrDefault(x => x.Login == user.Login).IsBlocked = true;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private bool IsCaptchaCorrect()
        {
            if (slot1.Tag?.ToString() == "1" &&
                slot2.Tag?.ToString() == "2" &&
                slot3.Tag?.ToString() == "3" &&
                slot4.Tag?.ToString() == "4")
            {
                return true;
            }
            return false;
        }
        private void ResortButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCaptcha();
        }
    }
}

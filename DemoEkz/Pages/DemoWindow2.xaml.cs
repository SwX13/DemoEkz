using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Логика взаимодействия для DemoWindow2.xaml
    /// </summary>
    public partial class DemoWindow2 : Window
    {
        private ImageSource selectedCaptchaImage = null;
        private int selectedCapcthaNumber = 0;

        private Random random = new Random();

        public DemoWindow2()
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

        private void ClearSlots()
        {
            slotImage1.Source = null;
            slotImage2.Source = null;
            slotImage3.Source = null;
            slotImage4.Source = null;

            slot1.Tag = null;
            slot2.Tag = null;
            slot3.Tag = null;
            slot4.Tag = null;
        }
        private void ClearBorders()
        {
            Border[] borders = new Border[] { part1, part2, part3, part4 };
            foreach (var br in borders)
            {
                br.BorderBrush = Brushes.LightGray;
                br.BorderThickness = new Thickness(1);
            }
        }

        private void LoadCaptcha()
        {
            Border[] partBorders = new Border[] { part1, part2, part3, part4 };
            Image[] partImages = new Image[] { partImage1, partImage2, partImage3, partImage4 };

            ClearBorders();
            ClearSlots();
            int[] numbers = new int[] { 1, 2, 3, 4 };

            numbers = numbers.OrderBy(x => random.Next()).ToArray();

            for (int i = 0; i < partBorders.Length; i++)
            {
                int number = numbers[i];
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $".\\Captcha\\png{number}.png");

                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                partImages[i].Source = bitmap;
                partBorders[i].Tag = number;

                partBorders[i].BorderBrush = Brushes.LightGray;
                partBorders[i].BorderThickness = new Thickness(1);
            }
        }

        private void Slot_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCaptchaImage == null || selectedCapcthaNumber == 0)
            {
                MessageBox.Show("At first pick fragment");
                return;
            }

            Border selectedBorder = sender as Border;

            if (selectedBorder == null)
                return;

            Grid grid = selectedBorder.Child as Grid;

            if (grid == null)
                return;

            foreach (var element in grid.Children)
            {
                if (element is Image)
                {
                    Image image = element as Image;
                    image.Source = selectedCaptchaImage;
                    selectedBorder.Tag = selectedCapcthaNumber;
                    break;
                }
            }
        }

        private void Part_Click(object sender, MouseButtonEventArgs e)
        {
            Border selectedBorder = sender as Border;

            if (selectedBorder == null)
                return;

            Image selectedImage = selectedBorder.Child as Image;

            if (selectedImage == null)
                return;

            ClearBorders();

            selectedCaptchaImage = selectedImage.Source;
            selectedCapcthaNumber = (int)selectedBorder.Tag;

            selectedBorder.BorderBrush = Brushes.Blue;
            selectedBorder.BorderThickness = new Thickness(3);

        }
        private bool IsCaptchaCorrect()
        {
            if (slot1.Tag != null && slot1.Tag.ToString() == "1" &&
                slot2.Tag != null && slot2.Tag.ToString() == "2" &&
                slot3.Tag != null && slot3.Tag.ToString() == "3" &&
                slot4.Tag != null && slot4.Tag.ToString() == "4")
                return true;
            else
                return false;
        }
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            User user = GetUserByLogin(loginTextBox.Text.Trim());
            bool canAuth = true;

            if (user != null)
            {
                if (user.Password != passwordBox.Password.Trim())
                    canAuth = false;
                if (!IsCaptchaCorrect())
                    canAuth = false;

                if (!user.IsBlocked)
                {
                    if (!canAuth)
                    {
                        MessageBox.Show("Password or captcha isnt correct!");
                        using (DatabaseContext db = new DatabaseContext())
                        {
                            User currentUser = db.Users.AsEnumerable().FirstOrDefault(x => x.Login == user.Login);
                            currentUser.FailledAttemp++;
                            db.SaveChanges();

                            if (currentUser.FailledAttemp > 2)
                            {
                                currentUser.IsBlocked = true;
                                db.SaveChanges();
                            }
                        }
                    }
                    else // Complete Auth
                    {
                        MessageBox.Show("Aurh complete");
                    }
                }
                else
                {
                    MessageBox.Show("U has been banned. Please meil Admin");
                }
            }
        }
    }
}

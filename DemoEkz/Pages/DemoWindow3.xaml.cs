using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для DemoWindow3.xaml
    /// </summary>
    public partial class DemoWindow3 : Window
    {
        private int selectedCaptchaNumber = 0;
        private ImageSource selectedCaptchaImage = null;

        private Random random = new Random();


        public DemoWindow3()
        {
            InitializeComponent();

            LoadCapthca();
        }
        private void LoadCapthca()
        {
            selectedCaptchaImage = null;
            selectedCaptchaNumber = 0;
            Border[] partBorders = new Border[] { part1, part2, part3, part4 };
            Image[] partImages = new Image[] { partImage1, partImage2, partImage3, partImage4 };

            ClearBorder();
            ClearCapthca();

            int[] numbers = new int[] { 1, 2, 3, 4 };

            numbers = numbers.OrderBy(x => random.Next()).ToArray();
            for (int i = 0; i < partBorders.Length; i++)
            {
                int number = numbers[i];
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $"\\Captcha\\png{number}.png");

                if (!File.Exists(filePath))
                    throw new Exception("File not founded");

                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                partBorders[i].Tag = number;
                partImages[i].Source = bitmap;

                partBorders[i].BorderBrush = Brushes.LightGray;
                partBorders[i].BorderThickness = new Thickness(1);

            }
        }

        private void Slot_Click(object sender, MouseButtonEventArgs e)
        {

            if (selectedCaptchaImage == null || selectedCaptchaNumber == 0)
                return;

            Border selectedBorder = sender as Border;

            if (selectedBorder == null)
                return;

            Grid grid = selectedBorder.Child as Grid;

            if (grid == null)
                return;

            foreach (var child in grid.Children)
            {
                if (child is Image)
                {
                    Image image = child as Image;
                    image.Source = selectedCaptchaImage;
                    selectedBorder.Tag = selectedCaptchaNumber;
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

            ClearBorder();

            selectedCaptchaImage = selectedImage.Source;
            selectedCaptchaNumber = (int)selectedBorder.Tag;


            selectedBorder.BorderBrush = Brushes.Blue;
            selectedBorder.BorderThickness = new Thickness(3);
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            User user = GetUserByLogin(loginTextBox.Text.Trim());

            if (user == null)
            {
                MessageBox.Show("User not founede");
            }
            else
            {using (DatabaseContext db = new DatabaseContext())
                {
                    bool canAuth = true;

                    if (user.Password != passBox.Password.Trim())
                    {
                        Console.WriteLine("PASSWORD");
                        canAuth = false;
                    }
                    if (!CaptchaIsCorrect())
                    {
                        canAuth = false;
                        Console.WriteLine("CAPTCHA");
                    }

                    if (user.IsBlocked)
                    {
                        MessageBox.Show("User is blocked");
                    }
                    else
                    {
                        if (!canAuth)
                        {
                            db.Users.AsEnumerable().FirstOrDefault(x => x.Login == user.Login).FailledAttemp++;
                            db.SaveChanges();
                            MessageBox.Show("Incorrect password or captcha!");
                        }
                        else
                        {
                            MessageBox.Show($"User {user.Login} succeseful auth");
                            db.Users.AsEnumerable().FirstOrDefault(x => x.Login == user.Login).FailledAttemp = 0;
                            db.SaveChanges();
                        }
                        if (user.FailledAttemp > 2)
                        {
                            db.Users.AsEnumerable().FirstOrDefault(x => x.Login == user.Login).IsBlocked = true;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private bool CaptchaIsCorrect()
        {
            if (slot1.Tag?.ToString() == "1" &&
                 slot2.Tag?.ToString() == "2" &&
                 slot3.Tag?.ToString() == "3" &&
                 slot4.Tag?.ToString() == "4")
            {
                return true;
            }
            else
            {
                Border[] partBorders = new Border[] { part1, part2, part3, part4 };
                foreach (var br in partBorders)
                {
                    Console.WriteLine($"{br.Name} {br.Tag}");
                }
                return false;
            }
        }

        private User GetUserByLogin(string login)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                return db.Users.AsEnumerable().FirstOrDefault(x => x.Login == login);
            }
        }
        private void ClearCapthca()
        {
            partImage1.Source = null;
            partImage2.Source = null;
            partImage3.Source = null;
            partImage4.Source = null;

            slot1.Tag = null;
            slot2.Tag = null;
            slot3.Tag = null;
            slot4.Tag = null;
        }
        private void ClearBorder()
        {
            Border[] borders = new Border[] { part1, part2, part3, part4 };
            foreach (Border b in borders)
            {
                b.BorderThickness = new Thickness(1);
                b.BorderBrush = Brushes.LightGray;
            }
        }
    }
}

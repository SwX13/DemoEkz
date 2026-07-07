using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для DemoWindow.xaml
    /// </summary>
    public partial class DemoWindow : Window
    {
        public static DatabaseContext DBContext = new DatabaseContext();

        private string captchaPath = "./Capthca";

        private int selectedCaptchaNumber = 0;
        private ImageSource selectedCaptchaImage = null;

        private Random random = new Random();
        public DemoWindow()
        {
            InitializeComponent();
            LoadCaptcha();
        }
        private User GetUserByLogin(string login)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                User user = db.Users.Include("Role").AsEnumerable().FirstOrDefault(x => x.Login == login);
                return user;
            }
        }
        private bool IsCaptchaCorrect()
        {
            if (slot1.Tag != null && slot1.Tag.ToString() == "1" &&
                slot2.Tag != null && slot2.Tag.ToString() == "2" &&
                slot3.Tag != null && slot3.Tag.ToString() == "3" &&
                slot4.Tag != null && slot4.Tag.ToString() == "4")
                return true;
            else
            {
                Console.WriteLine($"{slot1.Tag} {slot2.Tag} {slot3.Tag} {slot4.Tag}");
                return false;
            }

        }

        private void Part_Click(object sender, MouseButtonEventArgs e)
        {
            Border selectedBorder = sender as Border;

            if (selectedBorder == null || selectedBorder.Tag == null)
                return;

            Image image = selectedBorder.Child as Image;

            if (image == null || image.Source == null)
                return;

            selectedCaptchaNumber = (int)selectedBorder.Tag;
            selectedCaptchaImage = image.Source;

            ClearBorders();

            selectedBorder.BorderBrush = Brushes.Blue;
            selectedBorder.BorderThickness = new Thickness(3);
        }

        private void ClearBorders()
        {
            Border[] borders = new Border[] { part1, part2, part3, part4 };

            foreach (Border b in borders)
            {
                b.BorderBrush = Brushes.LightGray;
                b.BorderThickness = new Thickness(1);
            }
        }

        private void Slot_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCaptchaNumber == 0 || selectedCaptchaImage == null)
            {
                MessageBox.Show("At first pick a part");
                return;
            }

            Border selectedBorder = sender as Border;

            if (selectedBorder == null)
                return;

            Grid grid = selectedBorder.Child as Grid;

            Image slotImage = null;

            foreach (var child in grid.Children)
            {
                if (child is Image)
                {
                    slotImage = child as Image;
                    break;
                }
            }

            if (slotImage == null)
                return;

            slotImage.Source = selectedCaptchaImage;

            selectedBorder.Tag = selectedCaptchaNumber;
        }

        private void LoadCaptcha()
        {
            selectedCaptchaImage = null;
            selectedCaptchaNumber = 0;

            ClearSlots();
            ClearBorders();

            Border[] partBorders = new Border[] { part1, part2, part3, part4 };
            Image[] partImages = new Image[] { partImage1, partImage2, partImage3, partImage4 };

            int[] numbers = new int[] { 1, 2, 3, 4 };

            numbers = numbers.OrderBy(x => random.Next()).ToArray();

            for (int i = 0; i < partBorders.Length; i++)
            {
                int number = numbers[i];

                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $"Captcha\\png{number}.png");

                if (!File.Exists(filePath))
                {
                    throw new Exception($"file is not founded {filePath}");
                }

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();

                bitmapImage.UriSource = new Uri(filePath);
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                partImages[i].Source = bitmapImage;
                partBorders[i].Tag = number;

                partBorders[i].BorderThickness = new Thickness(1);
                partBorders[i].IsEnabled = true;
                partBorders[i].Opacity = 1;


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

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            User user = GetUserByLogin(loginTextBox.Text.Trim());
            bool canAuth = true;

            if (user != null)
            {
                if (user.Password != passBox.Password.Trim())
                {
                    canAuth = false;
                    Console.WriteLine($"{passBox.Password} != {user.Password}");
                }
                if (!IsCaptchaCorrect())
                {
                    canAuth = false;
                    Console.WriteLine("Incorrect captcha");
                }


                if (!user.IsBlocked)
                {
                    if (canAuth)
                    {
                        MessageBox.Show("Auth complete");
                        ResetFilledAttemp(user);
                    }
                    else
                    {
                        using (DatabaseContext db = new DatabaseContext())
                        {
                            User currnetUser = db.Users.AsEnumerable().Where(x => x.Login == user.Login).FirstOrDefault();
                            currnetUser.FailledAttemp++;
                            db.SaveChanges();

                            if (currnetUser.IsBlocked == false && currnetUser.FailledAttemp > 2)
                            {
                                Console.WriteLine($"{currnetUser.Login} is Blocked");

                                currnetUser.IsBlocked = true;
                                db.SaveChanges();
                            }
                            Console.WriteLine(currnetUser.FailledAttemp);
                        }
                        MessageBox.Show("Incorrect login, password or captcha");
                    }
                }
                else
                {

                    MessageBox.Show("U has been Blocked. Mail Admin");
                }
            }
            else
            {
                MessageBox.Show("User not found");
            }
        }

        private void ResetFilledAttemp(User user)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                db.Users.AsEnumerable().FirstOrDefault(x => x.Login == user.Login).FailledAttemp = 0;
                db.SaveChanges();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadCaptcha();
        }
    }
}

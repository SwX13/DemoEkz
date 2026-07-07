using DemoEkz.Pages;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoEkz
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static DatabaseContext DBContext = new DatabaseContext();
        public static Frame Frame;

        // Captcha
        private string captchaPath = "./Captcha";

        private int selectedCaptchaNumber = 0;
        private ImageSource selectedCaptchaImage = null;

        private Random random = new Random();




        public MainWindow()
        {
            InitializeComponent();

            LoadCaptcha();
            Frame = MainFrame;
        }

        private void LoadCaptcha()
        {
            selectedCaptchaNumber = 0;
            selectedCaptchaImage = null;

            // Очищаем итоговую область капчи
            ClearSlots();

            // Массивы с Border-элементами, в которых лежат фрагменты
            Border[] partBorders = { part1, part2, part3, part4 };

            // Массивы с Image-элементами, куда загружаются картинки
            Image[] partImages = { partImage1, partImage2, partImage3, partImage4 };

            // Правильные номера фрагментов
            int[] numbers = { 1, 2, 3, 4 };

            // Перемешиваем номера, чтобы капча каждый раз была в другом порядке
            numbers = numbers.OrderBy(x => random.Next()).ToArray();

            // Загружаем картинки в перемешанном порядке
            for (int i = 0; i < partBorders.Length; i++)
            {
                int number = numbers[i];

                // Формируем путь к файлу, например:
                // C:\...\1.png
                string filePath = System.IO.Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "Captcha",
    $"png{number}.png");
                Console.WriteLine(filePath);
                // Проверяем, существует ли файл
                if (!File.Exists(filePath))
                {
                    
                    MessageBox.Show("Не найден файл капчи: " + filePath);
                    return;
                }

                // Создаем изображение для WPF
                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();

                // Указываем путь к картинке
                bitmap.UriSource = new Uri(filePath);

                // Картинка полностью загружается в память,
                // чтобы файл не был заблокирован системой
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                bitmap.EndInit();

                // Freeze делает изображение безопасным для использования
                // и немного улучшает производительность
                bitmap.Freeze();

                // Помещаем картинку в Image
                partImages[i].Source = bitmap;

                // В Tag записываем правильный номер картинки.
                // Например, если это 3.png, то Tag = 3.
                // Потом по Tag будет проверяться правильность сборки.
                partBorders[i].Tag = number;

                // Возвращаем обычный внешний вид фрагмента
                partBorders[i].BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225));
                partBorders[i].BorderThickness = new Thickness(1);
                partBorders[i].Opacity = 1;
                partBorders[i].IsEnabled = true;
            }
        }


        // ------------------------------------------------------------
        // ОЧИСТКА ИТОГОВОЙ ОБЛАСТИ КАПЧИ
        // ------------------------------------------------------------
        // Очищает 4 ячейки, куда пользователь собирает изображение.
        // ------------------------------------------------------------
        private void ClearSlots()
        {
            // Убираем картинки из ячеек
            slotImage1.Source = null;
            slotImage2.Source = null;
            slotImage3.Source = null;
            slotImage4.Source = null;

            // Очищаем Tag у ячеек.
            // Tag хранит номер фрагмента, который был помещен в ячейку.
            slot1.Tag = null;
            slot2.Tag = null;
            slot3.Tag = null;
            slot4.Tag = null;

            // Возвращаем обычный вид рамок
            ResetSlotBorders();
        }


        // ------------------------------------------------------------
        // СБРОС РАМОК ЯЧЕЕК КАПЧИ
        // ------------------------------------------------------------
        // Делает рамки итоговых ячеек обычными.
        // ------------------------------------------------------------
        private void ResetSlotBorders()
        {
            Border[] slots = { slot1, slot2, slot3, slot4 };

            foreach (Border slot in slots)
            {
                slot.BorderBrush = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                slot.BorderThickness = new Thickness(1);
            }
        }


        // ------------------------------------------------------------
        // СБРОС РАМОК ФРАГМЕНТОВ КАПЧИ
        // ------------------------------------------------------------
        // Делает рамки всех фрагментов обычными.
        // Используется перед выделением выбранного фрагмента.
        // ------------------------------------------------------------
        private void ResetPartBorders()
        {
            Border[] parts = { part1, part2, part3, part4 };

            foreach (Border part in parts)
            {
                part.BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225));
                part.BorderThickness = new Thickness(1);
            }
        }

        // ------------------------------------------------------------
        // НАЖАТИЕ НА ФРАГМЕНТ КАПЧИ
        // ------------------------------------------------------------
        // Когда пользователь нажимает на один из фрагментов,
        // программа запоминает:
        // 1. Номер выбранной картинки.
        // 2. Саму картинку.
        //
        // После этого пользователь должен нажать на ячейку сверху,
        // куда нужно поместить выбранный фрагмент.
        // ------------------------------------------------------------
        private void Part_Click(object sender, MouseButtonEventArgs e)
        {
            // sender — это элемент, на который нажали.
            // В нашем случае это Border.
            Border selectedBorder = sender as Border;

            // Если почему-то элемент не найден или у него нет Tag,
            // просто выходим из метода.
            if (selectedBorder == null || selectedBorder.Tag == null)
            {
                return;
            }

            // Внутри Border находится Image
            Image image = selectedBorder.Child as Image;

            // Проверяем, что картинка действительно есть
            if (image == null || image.Source == null)
            {
                return;
            }

            // Запоминаем номер выбранного фрагмента
            selectedCaptchaNumber = Convert.ToInt32(selectedBorder.Tag);

            // Запоминаем само изображение
            selectedCaptchaImage = image.Source;

            // Сбрасываем выделение со всех фрагментов
            ResetPartBorders();

            // Подсвечиваем выбранный фрагмент синей рамкой
            selectedBorder.BorderBrush = Brushes.DodgerBlue;
            selectedBorder.BorderThickness = new Thickness(3);
        }


        // ------------------------------------------------------------
        // НАЖАТИЕ НА ЯЧЕЙКУ ИТОГОВОЙ КАПЧИ
        // ------------------------------------------------------------
        // Когда пользователь выбрал фрагмент и нажал на ячейку,
        // выбранное изображение помещается в эту ячейку.
        // ------------------------------------------------------------
        private void Slot_Click(object sender, MouseButtonEventArgs e)
        {
            // Если пользователь не выбрал фрагмент,
            // нельзя вставлять картинку в ячейку.
            if (selectedCaptchaNumber == 0 || selectedCaptchaImage == null)
            {
                MessageBox.Show("Сначала выберите фрагмент капчи.");
                return;
            }

            // Получаем ячейку, на которую нажал пользователь
            Border selectedSlot = sender as Border;

            if (selectedSlot == null)
            {
                return;
            }

            // В ячейке находится Grid.
            // Внутри Grid лежит TextBlock с номером ячейки и Image.
            Grid grid = selectedSlot.Child as Grid;

            if (grid == null)
            {
                return;
            }

            // Найдем Image внутри Grid
            Image slotImage = null;

            foreach (UIElement element in grid.Children)
            {
                if (element is Image)
                {
                    slotImage = element as Image;
                    break;
                }
            }

            if (slotImage == null)
            {
                return;
            }

            // Вставляем выбранную картинку в ячейку
            slotImage.Source = selectedCaptchaImage;

            // В Tag ячейки записываем номер вставленного фрагмента.
            // Например, если вставили картинку 2.png, то Tag = 2.
            selectedSlot.Tag = selectedCaptchaNumber;

            // Сбрасываем рамки всех ячеек
            ResetSlotBorders();

            // Подсвечиваем ячейку, куда только что вставили фрагмент
            selectedSlot.BorderBrush = Brushes.DodgerBlue;
            selectedSlot.BorderThickness = new Thickness(3);
        }


        // ------------------------------------------------------------
        // КНОПКА "ПЕРЕМЕШАТЬ КАПЧУ"
        // ------------------------------------------------------------
        // Полностью перезагружает капчу:
        // 1. Очищает итоговую область.
        // 2. Перемешивает фрагменты заново.
        // ------------------------------------------------------------
        private void btnResetCaptcha_Click(object sender, RoutedEventArgs e)
        {
            LoadCaptcha();
        }


        // ------------------------------------------------------------
        // КНОПКА "ВОЙТИ"
        // ------------------------------------------------------------
        // Основная логика авторизации:
        //
        // 1. Проверяем, заполнены ли логин и пароль.
        // 2. Ищем пользователя в базе по логину.
        // 3. Проверяем, не заблокирован ли пользователь.
        // 4. Проверяем правильность капчи.
        // 5. Проверяем пароль.
        // 6. Если всё правильно — авторизуем пользователя.
        // 7. Если роль "Администратор" — открываем рабочий стол администратора.
        // 8. Если роль "Пользователь" — открываем рабочий стол пользователя.
        // ------------------------------------------------------------
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Получаем логин из TextBox
            string login = txtLogin.Text.Trim();

            // Получаем пароль из PasswordBox
            string password = txtPassword.Password.Trim();

            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Поля Логин и Пароль обязательны для заполнения");
                return;
            }

            // Получаем пользователя из базы данных по логину
            User user = GetUserByLogin(login);

            // Если пользователь не найден
            if (user == null)
            {
                MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
                return;
            }

            // Если пользователь уже заблокирован
            if (user.IsBlocked)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору");
                return;
            }

            // Проверяем капчу.
            // Если капча собрана неверно, увеличиваем счетчик ошибок.
            if (!IsCaptchaCorrect())
            {
                bool blocked = AddFailedAttempt(login);

                if (!blocked)
                {
                    MessageBox.Show("Капча собрана неверно.");
                }

                LoadCaptcha();
                return;
            }

            // Проверяем пароль.
            // Если пароль неверный, увеличиваем счетчик ошибок.
            if (user.Password != password)
            {
                bool blocked = AddFailedAttempt(login);

                if (!blocked)
                {
                    MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
                }

                return;
            }

            // Если логин, пароль и капча верные,
            // сбрасываем количество неудачных попыток
            ResetFailedAttempts(login);

            // Сообщение по ТЗ
            MessageBox.Show("Вы успешно авторизовались");

            // Проверяем роль пользователя
            if (user.Role.Name == "Администратор")
            {
                MessageBox.Show("Открывается рабочий стол администратора");

                // Когда создашь AdminWindow, можно будет раскомментировать:
                //
                // AdminWindow adminWindow = new AdminWindow();
                // adminWindow.Show();
                // this.Close();
            }
            else if (user.Role.Name == "Пользователь")
            {
                MessageBox.Show("Открывается рабочий стол пользователя");

                // Когда создашь UserWindow, можно будет раскомментировать:
                //
                // UserWindow userWindow = new UserWindow();
                // userWindow.Show();
                // this.Close();
            }
            else
            {
                // На случай, если в базе данных указана неправильная роль
                MessageBox.Show("Для пользователя указана неизвестная роль.");
            }
        }


        // ------------------------------------------------------------
        // ПРОВЕРКА ПРАВИЛЬНОСТИ КАПЧИ
        // ------------------------------------------------------------
        // Так как капча собирается квадратом 2x2,
        // правильный порядок должен быть:
        //
        // slot1 = 1.png     slot2 = 2.png
        // slot3 = 3.png     slot4 = 4.png
        //
        // Если во всех ячейках правильные номера,
        // метод вернет true.
        // ------------------------------------------------------------
        private bool IsCaptchaCorrect()
        {
            bool isCorrect =
                slot1.Tag != null && slot1.Tag.ToString() == "1" &&
                slot2.Tag != null && slot2.Tag.ToString() == "2" &&
                slot3.Tag != null && slot3.Tag.ToString() == "3" &&
                slot4.Tag != null && slot4.Tag.ToString() == "4";

            return isCorrect;
        }


        // ------------------------------------------------------------
        // ПОЛУЧЕНИЕ ПОЛЬЗОВАТЕЛЯ ИЗ БАЗЫ ДАННЫХ ПО ЛОГИНУ
        // ------------------------------------------------------------
        // Метод ищет пользователя в таблице "Пользователи".
        //
        // Если пользователь найден — возвращает объект UserInfo.
        // Если пользователь не найден — возвращает null.
        // ------------------------------------------------------------
        private User GetUserByLogin(string login)
        {
            // using нужен для того, чтобы соединение с БД
            // автоматически закрылось после выполнения запроса.
            using (DatabaseContext db = new DatabaseContext())
            {
                User user = db.Users.Where(x => x.Login == login).Include("Role").FirstOrDefault();              
                
                foreach (User us in db.Users.Include("Role"))
                {
                    Console.WriteLine($"{us.Login} {us.Password} {us.Role.Name} {us.IsBlocked} {us.FailledAttemp} {us.FullName}");
                }

                return user;
            }
        }


        // ------------------------------------------------------------
        // ДОБАВЛЕНИЕ НЕУДАЧНОЙ ПОПЫТКИ
        // ------------------------------------------------------------
        // Этот метод вызывается, если:
        // 1. Пользователь неверно ввел пароль.
        // 2. Пользователь неверно собрал капчу.
        //
        // Метод увеличивает FailedAttempts на 1.
        //
        // Если FailedAttempts становится 3 или больше,
        // пользователь блокируется:
        // IsBlocked = 1
        //
        // Метод возвращает:
        // true  — если пользователь был заблокирован
        // false — если пользователь еще не заблокирован
        // ------------------------------------------------------------
        private bool AddFailedAttempt(string login)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                User user = db.Users.Where(x => x.Login == login).FirstOrDefault();
                int errorCount;
                if (user != null)
                {
                    user.FailledAttemp++;
                    errorCount = user.FailledAttemp;
                    db.SaveChanges();
                    if (errorCount >= 3)
                    {
                        user.IsBlocked = true;
                        MessageBox.Show("Учетная запись была заблокирована, из-за большого количества попыток неудачного входа. Обратитесь к администратору.");
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                    //else if (errorCount == 0 && user.IsBlocked)
                    //{
                    //    user.IsBlocked = false;
                    //    db.SaveChanges();
                    //    return false;
                    //}
                }
                else { return false; }

                
            }
        }

        // ------------------------------------------------------------
        // СБРОС НЕУДАЧНЫХ ПОПЫТОК
        // ------------------------------------------------------------
        // Если пользователь успешно прошел авторизацию,
        // FailedAttempts снова становится 0.
        // ------------------------------------------------------------
        private void ResetFailedAttempts(string login)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                User user = db.Users.Where(x => x.Login == login).FirstOrDefault();
                if (user != null)
                {
                    user.FailledAttemp = 0;
                    db.SaveChanges();
                }
            }
        }
    }
}

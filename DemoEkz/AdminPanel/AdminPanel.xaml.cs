using DemoEkz.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
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

namespace DemoEkz.AdminPanel
{
    /// <summary>
    /// Логика взаимодействия для AdminPanel.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        
       

        // ------------------------------------------------------------
        // КОНСТРУКТОР ОКНА АДМИНИСТРАТОРА
        // ------------------------------------------------------------
        // Метод запускается при открытии AdminWindow.
        //
        // InitializeComponent()
        // Загружает интерфейс из файла AdminWindow.xaml.
        //
        // cmbNewRole.SelectedIndex = 1
        // По умолчанию выбирает роль "Пользователь" при добавлении нового пользователя.
        //
        // LoadUsers()
        // Загружает список пользователей из базы данных в таблицу.
        // ------------------------------------------------------------
        public AdminWindow()
        {
            InitializeComponent();

            // Устанавливаем роль по умолчанию — "Пользователь"
            cmbNewRole.SelectedIndex = 1;

            // Загружаем пользователей из базы данных
            LoadUsers();
        }

        // ------------------------------------------------------------
        // ЗАГРУЗКА ПОЛЬЗОВАТЕЛЕЙ ИЗ БАЗЫ ДАННЫХ
        // ------------------------------------------------------------
        // Метод очищает текущий список пользователей и заново загружает
        // актуальные данные из таблицы "Пользователи".
        //
        // Используется:
        // - при открытии окна администратора;
        // - после добавления пользователя;
        // - после изменения данных пользователя;
        // - после снятия блокировки;
        // - при нажатии кнопки "Обновить список".
        // ------------------------------------------------------------
        private void LoadUsers()
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                dataGridUsers.ItemsSource = db.Users.Include("Role").ToList();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }


        // ------------------------------------------------------------
        // КНОПКА "ДОБАВИТЬ"
        // ------------------------------------------------------------
        // Метод добавляет нового пользователя в базу данных.
        //
        // Алгоритм:
        // 1. Получаем логин, пароль и роль из полей формы.
        // 2. Проверяем, что логин и пароль заполнены.
        // 3. Проверяем, существует ли уже пользователь с таким логином.
        // 4. Если логин свободен — добавляем пользователя в таблицу.
        // 5. Обновляем список пользователей.
        // ------------------------------------------------------------
        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string login = txtNewLogin.Text.Trim();
            string password = txtNewPassword.Text.Trim();

            int roleId = Convert.ToInt32((cmbNewRole.SelectedItem as ComboBoxItem).Tag);

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            using (DatabaseContext db = new DatabaseContext())
            {
                if (db.Users.Any(x => x.Login == login))
                {
                    MessageBox.Show("Такой пользователь уже существует");
                    return;
                }

                User user = new User()
                {
                    Login = login,
                    Password = password,
                    RoleID = roleId, // например пользователь
                    IsBlocked = false,
                    FailledAttemp = 0
                };

                db.Users.Add(user);
                db.SaveChanges();
            }

            MessageBox.Show("Пользователь добавлен");

            LoadUsers();
        }

        // ------------------------------------------------------------
        // ПРОВЕРКА СУЩЕСТВОВАНИЯ ЛОГИНА
        // ------------------------------------------------------------
        // Метод проверяет, есть ли в базе пользователь с указанным логином.
        //
        // Используется при добавлении нового пользователя.
        //
        // Возвращает:
        // true  — если пользователь с таким логином уже есть;
        // false — если такого логина нет.
        // ------------------------------------------------------------
        private bool IsLoginExists(string login)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                return db.Users.Any(x => x.Login == login);
            }
        }


        // ------------------------------------------------------------
        // ПРОВЕРКА ЛОГИНА У ДРУГОГО ПОЛЬЗОВАТЕЛЯ
        // ------------------------------------------------------------
        // Метод нужен при изменении существующего пользователя.
        //
        // Например:
        // есть пользователь с Id = 1 и логином admin.
        // Администратор редактирует этого пользователя.
        // Логин admin уже существует, но принадлежит этому же пользователю.
        //
        // Поэтому важно проверять не просто наличие логина,
        // а наличие такого логина у другого пользователя.
        //
        // Возвращает:
        // true  — если такой логин уже есть у другого пользователя;
        // false — если логин свободен или принадлежит текущему пользователю.
        // ------------------------------------------------------------
        private bool IsLoginExistsForAnotherUser(string login, int currentUserId)
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                return db.Users.Any(x =>
                    x.Login == login &&
                    x.Id != currentUserId);
            }
        }


        // ------------------------------------------------------------
        // КНОПКА "СОХРАНИТЬ ИЗМЕНЕНИЯ"
        // ------------------------------------------------------------
        // Метод сохраняет изменения выбранного пользователя.
        //
        // Пользователь редактируется прямо в таблице DataGrid.
        // Можно изменить:
        // - логин;
        // - пароль;
        // - роль;
        // - статус блокировки;
        // - количество ошибок.
        //
        // Алгоритм:
        // 1. Получаем выбранную строку из DataGrid.
        // 2. Проверяем, что пользователь выбран.
        // 3. Завершаем редактирование ячейки DataGrid.
        // 4. Проверяем, что обязательные поля не пустые.
        // 5. Проверяем корректность роли.
        // 6. Проверяем, нет ли дубликата логина.
        // 7. Обновляем пользователя в базе данных.
        // ------------------------------------------------------------
        private void btnSaveSelectedUser_Click(object sender, RoutedEventArgs e)
        {
            dataGridUsers.CommitEdit(DataGridEditingUnit.Cell, true);
            dataGridUsers.CommitEdit(DataGridEditingUnit.Row, true);

            //UserInfo selectedUser = dataGridUsers.SelectedItem as UserInfo;
            var selectedUser = dataGridUsers.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для изменения.");
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedUser.Login) ||
                string.IsNullOrWhiteSpace(selectedUser.Password) /*||
                string.IsNullOrWhiteSpace(selectedUser.Role)*/)
            {
                MessageBox.Show("Логин, пароль и роль не могут быть пустыми.");
                return;
            }

            if (selectedUser.Role.Name != "Администратор" &&
                selectedUser.Role.Name != "Пользователь")    
            {
                MessageBox.Show("Роль должна быть: Администратор или Пользователь.");
                return;
            }

            using (DatabaseContext db = new DatabaseContext())
            {
                var user = db.Users.FirstOrDefault(x => x.Id == selectedUser.Id);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден в базе.");
                    return;
                }

                bool loginExists = db.Users.Any(x =>
                    x.Login == selectedUser.Login &&
                    x.Id != selectedUser.Id);

                if (loginExists)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.");
                    return;
                }

                // обновление данных
                user.Login = selectedUser.Login;
                user.Password = selectedUser.Password;

                // если Role у тебя строкой в UI
                user.Role = db.Roles.FirstOrDefault(r => r.Name == selectedUser.Role.Name);

                user.IsBlocked = selectedUser.IsBlocked;
                user.FailledAttemp = selectedUser.FailledAttemp;

                db.SaveChanges();
            }

            MessageBox.Show("Данные пользователя успешно изменены.");

            LoadUsers();
        }


        private void btnUnblockUser_Click(object sender, RoutedEventArgs e)
        {
            User selectedUser = dataGridUsers.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для снятия блокировки.");
                return;
            }

            using (DatabaseContext db = new DatabaseContext())
            {
                var user = db.Users.FirstOrDefault(x => x.Id == selectedUser.Id);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден.");
                    return;
                }

                user.IsBlocked = false;
                user.FailledAttemp = 0;

                db.SaveChanges();
            }

            MessageBox.Show("Блокировка пользователя снята.");

            LoadUsers();
        }


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            DemiWindow4 loginWindow = new DemiWindow4();
            loginWindow.Show();

            this.Close();
        }
    }
}




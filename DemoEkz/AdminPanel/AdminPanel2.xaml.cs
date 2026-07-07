using DemoEkz.Pages;
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

namespace DemoEkz.AdminPanel
{
    /// <summary>
    /// Логика взаимодействия для AdminPanel2.xaml
    /// </summary>
    public partial class AdminPanel2 : Window
    {
        public AdminPanel2()
        {
            InitializeComponent();
            LoadRoles();
            LoadUsers();

        }

        private void LoadRoles()
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                datagridComboBox.ItemsSource = db.Roles.ToList();
            }
        }
        private void LoadUsers()
        {
            using (DatabaseContext db = new DatabaseContext())
            {
                dataGridUsers.ItemsSource = db.Users.Include("Role").ToList();
            }
        }
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string login = loginBox.Text.Trim();
            string password = passBox.Text.Trim();

            int roleId = Convert.ToInt32((comboBox.SelectedItem as ComboBoxItem).Tag);

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните поля");
                return;
            }

            using (DatabaseContext db = new DatabaseContext())
            {
                if (db.Users.AsEnumerable().Any(x=> x.Login == login))
                {
                    MessageBox.Show("Логин занят");
                    return;
                }

                User user = new User()
                {
                    Login = login,
                    Password = password,
                    RoleID = roleId,
                    FailledAttemp = 0,
                    IsBlocked = false
                };

                db.Users.Add(user);
                db.SaveChanges();
            }

            MessageBox.Show("Пользователь добавлен");
            LoadUsers();
        }

        private void RefreshTable_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            dataGridUsers.CommitEdit(DataGridEditingUnit.Cell,true);
            dataGridUsers.CommitEdit(DataGridEditingUnit.Row,true);

            var selectedUser = dataGridUsers.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для изменения");
                return;
            }

            if (string.IsNullOrEmpty(selectedUser.Login) || string.IsNullOrEmpty(selectedUser.Password))
            {
                MessageBox.Show("Логин\\Пароль не могут быть пустыми");
                return;
            }

            using (DatabaseContext db = new DatabaseContext())
            {
                var user = db.Users.AsEnumerable().FirstOrDefault(x => x.Id == selectedUser.Id);

                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден");
                    return;
                }

                user.Login = selectedUser.Login;
                user.Password = selectedUser.Password;

                
                    user.RoleID = selectedUser.RoleID;

                user.IsBlocked = selectedUser.IsBlocked;
                user.FailledAttemp = selectedUser.FailledAttemp;

                db.SaveChanges();
            }

        }

        private void ClearBlock_Click(object sender, RoutedEventArgs e)
        {
            User selectedUser = dataGridUsers.SelectedItem as User;
            
            using (DatabaseContext db = new DatabaseContext())
            {
                var user = db.Users.FirstOrDefault(x=> x.Id == selectedUser.Id);

                if (user == null)
                {
                    MessageBox.Show("Пользоваатель не найден");
                    return;
                }

                user.IsBlocked = false;
                user.FailledAttemp = 0;

                db.SaveChanges();
            }
            MessageBox.Show("Блокировка снята");
            LoadUsers();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DemiWindow4 window = new DemiWindow4();
            window.Show();

            this.Close();
        }
    }
}

using System;
using System.Windows;
using System.Data.SQLite;
using System.Data;

namespace SimpleSQLiteConnection_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path;
        public MainWindow()
        {
            InitializeComponent();

            // ToDo: Only for debuging. Path to a database.
            path = Environment.CurrentDirectory.Replace(@"\bin\Debug", "");
            path += @"\Players.db";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            PlayerProfile playerProfile = new PlayerProfile();
            playerProfile.Name = tbName.Text;

            int level;
            float experiens;
            if (int.TryParse(tbLevel.Text, out level) &&
                float.TryParse(tbExperience.Text, out experiens))
            {
                playerProfile.Level = level;
                playerProfile.Experience = experiens;
            }
            else
            {
                MessageBox.Show("Enter correct input data", "Information");
                return;
            }

            SavePlayerProfile(playerProfile);
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            string name = tbName.Text;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
            {
                PlayerProfile profile = LoadPlayerProfile(name);
                tbLevel.Text = profile.Level.ToString();
                tbExperience.Text = profile.Experience.ToString();
            }
            else
            {
                MessageBox.Show("Name must not be empty" + name, "Information");
            }
        }

        private void SavePlayerProfile(PlayerProfile profile)
        {
            var command = @"UPDATE profiles SET level = @levelValue, experience = @experienceValue WHERE display_name = @displayNameValue";

            using (var dbConnection = new SQLiteConnection("URI=file:" + path))
            {
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbConnection.Open();

                    dbCommand.CommandText = command;

                    dbCommand.Parameters.Add("@levelValue", DbType.Int32).Value = profile.Level;
                    dbCommand.Parameters.Add("@experienceValue", DbType.Single).Value = profile.Experience;
                    dbCommand.Parameters.Add("@displayNameValue", DbType.String).Value = profile.Name;

                    int entries = dbCommand.ExecuteNonQuery();

                    if (entries == 0)
                    {
                        command = @"INSERT INTO profiles (display_name, level, experience) VALUES (@displayNameValue, @levelValue, @experienceValue)";

                        dbCommand.CommandText = command;
                        dbCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private PlayerProfile LoadPlayerProfile(string displayName)
        {
            var command = @"SELECT * FROM profiles WHERE display_name = @displayNameValue";
            var profile = new PlayerProfile();

            using (var dbConnection = new SQLiteConnection("URI=file:" + path))
            {
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbConnection.Open();

                    dbCommand.CommandText = command;

                    dbCommand.Parameters.Add("@displayNameValue", DbType.String).Value = displayName;

                    using (var dbReader = dbCommand.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            if (dbReader.GetValue(0) != null)
                            {
                                profile.Name = dbReader.GetString(0);
                                profile.Level = dbReader.GetInt32(1);
                                profile.Experience = dbReader.GetFloat(2);
                            }
                        }
                    }
                }
            }
            return profile;
        }
    }
}

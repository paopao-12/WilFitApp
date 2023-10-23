using CalorieTracker;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using System.Windows.Controls;

namespace CalorieTracker
{
    public partial class MainWindow : Window
    {
        Connection con;
        private Timer resetTimer;
        private bool userChangedMeasurement = false;
        private double accumulatedCalories = 0;

        public MainWindow()
        {
            InitializeComponent();
            con = new Connection("Data Source=NITRO5\\SQLEXPRESS01;Integrated Security=True");

            caloriesProgressBar.Value = accumulatedCalories; 
            foodItemTextBox.TextChanged += (sender, e) =>
            {
                if (!userChangedMeasurement)
                {
                    
                    if (foodItemTextBox.Text.ToLower() == "rice")
                    {
                        
                        measurementTextBox.Text = "cups";
                    }
                }
            };

            measurementTextBox.TextChanged += (sender, e) =>
            {
                userChangedMeasurement = true; 
            };
            InitializeResetTimer();
            InitializeWaterResetTimer();
        }

        private void InitializeResetTimer()
        {

            DateTime now = DateTime.Now;
            DateTime nextResetTime = now.Date.AddHours(12);

            if (now > nextResetTime)
            {
                nextResetTime = nextResetTime.AddDays(1);
            }

            double millisecondsUntilReset = (nextResetTime - now).TotalMilliseconds;


            resetTimer = new Timer(millisecondsUntilReset);
            resetTimer.Elapsed += ResetTimer_Elapsed;
            resetTimer.AutoReset = true;
            resetTimer.Start();
        }

        private void ResetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetData();
            InitializeResetTimer();
        }

        private void ResetData()
        {

            using (SqlConnection connection = con.GetConnection())
            {
                connection.Open();


                string resetQuery = "DELETE FROM FoodLogg;";

                using (SqlCommand command = new SqlCommand(resetQuery, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Failed to reset data in the database: " + ex.Message);
                    }
                }
            }
            caloriesProgressBar.Value = 0;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void AddEntryButton_Click(object sender, RoutedEventArgs e)
        {
            string foodItem = foodItemTextBox.Text;
            string servingSizeText = servingSizeTextBox.Text;
            string measurement = measurementTextBox.Text;

            if (foodItem.ToLower().Contains("rice"))
            {
                
                measurement = "cups";
                measurementTextBox.Text = measurement;
            }

            if (!double.TryParse(caloriesPerServingTextBox.Text, out double caloriesPerServing))
            {
                MessageBox.Show("Invalid input for Calories per Serving.");
                return;
            }

            if (!double.TryParse(servingSizeText, out double servingSize))
            {
                MessageBox.Show("Invalid input for Serving Size.");
                return;
            }

            double totalCalories = servingSize * caloriesPerServing;
            accumulatedCalories += totalCalories;
            MessageBox.Show($"You have a total of {totalCalories} calories.");

            using (SqlConnection connection = con.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO FoodLogg (FoodItem, ServingSize, Measurement, CaloriesPerServing) " +
                                    "VALUES (@FoodItem, @ServingSize, @Measurement, @CaloriesPerServing)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@FoodItem", foodItem);
                    command.Parameters.AddWithValue("@ServingSize", servingSize);
                    command.Parameters.AddWithValue("@Measurement", measurement);
                    command.Parameters.AddWithValue("@CaloriesPerServing", caloriesPerServing);

                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Entry added to the database.");

                        
                        double totalCaloriesConsumed = CalculateTotalCaloriesConsumed();
                        caloriesProgressBar.Value = accumulatedCalories;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to add entry to the database.");
                    }
                }
            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            DataGridStyle dataGridStyle = new DataGridStyle();
            dataGridStyle.ShowDialog();
        }
        private double CalculateTotalCaloriesConsumed()
        {
            double totalCaloriesConsumed = 0;

            try
            {
                using (SqlConnection connection = con.GetConnection())
                {
                    connection.Open();


                    string query = "SELECT SUM(CaloriesPerServing) FROM FoodLogg";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {

                        object result = command.ExecuteScalar();

                        if (result != DBNull.Value)
                        {

                            totalCaloriesConsumed = Convert.ToDouble(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Failed to retrieve total calories consumed: " + ex.Message);
            }

            return totalCaloriesConsumed;
        }
        private void AddWaterEntryButton_Click(object sender, RoutedEventArgs e)
        {
            string waterAmountText = waterAmountTextBox.Text;
            string measurement = waterMeasurementTextBox.Text;

            if (!double.TryParse(waterAmountText, out double waterAmount))
            {
                MessageBox.Show("Invalid input for Water Amount.");
                return;
            }

            MessageBox.Show($"You have consumed {waterAmount} ml of water.");

            using (SqlConnection connection = con.GetConnection())
            {
                connection.Open();

                string insertQuery = "INSERT INTO WaterLog (Date, Amount, Measurement) " +
                                    "VALUES (@Date, @Amount, @Measurement)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Date", waterDatePicker.SelectedDate);
                    command.Parameters.AddWithValue("@Amount", waterAmount);
                    command.Parameters.AddWithValue("@Measurement", measurement);

                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Water entry added to the database.");

                        // Update the progress bar value after adding the entry
                        double totalWaterConsumed = CalculateTotalWaterConsumed();
                        waterProgressBar.Value = totalWaterConsumed;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to add water entry to the database.");
                    }
                }
            }
        }

        private double CalculateTotalWaterConsumed()
        {
            double totalWaterConsumed = 0;

            try
            {
                using (SqlConnection connection = con.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT SUM(Amount) FROM WaterLog";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();

                        if (result != DBNull.Value)
                        {
                            totalWaterConsumed = Convert.ToDouble(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to retrieve total water consumed: " + ex.Message);
            }

            return totalWaterConsumed;
        }
        private void ResetWaterData()
        {
            using (SqlConnection connection = con.GetConnection())
            {
                connection.Open();

                string resetQuery = "DELETE FROM WaterLog;";

                using (SqlCommand command = new SqlCommand(resetQuery, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to reset water data in the database: " + ex.Message);
                    }
                }
            }
            waterProgressBar.Value = 0;
        }
        private void InitializeWaterResetTimer()
        {
            DateTime now = DateTime.Now;
            DateTime nextResetTime = now.Date.AddHours(24);

            if (now > nextResetTime)
            {
                nextResetTime = nextResetTime.AddDays(1);
            }

            double millisecondsUntilReset = (nextResetTime - now).TotalMilliseconds;

            resetTimer = new Timer(millisecondsUntilReset);
            resetTimer.Elapsed += WaterResetTimer_Elapsed;
            resetTimer.AutoReset = true;
            resetTimer.Start();
        }
        private void WaterResetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetWaterData();
            InitializeWaterResetTimer();
        }
        private void CalculateCalorieSurplusDeficit(object sender, RoutedEventArgs e)
        {
            // Check if the input is the placeholder text
            if (dailyCalorieInputTextBox.Text != "Input calories")
            {
                if (double.TryParse(dailyCalorieInputTextBox.Text, out double dailyCalorieIntake))
                {
                    UserData userData = FetchUserDataFromDatabase("Pao");
                    if (userData != null)
                    {
                        double dailyCalorieNeeds = CalculateDailyCalorieNeeds(userData);
                        DisplayCalorieSurplusDeficit(dailyCalorieIntake, dailyCalorieNeeds);
                    }
                    else
                    {
                        resultLabel.Text = "User data not found in the database.";
                    }
                }
                else
                {
                    resultLabel.Text = "Invalid input for daily calorie intake.";
                }
            }
            else
            {
                resultLabel.Text = "Please enter your daily calorie intake.";
            }
        }

        private void DisplayCalorieSurplusDeficit(double dailyCalorieIntake, double dailyCalorieNeeds)
        {
            if (dailyCalorieNeeds != -1)
            {
                double calorieSurplusDeficit = dailyCalorieIntake - dailyCalorieNeeds;
                if (calorieSurplusDeficit > 0)
                {
                    resultLabel.Text = $"You have a calorie surplus of {calorieSurplusDeficit} calories. Consider reducing your intake.";
                }
                else if (calorieSurplusDeficit < 0)
                {
                    resultLabel.Text = $"You have a calorie deficit of {Math.Abs(calorieSurplusDeficit)} calories. Consider increasing your intake.";
                }
                else
                {
                    resultLabel.Text = "You are meeting your daily calorie needs. Well done!";
                }
            }
            else
            {
                resultLabel.Text = "User data not found in the database.";
            }
        }

        private UserData FetchUserDataFromDatabase(string username)
        {
            using (SqlConnection connection = new SqlConnection("Data Source=NITRO5\\SQLEXPRESS01;Initial Catalog=OOP_User;Integrated Security=True"))
            {
                connection.Open();

                string query = "SELECT age, weight, height, gender, ActivityLevel FROM UserInfo WHERE userName = @Username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UserData userData = new UserData();

                            if (double.TryParse(reader["age"].ToString(), out double age))
                            {
                                userData.age = age;
                            }

                            if (double.TryParse(reader["weight"].ToString(), out double weight))
                            {
                                userData.weight = weight;
                            }

                            if (double.TryParse(reader["height"].ToString(), out double height))
                            {
                                userData.height = height;
                            }

                            userData.gender = reader["gender"].ToString();

                            if (double.TryParse(reader["ActivityLevel"].ToString(), out double activityLevel))
                            {
                                userData.ActivityLevel = activityLevel;
                            }

                            return userData;
                        }
                        else
                        {
                            return null; 
                        }
                    }
                }
            }
        }


        private double CalculateDailyCalorieNeeds(UserData userData)
        {
            double BMR;
            if (userData.gender == "Male")
            {
                // For males
                BMR = 88.362 + (13.397 * userData.weight) + (4.799 * userData.height) - (5.677 * userData.age);
            }
            else if (userData.gender == "Female")
            {
                // For females
                BMR = 447.593 + (9.247 * userData.weight) + (3.098 * userData.height) - (4.330 * userData.age);
            }
            else
            {
                BMR = 0; 
            }

            double activityMultiplier = 1.2; 

            switch (userData.ActivityLevel)
            {
                case 1: 
                    activityMultiplier = 1.2;
                    break;
                case 2: 
                    activityMultiplier = 1.375;
                    break;
                case 3: 
                    activityMultiplier = 1.55;
                    break;
                case 4: 
                    activityMultiplier = 1.725;
                    break;
                case 5: 
                    activityMultiplier = 1.9;
                    break;
                default:
                    activityMultiplier = 1.2; 
                    break;
            }

           
            double dailyCalorieNeeds = BMR * activityMultiplier;

            return dailyCalorieNeeds;
        }
        private void foodItemTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (foodItemTextBox.Text.ToLower().Contains("rice"))
            {
                
                measurementTextBox.Text = "cups";
            }
            else
            {
                
                measurementTextBox.Clear();
            }
        }

    }
}